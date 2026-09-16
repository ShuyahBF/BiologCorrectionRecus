using System;
using System.IO;
using System.Text.Json;
using BiologCorrectionRecus.Modeles;

namespace BiologCorrectionRecus.Configuration
{
    /// <summary>
    /// Variables globales de connexion au serveur HFSQL, communes à toute l'application.
    /// Reprend la même architecture que le projet de référence "HFSQL_LoginApp" (dépôt Claude) :
    /// paramètres externalisés dans appsettings.json plutôt qu'écrits en dur dans le code, pour
    /// pouvoir être ajustés sans recompiler. Connexion via le pilote OLEDB HFSQL
    /// (System.Data.OleDb), conformément à votre configuration actuelle.
    ///
    /// La configuration se fait maintenant en deux étages, comme pour vos autres logiciels
    /// (Aizenta, eKol, ...) :
    /// 1. <b>appsettings.json</b> (à côté de l'exécutable) : paramètres "techniques" par défaut
    ///    (provider OLEDB, identifiants du compte technique, table des utilisateurs) + le nom du
    ///    logiciel et l'emplacement du fichier d'initialisation (.ini) de ce logiciel.
    /// 2. <b>Le fichier .ini du logiciel</b> (ex : C:\BOOT_Biolog\Biolog.ini), propre à chaque
    ///    poste/installation et modifiable par un administrateur : c'est lui qui donne la vraie
    ///    ressource à utiliser, dans la section [Serveur] :
    ///    - clé "Nom" -> adresse du serveur HFSQL, éventuellement suivie de ":port"
    ///      (ex : "XRVEUR" ou "XRVEUR:4900" — s'il y a ":" suivi d'un nombre, c'est le port) ;
    ///    - clé "NomBaseDonnées" -> nom de la base HFSQL à ouvrir sur ce serveur.
    ///    Ses valeurs, quand présentes, remplacent celles d'appsettings.json.
    ///
    /// C'est cette classe (et non plus Config.cs) qui porte désormais la connexion HFSQL ;
    /// Config.cs ne garde que les constantes métier propres à Biolog (tables des reçus,
    /// règles PEDIAT, etc.).
    /// </summary>
    public static class AppConfig
    {
        // ----- Paramètres de connexion au serveur HFSQL -----
        public static string ServeurHFSQL { get; set; } = "localhost";

        /// <summary>
        /// Port du serveur HFSQL, s'il est connu (ex : 4900). Peut venir d'appsettings.json ou
        /// être extrait de la clé [Serveur] "Nom" du fichier .ini si elle est écrite sous la
        /// forme "Adresse:Port" (ex : "XRVEUR:4900"). Null si non renseigné.
        /// </summary>
        public static int? PortHFSQL { get; set; }

        public static string NomBaseDeDonnees { get; set; } = "Biolog";

        // Nom exact du provider OLEDB HFSQL installé sur le poste (fourni par PCSoft avec
        // HFSQL Client/Serveur). Valeurs courantes : "HFSQLOLEDB", "PCSoft.HFSQL.1".
        public static string NomProviderOleDb { get; set; } = "HFSQLOLEDB";

        public static string UtilisateurConnexion { get; set; } = "admin";
        public static string MotDePasseConnexion { get; set; } = "";
        public static int TimeoutConnexionSecondes { get; set; } = 10;

        // ----- Description de la table des utilisateurs (à adapter à votre structure réelle) -----
        public static string TableUtilisateurs { get; set; } = "UtilisateurBlg";
        public static string ColonneId { get; set; } = "IDUtilisateur";
        public static string ColonneLogin { get; set; } = "Login";
        public static string ColonneMotDePasse { get; set; } = "MotDePasse";
        public static string ColonneNomComplet { get; set; } = "NomComplet";

        // ----- Identité du logiciel + emplacement de son fichier .ini -----
        // Modifiables depuis l'écran de connexion (bouton "Paramètres du logiciel", voir
        // Formulaires/FormParametresLogiciel.cs) par un administrateur, sans avoir à éditer
        // appsettings.json à la main.
        public static string NomLogiciel { get; set; } = "Biolog";
        public static string CheminFichierIni { get; set; } = @"C:\BOOT_Biolog\Biolog.ini";

        /// <summary>
        /// Dernier message d'erreur rencontré en lisant le fichier .ini (vide si tout s'est bien
        /// passé). Affiché par la fenêtre de connexion si la lecture échoue, pour guider
        /// l'administrateur (chemin incorrect, fichier absent, section/clé manquante...).
        /// </summary>
        public static string DerniereErreurIni { get; private set; } = string.Empty;

