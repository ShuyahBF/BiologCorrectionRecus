namespace BiologCorrectionRecus.Modeles;

/// <summary>Représente une ligne de la table UtilisateurBlg (un caissier/utilisateur de l'appli).</summary>
internal sealed class Utilisateur
{
    public int IdUtilisateur { get; init; }
    public string Login { get; init; } = string.Empty;
    public string NomComplet { get; init; } = string.Empty;

    /// <summary>Texte affiché dans la ComboBox de sélection de la fenêtre de connexion.</summary>
    public override string ToString() =>
        string.IsNullOrWhiteSpace(NomComplet) ? Login : $"{NomComplet} ({Login})";
}
