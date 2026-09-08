using BiologCorrectionRecus.Data;

namespace BiologCorrectionRecus.Formulaires;

/// <summary>Écran de connexion : authentifie l'utilisateur contre la table UtilisateurBlg.</summary>
public partial class FormConnexion : Form
{
    public FormConnexion()
    {
        InitializeComponent();
    }

    private void BoutonConnexion_Click(object? sender, EventArgs e)
    {
        var login = texteLogin.Text.Trim();
        var motDePasse = texteMotDePasse.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(motDePasse))
        {
            labelMessage.Text = "Veuillez saisir un identifiant et un mot de passe.";
            return;
        }

        try
        {
            var utilisateur = RepositoryUtilisateurs.Authentifier(login, motDePasse);
            if (utilisateur is null)
            {
                labelMessage.Text = "Identifiant ou mot de passe incorrect.";
                return;
            }

            // Connexion réussie : on ouvre l'écran principal et on ferme celui-ci.
            var formPrincipal = new FormPrincipal(utilisateur);
            formPrincipal.Show();
            Hide();
            formPrincipal.FormClosed += (_, _) => Close();
        }
        catch (Exception ex)
        {
            // En cas d'échec de connexion à la base HFSQL elle-même (serveur injoignable,
            // mauvais nom de base, etc.), on affiche le message d'erreur brut : c'est ce
            // message qui doit être remonté pour ajuster Config.cs (voir README).
            labelMessage.Text = $"Erreur de connexion à la base : {ex.Message}";
        }
    }
}
