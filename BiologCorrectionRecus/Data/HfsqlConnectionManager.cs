using System;
using System.Collections.Generic;
using System.Data.OleDb;
using BiologCorrectionRecus.Configuration;
using BiologCorrectionRecus.Modeles;

namespace BiologCorrectionRecus.Data
{
    /// <summary>
    /// Centralise l'accès au serveur HFSQL : création de connexion (via le pilote OLEDB HFSQL),
    /// chargement de la table des utilisateurs et authentification.
    ///
    /// Reprend la même architecture que le projet de référence "HFSQL_LoginApp" (dépôt Claude),
    /// adaptée à OLEDB : c'est cette même classe, au même rôle, qui doit être reproduite dans
    /// tout futur projet WinForms ayant besoin de se connecter à un serveur HFSQL.
    /// </summary>
    internal static class HfsqlConnectionManager
    {
        /// <summary>
        /// Crée une nouvelle connexion OLEDB vers le serveur HFSQL configuré dans AppConfig.
        /// L'appelant est responsable de l'ouvrir et de la libérer (bloc "using").
        /// </summary>
        public static OleDbConnection CreerConnexion() => new OleDbConnection(AppConfig.ChaineConnexion);

        /// <summary>
        /// Teste la connexion au serveur HFSQL sans rien lire ni écrire.
        /// </summary>
        public static bool TesterConnexion(out string messageErreur)
        {
            messageErreur = string.Empty;
            try
            {
                using OleDbConnection connexion = CreerConnexion();
                connexion.Open();
                return true;
            }
            catch (Exception ex)
            {
                messageErreur = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Ouvre la table des utilisateurs et renvoie la liste (id, login, nom complet) utilisée
        /// pour alimenter la ComboBox de la fenêtre de connexion.
        /// Le mot de passe n'est volontairement pas chargé à cette étape.
        /// </summary>
        public static List<Utilisateur> ChargerUtilisateurs()
        {
            var utilisateurs = new List<Utilisateur>();

            string requete =
                $"SELECT {AppConfig.ColonneId}, {AppConfig.ColonneLogin}, {AppConfig.ColonneNomComplet} " +
                $"FROM {AppConfig.TableUtilisateurs} " +
                $"ORDER BY {AppConfig.ColonneNomComplet}";

            using OleDbConnection connexion = CreerConnexion();
            connexion.Open();

            using var commande = new OleDbCommand(requete, connexion);
            using OleDbDataReader lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                utilisateurs.Add(new Utilisateur
                {
                    IdUtilisateur = lecteur.GetInt32(lecteur.GetOrdinal(AppConfig.ColonneId)),
                    Login = LireColonne(lecteur, AppConfig.ColonneLogin),
                    NomComplet = LireColonne(lecteur, AppConfig.ColonneNomComplet),
                });
            }

            return utilisateurs;
        }

        /// <summary>
        /// Vérifie le couple login / mot de passe saisi par rapport à la table HFSQL des
        /// utilisateurs. Renvoie l'utilisateur authentifié, ou null si le mot de passe est
        /// incorrect.
        /// </summary>
        /// <remarks>
        /// Si les mots de passe sont stockés hachés dans votre table (recommandé), remplacez
        /// la comparaison SQL ci-dessous par un hachage du mot de passe saisi avant comparaison.
        /// </remarks>
        public static Utilisateur? Authentifier(string login, string motDePasse)
        {
            string requete =
                $"SELECT {AppConfig.ColonneId}, {AppConfig.ColonneLogin}, {AppConfig.ColonneNomComplet} " +
                $"FROM {AppConfig.TableUtilisateurs} " +
                $"WHERE {AppConfig.ColonneLogin} = ? AND {AppConfig.ColonneMotDePasse} = ?";

            using OleDbConnection connexion = CreerConnexion();
            connexion.Open();

            using var commande = new OleDbCommand(requete, connexion);
            // Les paramètres OLEDB sont positionnels : l'ordre ci-dessous doit correspondre à
            // l'ordre des "?" dans la requête (login puis mot de passe).
            commande.Parameters.AddWithValue("@Login", login);
            commande.Parameters.AddWithValue("@MotDePasse", motDePasse);

            using OleDbDataReader lecteur = commande.ExecuteReader();
            if (lecteur.Read())
            {
                return new Utilisateur
                {
                    IdUtilisateur = lecteur.GetInt32(lecteur.GetOrdinal(AppConfig.ColonneId)),
                    Login = LireColonne(lecteur, AppConfig.ColonneLogin),
                    NomComplet = LireColonne(lecteur, AppConfig.ColonneNomComplet),
                };
            }

            return null;
        }

        private static string LireColonne(OleDbDataReader lecteur, string nomColonne)
        {
            int index = lecteur.GetOrdinal(nomColonne);
            return lecteur.IsDBNull(index) ? string.Empty : lecteur.GetValue(index).ToString() ?? string.Empty;
        }
    }
}
