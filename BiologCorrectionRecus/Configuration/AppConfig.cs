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
        /// Charge la configuration depuis appsettings.json (s'il existe) et met à jour les
        /// valeurs par défaut ci-dessus. A appeler une seule fois, au démarrage (Program.cs).
        /// </summary>
        public static void Charger()
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
            }
            catch (Exception ex)
            {
                // En cas d'erreur de lecture, on conserve les valeurs par défaut ci-dessus.
                Console.Error.WriteLine("AppConfig: impossible de lire appsettings.json - " + ex.Message);
            }
        }

        private static string LireTexte(JsonElement element, string propriete, string valeurParDefaut) =>
            element.TryGetProperty(propriete, out JsonElement valeur) ? (valeur.GetString() ?? valeurParDefaut) : valeurParDefaut;

        private static int LireEntier(JsonElement element, string propriete, int valeurParDefaut) =>
            element.TryGetProperty(propriete, out JsonElement valeur) && valeur.TryGetInt32(out int resultat) ? resultat : valeurParDefaut;
    }
}
