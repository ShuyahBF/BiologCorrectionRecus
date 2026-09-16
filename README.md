# Biolog - Correction des reçus (WinForms, VS2026)

## Ouverture du projet
Ouvrir `BiologCorrectionRecus.sln` dans Visual Studio Community 2026.
Au premier build, VS restaure automatiquement les 2 packages NuGet
(`System.Data.OleDb` et `PdfSharpCore`). Sinon : clic droit sur la
solution → *Restaurer les packages NuGet*.

**Ce projet n'a pas pu être compilé de mon côté** (pas de SDK .NET dans
mon environnement d'exécution, un conteneur Linux distant) — je l'ai
écrit et relu avec soin, mais c'est à toi de faire le premier build et
de me remonter les éventuelles erreurs, je corrigerai directement.

## Structure du code

```
BiologCorrectionRecus/
  appsettings.json             → connexion HFSQL + table des utilisateurs (copié à côté de l'exe)
  Config.cs                    → paramètres métier (tables/colonnes des reçus, règles PEDIAT)
  Program.cs                   → point d'entrée (charge AppConfig, lance LoginApplicationContext)
  LoginApplicationContext.cs   → enchaîne LoginForm -> FormPrincipal une fois authentifié
  Configuration/
    AppConfig.cs                → connexion HFSQL (appsettings.json + fichier .ini) + session en cours
    IniFileReader.cs             → lecteur générique de fichier .ini ([Section] Cle=Valeur)
  Modeles/
    Utilisateur.cs              → une ligne de UtilisateurBlg
    Recu.cs                     → une ligne de VenteClinique
    LigneRecu.cs                → une ligne de A_Acheté (+ détection PEDIAT)
  Data/
    HfsqlConnectionManager.cs   → connexion OLEDB + chargement utilisateurs + authentification
    RepositoryRecus.cs          → lecture des reçus/lignes + écriture du PDF corrigé (blob)
  Services/
    ServiceCorrectionPdf.cs     → génère le PDF corrigé avec PdfSharpCore
    ServiceVerification.cs      → orchestre le scan + la correction de tous les reçus
  Formulaires/
    LoginForm.cs/.Designer.cs               → écran de connexion (ComboBox utilisateur + mot de passe)
    FormParametresLogiciel.cs/.Designer.cs  → admin : nom du logiciel + emplacement du .ini
    FormPrincipal.cs/.Designer.cs           → grille des reçus, boutons, barre de statut
```

## Connexion HFSQL : solution commune à tous les projets (Biolog, Aizenta, eKol...)

La connexion au serveur HFSQL (`Configuration/AppConfig.cs` +
`Data/HfsqlConnectionManager.cs` + `Formulaires/LoginForm.cs` +
`LoginApplicationContext.cs`) reproduit l'architecture validée dans le
projet `HFSQL_LoginApp` (dépôt `Claude`), adaptée à OLEDB et à votre
logique habituelle de fichier d'initialisation (`.ini`) par logiciel.

La configuration se fait en **deux étages** :

1. **`appsettings.json`** (à côté de l'exécutable) : paramètres
   "techniques" par défaut (provider OLEDB, identifiants du compte
   technique, table des utilisateurs), ainsi que le **nom du logiciel**
   et l'**emplacement de son fichier `.ini`** (section `Logiciel`).
2. **Le fichier `.ini` du logiciel** (ex : `C:\BOOT_Biolog\Biolog.ini`),
   propre à chaque poste et modifiable par un administrateur — c'est lui
   qui donne la vraie ressource à utiliser. Convention reprise de vos
   autres logiciels (Aizenta, eKol...) : le nom de la base HFSQL est lu
   dans la section `[Serveur]`, clé `Nom`. Quand ce fichier est
   accessible, sa valeur remplace celle d'`appsettings.json`.

Fonctionnement :

- `AppConfig.Charger()` (appelé une fois dans `Program.cs`) lit d'abord
  `appsettings.json`, puis relit aussitôt le fichier `.ini` pointé
  (`AppConfig.ChargerDepuisFichierIni()`).
- `HfsqlConnectionManager` centralise toutes les connexions OLEDB
  (authentification comme accès aux données métier des reçus), à partir
  de la configuration ainsi obtenue.
- `LoginForm` affiche une ComboBox d'utilisateurs (chargée depuis
  `UtilisateurBlg`), un champ mot de passe, limite à 3 tentatives, et
  affiche l'éventuelle erreur de lecture du `.ini` (fichier introuvable,
  clé absente...).
- Un lien discret **"⚙ Paramètres du logiciel"** en bas de l'écran de
  connexion ouvre `FormParametresLogiciel` : un administrateur y modifie
  le **nom du logiciel** et l'**emplacement du fichier `.ini`**. À
  l'enregistrement, ces deux valeurs sont écrites dans `appsettings.json`
  (`AppConfig.Enregistrer()`), le `.ini` est relu immédiatement, et la
  liste des utilisateurs se recharge avec la nouvelle base HFSQL — sans
  recompiler ni relancer l'application.
- `LoginApplicationContext` pilote l'enchaînement : connexion réussie →
  fermeture de `LoginForm` → ouverture de `FormPrincipal` (le menu
  principal) avec l'utilisateur connecté.

