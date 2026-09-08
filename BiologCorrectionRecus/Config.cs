namespace BiologCorrectionRecus;

/// <summary>
/// Tous les paramètres "à ajuster selon votre installation" sont centralisés ici.
/// Objectif : ne jamais avoir à modifier le code métier pour changer un nom de
/// serveur, de base ou de table — on ne touche qu'à cette classe.
/// </summary>
internal static class Config
{
    // ------------------------------------------------------------------
    // 1) Connexion HFSQL (via le pilote OLEDB fourni par PCSoft)
    // ------------------------------------------------------------------

    /// <summary>Nom ou adresse IP du serveur HFSQL (moteur client/serveur), ex: "localhost" ou "192.168.1.10".</summary>
    public const string HfsqlServeur = "localhost";

    /// <summary>Nom de la base HFSQL. À VÉRIFIER en priorité (hypothèse 4 du README).</summary>
    public const string HfsqlBase = "Biolog";

    /// <summary>Identifiant de connexion HFSQL (compte technique, pas celui d'un utilisateur de l'appli).</summary>
    public const string HfsqlUtilisateur = "admin";

    /// <summary>Mot de passe du compte HFSQL ci-dessus.</summary>
    public const string HfsqlMotDePasse = "";

    /// <summary>
    /// Nom du "Provider" OLEDB à utiliser. C'est la valeur la plus susceptible de varier
    /// selon la version du pilote HFSQL installée sur le poste (32/64 bits, version du moteur).
    /// Valeurs courantes à essayer si la connexion échoue : "HFSQLOLEDB", "PCSoft.HFSQL.1".
    /// Voir Data/HfsqlConnexion.cs pour la construction complète de la chaîne de connexion.
    /// </summary>
    public const string HfsqlProviderOleDb = "HFSQLOLEDB";

    // ------------------------------------------------------------------
    // 2) Noms des tables / colonnes HFSQL utilisées par l'application
    //    (regroupés ici pour être faciles à corriger si le schéma réel diffère)
    // ------------------------------------------------------------------

    public const string TableUtilisateurs = "UtilisateurBlg";
    public const string ColUtilisateurId = "IDUtilisateur";
    public const string ColUtilisateurLogin = "Login";
    public const string ColUtilisateurMotDePasse = "MotDePasse";
    public const string ColUtilisateurNomComplet = "NomComplet";

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
    // 3) Règles métier
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
