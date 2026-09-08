namespace BiologCorrectionRecus.Modeles;

/// <summary>
/// Une ligne d'article facturé sur un reçu (table A_Acheté), enrichie du nom du
/// médecin prescripteur (jointure avec MédecinT — voir hypothèse 1 du README).
/// </summary>
internal sealed class LigneRecu
{
    public int IdLigne { get; init; }
    public int IdVente { get; init; }
    public string CodeArticle { get; init; } = string.Empty;
    public string Designation { get; init; } = string.Empty;
    public DateTime DateEmission { get; init; }
    public string NomMedecin { get; init; } = string.Empty;

    /// <summary>Vrai si CodeArticle/Désignation correspond au motif pédiatrie (Config.MotifCodePediatrie).</summary>
    public bool EstPediatrie =>
        CodeArticle.Contains(Config.MotifCodePediatrie, StringComparison.OrdinalIgnoreCase) ||
        Designation.Contains(Config.MotifCodePediatrie, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Date de validité corrigée pour une ligne pédiatrie : émission + N jours (Config.NombreJoursValiditePediatrie).
    /// Pour une ligne non pédiatrie, la notion de "validité corrigée" ne s'applique pas.
    /// </summary>
    public DateTime DateValiditeCorrigee => DateEmission.AddDays(Config.NombreJoursValiditePediatrie);
}
