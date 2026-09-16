using System.Data.OleDb;
using BiologCorrectionRecus.Modeles;

namespace BiologCorrectionRecus.Data;

/// <summary>
/// Accès aux reçus (VenteClinique), à leurs lignes (A_Acheté) et à leur contenu PDF.
/// C'est la classe la plus sensible du projet : c'est ici que se joue l'écriture du
/// blob PDF corrigé dans HFSQL (hypothèse 3 du README — à tester en priorité).
/// </summary>
internal static class RepositoryRecus
{
    /// <summary>Charge les N reçus les plus récents, triés du plus récent au plus ancien.</summary>
    public static List<Recu> ObtenirRecusRecents(int nombre = 0)
    {
        if (nombre <= 0)
        {
            nombre = Config.NombreRecusAffiches;
        }

        using var connexion = HfsqlConnectionManager.CreerConnexion();
        connexion.Open();

        // "SELECT TOP n" est la syntaxe la plus répandue côté HFSQL/Access ;
        // si le pilote la refuse, se replier sur un tri + limite côté C# (moins efficace
        // mais fonctionne toujours) — voir remarque dans le README (hypothèses SQL).
        var texteRequete =
            $"SELECT TOP {nombre} {Config.ColVenteId}, {Config.ColVenteDate}, {Config.ColVenteCodeClient}, {Config.ColVenteEstCorrige} " +
            $"FROM {Config.TableVentes} " +
            $"ORDER BY {Config.ColVenteDate} DESC";

        using var commande = new OleDbCommand(texteRequete, connexion);
        using var lecteur = commande.ExecuteReader();

        var recus = new List<Recu>();
        while (lecteur.Read())
        {
            recus.Add(new Recu
            {
                IdVente = lecteur.GetInt32(0),
                DateVente = lecteur.GetDateTime(1),
                CodeClient = lecteur.IsDBNull(2) ? string.Empty : lecteur.GetString(2),
                EstCorrige = !lecteur.IsDBNull(3) && lecteur.GetBoolean(3),
            });
        }

        return recus;
    }

    /// <summary>
    /// Charge les lignes d'un reçu, avec le nom du médecin prescripteur obtenu par jointure
    /// A_Acheté.IdentitéPrestataire = MédecinT.IDMédecin (hypothèse 1 du README — à vérifier
    /// si le nom du médecin n'apparaît pas correctement sur le PDF généré).
    /// </summary>
    public static List<LigneRecu> ObtenirLignesDuRecu(int idVente)
    {
        using var connexion = HfsqlConnectionManager.CreerConnexion();
        connexion.Open();

        var texteRequete =
            $"SELECT l.{Config.ColLigneId}, l.{Config.ColLigneVenteId}, l.{Config.ColLigneCodeArticle}, " +
            $"       l.{Config.ColLigneDesignation}, l.{Config.ColLigneDateEmission}, " +
            $"       m.{Config.ColMedecinNom}, m.{Config.ColMedecinPrenom} " +
            $"FROM {Config.TableLignesAchat} l " +
            $"LEFT JOIN {Config.TableMedecins} m ON l.{Config.ColLigneIdentitePrestataire} = m.{Config.ColMedecinId} " +
            $"WHERE l.{Config.ColLigneVenteId} = ?";

        using var commande = new OleDbCommand(texteRequete, connexion);
        commande.Parameters.AddWithValue("@idVente", idVente);

        using var lecteur = commande.ExecuteReader();

        var lignes = new List<LigneRecu>();
        while (lecteur.Read())
        {
            var nom = lecteur.IsDBNull(5) ? string.Empty : lecteur.GetString(5);
            var prenom = lecteur.IsDBNull(6) ? string.Empty : lecteur.GetString(6);

            lignes.Add(new LigneRecu
            {
                IdLigne = lecteur.GetInt32(0),
                IdVente = lecteur.GetInt32(1),
                CodeArticle = lecteur.IsDBNull(2) ? string.Empty : lecteur.GetString(2),
                Designation = lecteur.IsDBNull(3) ? string.Empty : lecteur.GetString(3),
                DateEmission = lecteur.GetDateTime(4),
                NomMedecin = string.Join(' ', new[] { prenom, nom }.Where(s => !string.IsNullOrWhiteSpace(s))),
            });
        }

        return lignes;
    }

    /// <summary>Renvoie le contenu PDF d'un reçu : la version corrigée si elle existe, sinon l'originale.</summary>
    public static byte[]? ObtenirPdfRecu(int idVente, bool preferCorrige = true)
    {
        using var connexion = HfsqlConnectionManager.CreerConnexion();
        connexion.Open();

        var texteRequete =
            $"SELECT {Config.ColVentePdfOriginal}, {Config.ColVentePdfCorrige} " +
            $"FROM {Config.TableVentes} WHERE {Config.ColVenteId} = ?";

        using var commande = new OleDbCommand(texteRequete, connexion);
        commande.Parameters.AddWithValue("@idVente", idVente);

        using var lecteur = commande.ExecuteReader();
        if (!lecteur.Read())
        {
            return null;
        }

        var pdfCorrige = lecteur.IsDBNull(1) ? null : (byte[])lecteur[1];
        var pdfOriginal = lecteur.IsDBNull(0) ? null : (byte[])lecteur[0];

        return preferCorrige && pdfCorrige is { Length: > 0 } ? pdfCorrige : pdfOriginal;
    }

    /// <summary>
    /// Enregistre le PDF corrigé dans HFSQL et marque le reçu comme corrigé.
    /// Le "WHERE EstCorrige = 0" (en plus du filtre sur l'ID) est une protection minimale
    /// contre la double correction si plusieurs postes caissiers tournent en même temps
    /// (limitation évoquée dans le README) : si un autre poste a corrigé entre-temps,
    /// cette mise à jour ne touchera simplement aucune ligne.
    /// </summary>
    public static bool EnregistrerPdfCorrige(int idVente, byte[] pdfCorrige)
    {
        using var connexion = HfsqlConnectionManager.CreerConnexion();
        connexion.Open();

        var texteRequete =
            $"UPDATE {Config.TableVentes} " +
            $"SET {Config.ColVentePdfCorrige} = ?, {Config.ColVenteEstCorrige} = 1 " +
            $"WHERE {Config.ColVenteId} = ? AND {Config.ColVenteEstCorrige} = 0";

        using var commande = new OleDbCommand(texteRequete, connexion);

        // Écriture d'un blob via OLEDB : le paramètre binaire doit être explicitement
        // typé en OleDbType.VarBinary/LongVarBinary, sinon certains pilotes HFSQL le
        // rejettent ou le tronquent (point le plus à risque du projet, cf. README).
        var parametrePdf = commande.Parameters.Add("@pdf", OleDbType.LongVarBinary);
        parametrePdf.Value = pdfCorrige;
        commande.Parameters.AddWithValue("@idVente", idVente);

        var lignesAffectees = commande.ExecuteNonQuery();
        return lignesAffectees > 0;
    }
}
