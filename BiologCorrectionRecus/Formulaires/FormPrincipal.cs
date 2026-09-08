using System.Diagnostics;
using BiologCorrectionRecus.Data;
using BiologCorrectionRecus.Modeles;
using BiologCorrectionRecus.Services;

namespace BiologCorrectionRecus.Formulaires;

/// <summary>
/// Écran principal : liste des reçus, correction en un clic, ouverture et impression
/// du reçu sélectionné (toujours la version corrigée quand elle existe).
/// </summary>
public partial class FormPrincipal : Form
{
    private readonly Utilisateur _utilisateurConnecte;
    private List<Recu> _recus = new();

    public FormPrincipal(Utilisateur utilisateurConnecte)
    {
        _utilisateurConnecte = utilisateurConnecte;
        InitializeComponent();
        Text = $"Biolog — Correction des reçus ({_utilisateurConnecte.NomComplet})";
        Load += (_, _) => ChargerGrille();
    }

    /// <summary>Recharge la grille depuis HFSQL (appelé au démarrage et par "Actualiser").</summary>
    private void ChargerGrille()
    {
        try
        {
            labelStatut.Text = "Chargement des reçus…";
            Application.DoEvents(); // Force l'affichage du message avant l'appel bloquant à HFSQL

            _recus = RepositoryRecus.ObtenirRecusRecents();

            grilleRecus.DataSource = null;
            grilleRecus.DataSource = _recus
                .Select(r => new
                {
                    Reçu = r.IdVente,
                    Date = r.DateVente,
                    Client = r.CodeClient,
                    Statut = r.Statut,
                })
                .ToList();

            labelStatut.Text = $"{_recus.Count} reçu(s) chargé(s).";
        }
        catch (Exception ex)
        {
            labelStatut.Text = "Erreur lors du chargement.";
            MessageBox.Show(this, $"Impossible de charger les reçus :\n{ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BoutonActualiser_Click(object? sender, EventArgs e) => ChargerGrille();

    /// <summary>Lance la vérification/correction de tous les reçus non corrigés.</summary>
    private void BoutonVerifier_Click(object? sender, EventArgs e)
    {
        boutonVerifier.Enabled = false;
        try
        {
            labelStatut.Text = "Vérification en cours…";
            Application.DoEvents();

            var resultat = ServiceVerification.VerifierEtCorrigerTout();

            labelStatut.Text =
                $"Vérification terminée : {resultat.ReçusCorriges} reçu(s) corrigé(s) sur {resultat.ReçusExamines} examiné(s).";

            ChargerGrille();
        }
        catch (Exception ex)
        {
            labelStatut.Text = "Erreur pendant la vérification.";
            MessageBox.Show(this, $"Erreur pendant la vérification :\n{ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            boutonVerifier.Enabled = true;
        }
    }

    /// <summary>Reçu actuellement sélectionné dans la grille, ou null si aucun.</summary>
    private Recu? RecuSelectionne()
    {
        if (grilleRecus.CurrentRow is null)
        {
            return null;
        }

        var idVente = (int)grilleRecus.CurrentRow.Cells["Reçu"].Value;
        return _recus.FirstOrDefault(r => r.IdVente == idVente);
    }

    /// <summary>
    /// Écrit le PDF du reçu (corrigé en priorité) dans un fichier temporaire et le renvoie.
    /// On passe systématiquement par un fichier temporaire car il n'existe pas de visionneuse
    /// PDF intégrée native à WinForms : on s'appuie sur l'application PDF par défaut de Windows.
    /// </summary>
    private string? EcrirePdfTemporaire(int idVente)
    {
        var pdf = RepositoryRecus.ObtenirPdfRecu(idVente, preferCorrige: true);
        if (pdf is null || pdf.Length == 0)
        {
            MessageBox.Show(this, "Aucun PDF n'est disponible pour ce reçu.",
                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        var cheminTemporaire = Path.Combine(Path.GetTempPath(), $"Recu_{idVente}.pdf");
        File.WriteAllBytes(cheminTemporaire, pdf);
        return cheminTemporaire;
    }

    /// <summary>Double-clic sur une ligne : ouvre le PDF (corrigé si disponible, sinon original).</summary>
    private void GrilleRecus_DoubleClick(object? sender, EventArgs e)
    {
        var recu = RecuSelectionne();
        if (recu is null)
        {
            return;
        }

        var chemin = EcrirePdfTemporaire(recu.IdVente);
        if (chemin is null)
        {
            return;
        }

        // Lance la visionneuse PDF par défaut du poste (UseShellExecute est indispensable
        // ici : c'est lui qui délègue l'ouverture du fichier à Windows selon l'association
        // de fichier .pdf configurée sur le poste).
        Process.Start(new ProcessStartInfo(chemin) { UseShellExecute = true });
    }

    /// <summary>Imprime le reçu sélectionné via l'application PDF par défaut du poste (verbe "print").</summary>
    private void BoutonImprimer_Click(object? sender, EventArgs e)
    {
        var recu = RecuSelectionne();
        if (recu is null)
        {
            MessageBox.Show(this, "Veuillez sélectionner un reçu à imprimer.",
                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var chemin = EcrirePdfTemporaire(recu.IdVente);
        if (chemin is null)
        {
            return;
        }

        try
        {
            // Le verbe "print" demande à l'application associée aux .pdf de lancer une
            // impression silencieuse sur l'imprimante par défaut du poste — c'est la façon
            // la plus simple d'imprimer un PDF depuis WinForms sans bibliothèque tierce.
            Process.Start(new ProcessStartInfo(chemin) { UseShellExecute = true, Verb = "print" });
            labelStatut.Text = $"Impression du reçu {recu.IdVente} envoyée.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Impossible d'imprimer ce reçu :\n{ex.Message}",
                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