**Pour tout futur projet (Aizenta, eKol...) nécessitant HFSQL** : copier
ces 6 fichiers (`Configuration/AppConfig.cs`, `Configuration/IniFileReader.cs`,
`Data/HfsqlConnectionManager.cs`, `Formulaires/LoginForm.cs`/`.Designer.cs`,
`Formulaires/FormParametresLogiciel.cs`/`.Designer.cs`,
`LoginApplicationContext.cs`) et `appsettings.json`, puis adapter
uniquement la table des utilisateurs, la valeur par défaut de
`Logiciel.Nom`/`Logiciel.CheminFichierIni`, et l'écran affiché après
connexion.

⚠️ Seule la clé `[Serveur] Nom` (→ nom de la base) est câblée pour
l'instant, car c'est la seule confirmée. Si votre fichier `.ini` expose
d'autres paramètres à reprendre (adresse du serveur, port...), indiquez-
moi leurs noms exacts de section/clé et j'étendrai
`AppConfig.ChargerDepuisFichierIni()` de la même façon.

## Avant de lancer : vérifier 3 hypothèses techniques

1. **Jointure médecin** (`RepositoryRecus.ObtenirLignesDuRecu`) — j'ai
   supposé que `A_Acheté.IdentitéPrestataire` correspond à
   `MédecinT.IDMédecin`. Si le nom du médecin n'apparaît pas correctement
   sur le PDF généré, c'est probablement ce champ à corriger.

2. **Wildcard SQL** — la constante `Config.JokerSql` vaut `%` (syntaxe
   SQL standard). Si HFSQL/OLEDB attend plutôt `*` (comme Access/Jet),
   il faudra changer cette seule constante.

3. **Écriture de blob (PDF) via OLEDB** — c'est le point le plus à risque :
   HFSQL a déjà montré des comportements particuliers avec les drivers
   externes (bug pyodbc rencontré précédemment). Teste
   `RepositoryRecus.EnregistrerPdfCorrige` en priorité, sur UN seul reçu
   de test, avant de laisser tourner le service en continu. Le paramètre
   binaire est typé `OleDbType.LongVarBinary` dans le code — si le pilote
   HFSQL le refuse, essayer `OleDbType.VarBinary`.

Le nom exact de la base, du Provider OLEDB et les identifiants de
connexion ne sont plus dans `Config.cs` : le nom de la base vient en
priorité du fichier `.ini` du logiciel (s'il est accessible), sinon
d'**`appsettings.json`** (voir section précédente) ; les autres valeurs
(provider, identifiants) restent dans `appsettings.json`.

## Ordre de test recommandé
1. Ajuster `appsettings.json` : section `HFSQL` (serveur, provider OLEDB,
   identifiants, table des utilisateurs) et section `Logiciel` (nom du
   logiciel, emplacement du fichier `.ini` réel sur le poste).
2. Lancer l'appli → écran de connexion (LoginForm) → si le `.ini` est
   introuvable ou incomplet, un message s'affiche : cliquer sur
   "⚙ Paramètres du logiciel" pour corriger le chemin sans recompiler.
3. Vérifier que la ComboBox se remplit avec les utilisateurs de
   `UtilisateurBlg` (donc que la base indiquée par le `.ini` est la bonne).
4. Sélectionner un utilisateur, saisir son mot de passe → vérifier
   l'authentification (et le message après un mot de passe incorrect).
5. Une fois connecté, vérifier l'ouverture de FormPrincipal et que la
   grille se remplit (liste des reçus, tri décroissant).
6. Cliquer sur "Vérifier maintenant" et observer la barre de statut.
7. Sur un reçu contenant une ligne PEDIAT, vérifier qu'un PDF corrigé est
   bien généré et que sa date de validité de ligne est correcte (émission + 10j).
8. Double-cliquer sur ce reçu → vérifier que c'est bien la version corrigée
   qui s'ouvre (et non l'originale).
9. Tester le bouton "Imprimer le reçu sélectionné" sur une imprimante de test.

## Ce qui est géré différemment de la version initiale
- **Plusieurs postes caissiers ouverts en même temps** : une protection
  minimale a été ajoutée dans `RepositoryRecus.EnregistrerPdfCorrige`
  (clause `WHERE EstCorrige = 0` en plus du filtre sur l'ID). Si deux
  postes tentent de corriger le même reçu au même moment, seul le premier
  UPDATE aboutira ; le second ne touchera aucune ligne et sera simplement
  journalisé comme "déjà corrigé entre-temps". Ce n'est pas un verrou
  transactionnel complet, mais cela évite la double écriture de PDF dans
  le cas courant.

## Ce qui n'est PAS encore géré (à discuter si besoin)
- Lien direct vers le patient (nom/prénom) sur le PDF corrigé : je n'ai
  pas trouvé de correspondance fiable entre `VenteClinique.CodeClient`
  et `Patient` dans les échantillons fournis (formats différents). Dis-moi
  le bon champ si tu veux que le nom du patient apparaisse aussi — il
  suffira d'ajouter la jointure dans `RepositoryRecus` et une ligne dans
  `ServiceCorrectionPdf`.
- Le PDF corrigé est **régénéré** (nouvelle mise en page simple : article,
  médecin, date d'émission, date de validité) plutôt que d'être un
  "patch" de l'original — plus fiable à produire avec PdfSharpCore, mais
  la mise en page ne reprend pas le graphisme exact du reçu d'origine.
  Si tu as un modèle de mise en page à respecter, dis-le moi.
