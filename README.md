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
  Config.cs                    → tous les paramètres à ajuster (serveur, base, tables, colonnes)
  Program.cs                   → point d'entrée
  Modeles/
    Utilisateur.cs              → une ligne de UtilisateurBlg
    Recu.cs                     → une ligne de VenteClinique
    LigneRecu.cs                → une ligne de A_Acheté (+ détection PEDIAT)
  Data/
    HfsqlConnexion.cs           → construction de la chaîne de connexion OLEDB
    RepositoryUtilisateurs.cs   → authentification
    RepositoryRecus.cs          → lecture des reçus/lignes + écriture du PDF corrigé (blob)
  Services/
    ServiceCorrectionPdf.cs     → génère le PDF corrigé avec PdfSharpCore
    ServiceVerification.cs      → orchestre le scan + la correction de tous les reçus
  Formulaires/
    FormConnexion.cs/.Designer.cs   → écran de connexion
    FormPrincipal.cs/.Designer.cs   → grille des reçus, boutons, barre de statut
```

## Avant de lancer : vérifier 4 hypothèses techniques

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

4. **Nom exact de la base** (`Config.HfsqlBase = "Biolog"`), du Provider
   OLEDB (`Config.HfsqlProviderOleDb = "HFSQLOLEDB"`) et identifiants de
   connexion — à ajuster dans `Config.cs`. Toutes les valeurs à vérifier
   sont regroupées en haut de ce fichier.

## Ordre de test recommandé
1. Lancer l'appli → écran de connexion → vérifier l'authentification
   contre `UtilisateurBlg`.
2. Une fois connecté, vérifier que la grille se remplit (liste des reçus,
   tri décroissant).
3. Cliquer sur "Vérifier maintenant" et observer la barre de statut.
4. Sur un reçu contenant une ligne PEDIAT, vérifier qu'un PDF corrigé est
   bien généré et que sa date de validité de ligne est correcte (émission + 10j).
5. Double-cliquer sur ce reçu → vérifier que c'est bien la version corrigée
   qui s'ouvre (et non l'originale).
6. Tester le bouton "Imprimer le reçu sélectionné" sur une imprimante de test.

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
