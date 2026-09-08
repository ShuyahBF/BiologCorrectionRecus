using System.Data.OleDb;

namespace BiologCorrectionRecus.Data;

/// <summary>
/// Fabrique de connexions OLEDB vers la base HFSQL.
/// Toute la chaîne de connexion est construite à partir de Config.cs : en cas de
/// souci de connexion, c'est là qu'il faut regarder en premier (nom du Provider,
/// nom du serveur, nom de la base).
/// </summary>
internal static class HfsqlConnexion
{
    /// <summary>
    /// Construit la chaîne de connexion OLEDB vers HFSQL.
    /// ATTENTION (hypothèse 3 du README) : les pilotes HFSQL peuvent se montrer capricieux
    /// avec certains outils externes. Toujours tester une connexion simple (SELECT) avant
    /// de tester l'écriture de blob (PDF).
    /// </summary>
    private static string ChaineDeConnexion =>
        $"Provider={Config.HfsqlProviderOleDb};" +
        $"Data Source={Config.HfsqlServeur};" +
        $"Location={Config.HfsqlBase};" +
        $"User ID={Config.HfsqlUtilisateur};" +
        $"Password={Config.HfsqlMotDePasse};";

    /// <summary>
    /// Ouvre une nouvelle connexion OLEDB vers HFSQL. L'appelant est responsable de la
    /// libérer (bloc "using") — c'est le pattern standard en C# pour toute ressource
    /// non managée (fichier, connexion réseau, etc.), équivalent d'un "HFermeConnexion"
    /// systématique en fin de traitement côté WinDev.
    /// </summary>
    public static OleDbConnection OuvrirConnexion()
    {
        var connexion = new OleDbConnection(ChaineDeConnexion);
        connexion.Open();
        return connexion;
    }
}
