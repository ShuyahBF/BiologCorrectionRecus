using BiologCorrectionRecus.Modeles;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace BiologCorrectionRecus.Services;

/// <summary>
/// Génère le PDF corrigé d'un reçu : mêmes informations que l'original, mais avec la
/// date de validité recalculée (émission + 10 jours) pour les lignes pédiatrie.
/// PdfSharpCore permet de dessiner un PDF "page blanche" sans dépendre de GDI+ ni
/// d'un moteur externe — c'est pourquoi on régénère un document plutôt que de modifier
/// l'original ligne à ligne (bien plus fiable qu'un "patch" du PDF existant).
/// </summary>
internal static class ServiceCorrectionPdf
{
    private static readonly XFont PoliceTitre = new("Arial", 14, XFontStyle.Bold);
    private static readonly XFont PoliceEntete = new("Arial", 10, XFontStyle.Bold);
    private static readonly XFont PoliceTexte = new("Arial", 10, XFontStyle.Regular);
    private static readonly XFont PolicePetite = new("Arial", 8, XFontStyle.Italic);

    /// <summary>Construit le PDF corrigé en mémoire (byte[]) prêt à être enregistré dans HFSQL.</summary>
    public static byte[] GenererPdfCorrige(Recu recu, List<LigneRecu> lignes)
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        using var graphique = XGraphics.FromPdfPage(page);

        double margeGauche = 40;
        double y = 40;

        graphique.DrawString("Reçu corrigé — Biolog", PoliceTitre, XBrushes.Black, margeGauche, y);
        y += 24;

        graphique.DrawString($"Reçu n° {recu.IdVente}", PoliceTexte, XBrushes.Black, margeGauche, y);
        y += 16;
        graphique.DrawString($"Date de vente : {recu.DateVente:dd/MM/yyyy}", PoliceTexte, XBrushes.Black, margeGauche, y);
        y += 16;
        graphique.DrawString($"Client : {recu.CodeClient}", PoliceTexte, XBrushes.Black, margeGauche, y);
        y += 24;

        // En-tête du tableau des lignes
        graphique.DrawString("Article", PoliceEntete, XBrushes.Black, margeGauche, y);
        graphique.DrawString("Médecin", PoliceEntete, XBrushes.Black, margeGauche + 200, y);
        graphique.DrawString("Émission", PoliceEntete, XBrushes.Black, margeGauche + 340, y);
        graphique.DrawString("Validité", PoliceEntete, XBrushes.Black, margeGauche + 430, y);
        y += 14;
        graphique.DrawLine(XPens.Black, margeGauche, y, margeGauche + 520, y);
        y += 8;

        foreach (var ligne in lignes)
        {
            // Ligne pédiatrie : date de validité recalculée (émission + N jours).
            // Ligne normale : on conserve la date d'émission telle quelle.
            var dateValiditeAffichee = ligne.EstPediatrie ? ligne.DateValiditeCorrigee : ligne.DateEmission;

            graphique.DrawString(ligne.Designation, PoliceTexte, XBrushes.Black, margeGauche, y);
            graphique.DrawString(ligne.NomMedecin, PoliceTexte, XBrushes.Black, margeGauche + 200, y);
            graphique.DrawString(ligne.DateEmission.ToString("dd/MM/yyyy"), PoliceTexte, XBrushes.Black, margeGauche + 340, y);
            graphique.DrawString(dateValiditeAffichee.ToString("dd/MM/yyyy"), PoliceTexte, XBrushes.Black, margeGauche + 430, y);
            y += 16;

            if (ligne.EstPediatrie)
            {
                graphique.DrawString(
                    $"   → ligne pédiatrie : validité corrigée à émission + {Config.NombreJoursValiditePediatrie} jours",
                    PolicePetite, XBrushes.DarkRed, margeGauche, y);
                y += 12;
            }
        }

        y += 20;
        graphique.DrawString(
            $"Document corrigé automatiquement le {DateTime.Now:dd/MM/yyyy HH:mm}.",
            PolicePetite, XBrushes.Gray, margeGauche, y);

        using var flux = new MemoryStream();
        document.Save(flux, closeStream: false);
        return flux.ToArray();
    }
}