        // ----- Session en cours -----
        public static Utilisateur? UtilisateurConnecte { get; set; }

        /// <summary>
        /// Chaîne de connexion OLEDB construite à partir des paramètres ci-dessus. Utilisée par
        /// Data/HfsqlConnectionManager.cs pour toutes les connexions au serveur HFSQL
        /// (authentification comme accès aux données métier des reçus).
        /// Le port (quand connu) est ajouté après le serveur sous la forme "Serveur:Port" dans
        /// "Data Source" — à ajuster si le pilote OLEDB HFSQL attend une autre syntaxe (ex : un
        /// paramètre "Server Port=" séparé), non confirmée pour l'instant.
        /// </summary>
        public static string ChaineConnexion =>
            $"Provider={NomProviderOleDb};" +
            $"Data Source={ServeurHFSQL}{(PortHFSQL.HasValue ? ":" + PortHFSQL.Value : string.Empty)};" +
            $"Location={NomBaseDeDonnees};" +
            $"User ID={UtilisateurConnexion};" +
            $"Password={MotDePasseConnexion};";

        /// <summary>
        /// Résumé lisible "serveur[:port] · base" affiché à côté du lien "Paramètres du
        /// logiciel" sur l'écran de connexion, pour qu'on voie en un coup d'œil vers quel
        /// serveur/base l'application est actuellement configurée.
        /// </summary>
        public static string ResumeConnexion =>
            $"{ServeurHFSQL}{(PortHFSQL.HasValue ? ":" + PortHFSQL.Value : string.Empty)} · {NomBaseDeDonnees}";

        /// <summary>
        /// Charge la configuration : d'abord appsettings.json (paramètres par défaut + emplacement
        /// du fichier .ini), puis le fichier .ini du logiciel s'il est accessible (ses valeurs
        /// remplacent alors celles d'appsettings.json). A appeler une seule fois, au démarrage
        /// (Program.cs), puis à nouveau après modification des paramètres du logiciel.
        /// </summary>
        public static void Charger()
        {
            ChargerDepuisAppSettings();
            ChargerDepuisFichierIni();
        }

