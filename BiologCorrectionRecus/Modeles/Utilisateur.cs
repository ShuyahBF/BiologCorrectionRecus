namespace BiologCorrectionRecus.Modeles;

/// <summary>Représente une ligne de la table UtilisateurBlg (un caissier/utilisateur de l'appli).</summary>
internal sealed class Utilisateur
{
    public int IdUtilisateur { get; init; }
    public string Login { get; init; } = string.Empty;
    public string NomComplet { get; init; } = string.Empty;
}
