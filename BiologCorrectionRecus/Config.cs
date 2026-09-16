namespace BiologCorrectionRecus;

/// <summary>
/// Paramètres métier propres à Biolog (tables et colonnes des reçus, règles PEDIAT), à
/// ajuster selon votre installation.
///
/// La connexion au serveur HFSQL (serveur, base, provider OLEDB, identifiants) ainsi que la
/// table des utilisateurs ne vivent plus ici : elles sont désormais dans
/// <see cref="BiologCorrectionRecus.Configuration.AppConfig"/> (chargée depuis appsettings.json), pour
/// rester la même dans tous les projets Biolog qui se connectent à HFSQL — voir
/// Configuration/AppConfig.cs et Data/HfsqlConnectionManager.cs.
/// </summary>
internal static class Config
{
    // ------------------------------------------------------------------
    // 1) Noms des tables / colonnes HFSQL utilisées par l'application
    //    (regroupés ici pour être faciles à corriger si le schéma réel diffère)
    // ------------------------------------------------------------------

    public const string TableVentes = "VenteClinique";
    public const string ColVenteId = "IDVente";
    public const string ColVenteDate = "DateVente";
    public const string ColVenteCodeClient = "CodeClient";
    public const string ColVenteEstCorrige = "EstCorrige";
    public const string ColVentePdfOriginal = "PdfRecu";
    public const string ColVentePdfCorrige = "PdfRecuCorrige";

    public const string TableLignesAchat = "A_Acheté";
    public const string ColLigneId = "IDLigne";
    public const string ColLigneVenteId = "IDVente";
    public const string ColLigneCodeArticle = "CodeArticle";
    public const string ColLigneDesignation = "Désignation";
    public const string ColLigneIdentitePrestataire = "IdentitéPrestataire";
    public const string ColLigneDateEmission = "DateEmission";

    public const string TableMedecins = "MédecinT";
    public const string ColMedecinId = "IDMédecin";
    public const string ColMedecinNom = "NomMédecin";
    public const string ColMedecinPrenom = "PrénomMédecin";

    // ------------------------------------------------------------------
    // 2) Règles métier
    // ------------------------------------------------------------------

    /// <summary>Préfixe/motif identifiant une ligne "pédiatrie" à corriger dans CodeArticle ou Désignation.</summary>
    public const string MotifCodePediatrie = "PEDIAT";

    /// <summary>
    /// Caractère joker (wildcard) utilisé dans les requêtes SQL "LIKE".
    /// SQL standard = '%'. Si HFSQL/OLEDB se comporte comme Access/Jet, remplacer par '*'
    /// (hypothèse 2 du README). Centralisé ici pour ne changer qu'une seule ligne.
    /// </summary>
    public const string JokerSql = "%";

    /// <summary>Nombre de jours de validité ajoutés à la date d'émission pour une ligne pédiatrie.</summary>
    public const int NombreJoursValiditePediatrie = 10;

    /// <summary>Nombre de reçus récents chargés dans la grille au démarrage.</summary>
    public const int NombreRecusAffiches = 200;
}