        /// <summary>Charge uniquement appsettings.json (paramètres par défaut, sans le .ini).</summary>
        private static void ChargerDepuisAppSettings()
        {
            try
            {
                string chemin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (!File.Exists(chemin))
                    return;

                string json = File.ReadAllText(chemin);
                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement racine = document.RootElement;

                if (racine.TryGetProperty("HFSQL", out JsonElement hfsql))
                {
                    ServeurHFSQL = LireTexte(hfsql, "ServeurHFSQL", ServeurHFSQL);
                    if (hfsql.TryGetProperty("PortHFSQL", out JsonElement portElement) && portElement.TryGetInt32(out int port))
                        PortHFSQL = port;
                    NomBaseDeDonnees = LireTexte(hfsql, "NomBaseDeDonnees", NomBaseDeDonnees);
                    NomProviderOleDb = LireTexte(hfsql, "NomProviderOleDb", NomProviderOleDb);
                    UtilisateurConnexion = LireTexte(hfsql, "UtilisateurConnexion", UtilisateurConnexion);
                    MotDePasseConnexion = LireTexte(hfsql, "MotDePasseConnexion", MotDePasseConnexion);
                    TimeoutConnexionSecondes = LireEntier(hfsql, "TimeoutConnexionSecondes", TimeoutConnexionSecondes);
                }

                if (racine.TryGetProperty("TableUtilisateurs", out JsonElement table))
                {
                    TableUtilisateurs = LireTexte(table, "Nom", TableUtilisateurs);
                    ColonneId = LireTexte(table, "ColonneId", ColonneId);
                    ColonneLogin = LireTexte(table, "ColonneLogin", ColonneLogin);
                    ColonneMotDePasse = LireTexte(table, "ColonneMotDePasse", ColonneMotDePasse);
                    ColonneNomComplet = LireTexte(table, "ColonneNomComplet", ColonneNomComplet);
                }

                if (racine.TryGetProperty("Logiciel", out JsonElement logiciel))
                {
                    NomLogiciel = LireTexte(logiciel, "Nom", NomLogiciel);
                    CheminFichierIni = LireTexte(logiciel, "CheminFichierIni", CheminFichierIni);
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur de lecture, on conserve les valeurs par défaut ci-dessus.
                Console.Error.WriteLine("AppConfig: impossible de lire appsettings.json - " + ex.Message);
            }
        }

        /// <summary>
        /// Relit le fichier .ini du logiciel (CheminFichierIni) et en extrait, dans la section
        /// [Serveur] :
        /// - clé "Nom" -> adresse du serveur HFSQL, avec port optionnel ("Adresse:Port") ;
        /// - clé "NomBaseDonnées" -> nom de la base HFSQL à utiliser.
        /// Convention observée sur vos autres logiciels (Aizenta, eKol, ...). Public pour
        /// pouvoir être rappelée juste après que l'administrateur a changé
        /// NomLogiciel/CheminFichierIni dans FormParametresLogiciel.
        /// </summary>
        public static void ChargerDepuisFichierIni()
        {
            DerniereErreurIni = string.Empty;

            if (string.IsNullOrWhiteSpace(CheminFichierIni))
                return;

            try
            {
                if (!File.Exists(CheminFichierIni))
                {
                    DerniereErreurIni = $"Fichier d'initialisation introuvable : {CheminFichierIni}";
                    return;
                }

                // "Nom" contient l'adresse du serveur, éventuellement suivie de ":port"
                // (ex : "XRVEUR" ou "XRVEUR:4900"). On ne coupe sur ":" que si ce qui suit est
                // bien un nombre, pour ne pas casser une adresse IPv6 ou un nom contenant ":".
                string? adresseServeur = IniFileReader.LireValeur(CheminFichierIni, "Serveur", "Nom");
                if (!string.IsNullOrWhiteSpace(adresseServeur))
                {
                    int indexDeuxPoints = adresseServeur.LastIndexOf(':');
                    if (indexDeuxPoints > 0 && int.TryParse(adresseServeur[(indexDeuxPoints + 1)..], out int port))
                    {
                        ServeurHFSQL = adresseServeur[..indexDeuxPoints];
                        PortHFSQL = port;
                    }
                    else
                    {
                        ServeurHFSQL = adresseServeur;
                    }
                }

                // "NomBaseDonnées" contient le nom de la base à ouvrir sur ce serveur : c'est la
                // valeur essentielle, celle qui change réellement d'un poste à l'autre.
                string? nomBase = IniFileReader.LireValeur(CheminFichierIni, "Serveur", "NomBaseDonnées");
                if (!string.IsNullOrWhiteSpace(nomBase))
                {
                    NomBaseDeDonnees = nomBase;
                }
                else
                {
                    DerniereErreurIni = $"Clé \"NomBaseDonnées\" absente de la section [Serveur] dans {CheminFichierIni}";
                }
            }
            catch (Exception ex)
            {
                // Une erreur ici ne doit pas empêcher l'application de démarrer : on garde les
                // valeurs précédentes (celles d'appsettings.json) et on remonte l'erreur pour
                // affichage sur l'écran de connexion.
                DerniereErreurIni = "Erreur de lecture du fichier .ini : " + ex.Message;
                Console.Error.WriteLine("AppConfig: " + DerniereErreurIni);
            }
        }

        /// <summary>
        /// Réécrit appsettings.json avec l'état actuel de la configuration (y compris NomLogiciel
        /// et CheminFichierIni). Appelée par FormParametresLogiciel après modification par un
        /// administrateur, pour que le choix soit conservé au prochain démarrage.
        /// </summary>
        public static void Enregistrer()
        {
            var structureJson = new
            {
                HFSQL = new
                {
                    ServeurHFSQL,
                    PortHFSQL,
                    NomBaseDeDonnees,
                    NomProviderOleDb,
                    UtilisateurConnexion,
                    MotDePasseConnexion,
                    TimeoutConnexionSecondes,
                },
                TableUtilisateurs = new
                {
                    Nom = TableUtilisateurs,
                    ColonneId,
                    ColonneLogin,
                    ColonneMotDePasse,
                    ColonneNomComplet,
                },
                Logiciel = new
                {
                    Nom = NomLogiciel,
                    CheminFichierIni,
                },
            };

            string chemin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            string json = JsonSerializer.Serialize(structureJson, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(chemin, json);
        }

        private static string LireTexte(JsonElement element, string propriete, string valeurParDefaut) =>
            element.TryGetProperty(propriete, out JsonElement valeur) ? (valeur.GetString() ?? valeurParDefaut) : valeurParDefaut;

        private static int LireEntier(JsonElement element, string propriete, int valeurParDefaut) =>
            element.TryGetProperty(propriete, out JsonElement valeur) && valeur.TryGetInt32(out int resultat) ? resultat : valeurParDefaut;
    }
}
