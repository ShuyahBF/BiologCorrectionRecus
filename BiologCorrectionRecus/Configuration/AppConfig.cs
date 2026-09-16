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
    ///    ressource à utiliser (section "Serveur", clé "Nom" -> nom de la base HFSQL). Ses
    ///    valeurs, quand présentes, remplacent celles d'appsettings.json.
    ///
    /// C'est cette classe (et non plus Config.cs) qui porte désormais la connexion HFSQL ;
    /// Config.cs ne garde que les constantes métier propres à Biolog (tables des reçus,
    /// règles PEDIAT, etc.).
    /// </summary>
    public static class AppConfig
    {
        // ----- Paramètres de connexion au serveur HFSQL -----
        public static string ServeurHFSQL { get; set; } = "localhost";
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
        /// </summary>
        public static string ChaineConnexion =>
            $"Provider={NomProviderOleDb};" +
            $"Data Source={ServeurHFSQL};" +
            $"Location={NomBaseDeDonnees};" +
            $"User ID={UtilisateurConnexion};" +
            $"Password={MotDePasseConnexion};";

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
        /// Relit le fichier .ini du logiciel (CheminFichierIni) et en extrait le nom de la base
        /// HFSQL à utiliser (section "Serveur", clé "Nom" — voir la convention Aizenta/Biolog/eKol).
        /// Public pour pouvoir être rappelée juste après que l'administrateur a changé
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

                // Convention observée sur vos autres logiciels (Aizenta, eKol, ...) : le nom de
                // la base HFSQL est écrit dans la section [Serveur], clé "Nom". Si votre fichier
                // .ini expose d'autres paramètres (adresse du serveur, port...) sous d'autres
                // clés, ajoutez-les ici de la même façon une fois leurs noms exacts confirmés.
                string? nomBase = IniFileReader.LireValeur(CheminFichierIni, "Serveur", "Nom");
                if (!string.IsNullOrWhiteSpace(nomBase))
                {
                    NomBaseDeDonnees = nomBase;
                }
                else
                {
                    DerniereErreurIni = $"Clé \"Nom\" absente de la section [Serveur] dans {CheminFichierIni}";
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
