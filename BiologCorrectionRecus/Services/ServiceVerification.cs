using BiologCorrectionRecus.Data;

namespace BiologCorrectionRecus.Services;

/// <summary>Résultat d'un passage de vérification, affiché dans la barre de statut.</summary>
internal sealed record ResultatVerification(int ReçusExamines, int ReçusCorriges, List<string> Journal);

/// <summary>
/// Orchestrateur du bouton "Vérifier maintenant" : parcourt les reçus non corrigés,
/// détecte les lignes pédiatrie, régénère le PDF corrigé et l'enregistre.
/// </summary>
internal static class ServiceVerification
{
    public static ResultatVerification VerifierEtCorrigerTout()
    {
        var journal = new List<string>();
        var recus = RepositoryRecus.ObtenirRecusRecents();
        var reçusACorreriger = recus.Where(r => !r.EstCorrige).ToList();

        var nombreCorriges = 0;

        foreach (var recu in reçusACorreriger)
        {
            var lignes = RepositoryRecus.ObtenirLignesDuRecu(recu.IdVente);

            // On ne corrige que les reçus contenant au moins une ligne pédiatrie :
            // pas besoin de régénérer un PDF pour un reçu qui n'a rien à corriger.
            if (!lignes.Any(l => l.EstPediatrie))
            {
                continue;
            }

            var pdfCorrige = ServiceCorrectionPdf.GenererPdfCorrige(recu, lignes);
            var enregistre = RepositoryRecus.EnregistrerPdfCorrige(recu.IdVente, pdfCorrige);

            if (enregistre)
            {
                nombreCorriges++;
                journal.Add($"Reçu {recu.IdVente} : corrigé ({lignes.Count(l => l.EstPediatrie)} ligne(s) pédiatrie).");
            }
            else
            {
                // La clause "WHERE EstCorrige = 0" de EnregistrerPdfCorrige n'a touché aucune
                // ligne : un autre poste a probablement corrigé ce reçu entre-temps.
                journal.Add($"Reçu {recu.IdVente} : ignoré (déjà corrigé entre-temps par un autre poste).");
            }
        }

        return new ResultatVerification(reçusACorreriger.Count, nombreCorriges, journal);
    }
}
