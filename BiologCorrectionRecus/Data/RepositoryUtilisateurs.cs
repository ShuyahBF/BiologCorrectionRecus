using System.Data.OleDb;
using BiologCorrectionRecus.Modeles;

namespace BiologCorrectionRecus.Data;

/// <summary>Accès en lecture à la table UtilisateurBlg (authentification des caissiers).</summary>
internal static class RepositoryUtilisateurs
{
    /// <summary>
    /// Vérifie le couple login/mot de passe et renvoie l'utilisateur si valide, sinon null.
    /// Utilise une requête paramétrée (jamais de concaténation de texte) pour éviter
    /// toute injection SQL, même si les identifiants viennent d'un simple formulaire interne.
    /// </summary>
    public static Utilisateur? Authentifier(string login, string motDePasse)
    {
        using var connexion = HfsqlConnexion.OuvrirConnexion();

        var texteRequete =
            $"SELECT {Config.ColUtilisateurId}, {Config.ColUtilisateurLogin}, {Config.ColUtilisateurNomComplet} " +
            $"FROM {Config.TableUtilisateurs} " +
            $"WHERE {Config.ColUtilisateurLogin} = ? AND {Config.ColUtilisateurMotDePasse} = ?";

        using var commande = new OleDbCommand(texteRequete, connexion);
        // OLEDB utilise des paramètres positionnels ("?"), l'ordre d'ajout doit
        // correspondre exactement à l'ordre des "?" dans la requête ci-dessus.
        commande.Parameters.AddWithValue("@login", login);
        commande.Parameters.AddWithValue("@motDePasse", motDePasse);

        using var lecteur = commande.ExecuteReader();
        if (!lecteur.Read())
        {
            return null; // Aucun utilisateur ne correspond : identifiants invalides
        }

        return new Utilisateur
        {
            IdUtilisateur = lecteur.GetInt32(0),
            Login = lecteur.GetString(1),
            NomComplet = lecteur.IsDBNull(2) ? string.Empty : lecteur.GetString(2),
        };
    }
}
