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
    AppConfig.cs                → connexion HFSQL (lue depuis appsettings.json) + session en cours
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
    LoginForm.cs/.Designer.cs       → écran de connexion (ComboBox utilisateur + mot de passe)
    FormPrincipal.cs/.Designer.cs   → grille des reçus, boutons, barre de statut
```

## Connexion HFSQL : solution commune à tous les projets Biolog

La connexion au serveur HFSQL (`Configuration/AppConfig.cs` +
`Data/HfsqlConnectionManager.cs` + `Formulaires/LoginForm.cs` +
`LoginApplicationContext.cs`) reproduit exactement l'architecture validée
dans le projet `HFSQL_LoginApp` (dépôt `Claude`), adaptée à OLEDB :

- Tous les paramètres de connexion (serveur, base, provider OLEDB,
  identifiants) et la description de la table des utilisateurs sont dans
  **`appsettings.json`**, pas codés en dur — modifiable sans recompiler.
- `AppConfig.Charger()` (appelé une fois dans `Program.cs`) lit ce fichier
  et alimente les propriétés statiques de `AppConfig`.
- `HfsqlConnectionManager` centralise toutes les connexions OLEDB
  (authentification comme accès aux données métier des reçus).
- `LoginForm` affiche une ComboBox d'utilisateurs (chargée depuis
  `UtilisateurBlg`), un champ mot de passe, et limite à 3 tentatives.
- `LoginApplicationContext` pilote l'enchaînement : connexion réussie →
  fermeture de `LoginForm` → ouverture de `FormPrincipal` (le menu
  principal) avec l'utilisateur connecté.

**Pour tout futur projet Biolog nécessitant HFSQL** : copier ces 4
fichiers (`Configuration/AppConfig.cs`, `Data/HfsqlConnectionManager.cs`,
`Formulaires/LoginForm.cs`/`.Designer.cs`, `LoginApplicationContext.cs`)
et `appsettings.json`, puis adapter uniquement la table des utilisateurs
et l'écran affiché après connexion.

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
connexion ne sont plus dans `Config.cs` : ils sont dans **`appsettings.json`**
(voir section précédente) — c'est ce fichier qu'il faut ajuster.

## Ordre de test recommandé
1. Ajuster `appsettings.json` (serveur, base, provider OLEDB, table des
   utilisateurs) selon votre installation HFSQL réelle.
2. Lancer l'appli → écran de connexion (LoginForm) → vérifier que la
   ComboBox se remplit avec les utilisateurs de `UtilisateurBlg`.
3. Sélectionner un utilisateur, saisir son mot de passe → vérifier
   l'authentification (et le message après un mot de passe incorrect).
4. Une fois connecté, vérifier l'ouverture de FormPrincipal et que la
   grille se remplit (liste des reçus, tri décroissant).
5. Cliquer sur "Vérifier maintenant" et observer la barre de statut.
6. Sur un reçu contenant une ligne PEDIAT, vérifier qu'un PDF corrigé est
   bien généré et que sa date de validité de ligne est correcte (émission + 10j).
7. Double-cliquer sur ce reçu → vérifier que c'est bien la version corrigée
   qui s'ouvre (et non l'originale).
8. Tester le bouton "Imprimer le reçu sélectionné" sur une imprimante de test.

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
