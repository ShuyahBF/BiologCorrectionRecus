namespace BiologCorrectionRecus.Modeles;

/// <summary>Représente un reçu (une ligne de VenteClinique), sans son contenu PDF (chargé à part).</summary>
internal sealed class Recu
{
    public int IdVente { get; init; }
    public DateTime DateVente { get; init; }
    public string CodeClient { get; init; } = string.Empty;
    public bool EstCorrige { get; init; }

    /// <summary>Libellé affiché dans la grille pour la colonne "Statut".</summary>
    public string Statut => EstCorrige ? "Corrigé" : "Original";
}
