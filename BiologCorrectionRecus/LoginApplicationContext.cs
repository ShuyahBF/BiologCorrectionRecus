using BiologCorrectionRecus.Configuration;
using BiologCorrectionRecus.Formulaires;
using BiologCorrectionRecus.Modeles;

namespace BiologCorrectionRecus;

/// <summary>
/// Pilote l'enchaînement des fenêtres de l'application : la fenêtre de connexion (LoginForm)
/// s'affiche en premier, puis cède la place à FormPrincipal (le menu principal) une fois
/// l'utilisateur authentifié. L'application se termine si l'une ou l'autre fenêtre se ferme
/// sans avoir mené à l'étape suivante.
///
/// Utiliser un ApplicationContext (plutôt que d'enchaîner des Application.Run successifs ou de
/// cacher/afficher des fenêtres) évite d'avoir une fenêtre de connexion "fantôme" ouverte en
/// tâche de fond une fois l'utilisateur authentifié — c'est le même mécanisme que celui déjà
/// utilisé et validé dans HFSQL_LoginApp, à reproduire dans tout futur projet Biolog.
/// </summary>
public class LoginApplicationContext : ApplicationContext
{
    public LoginApplicationContext()
    {
        AfficherFenetreConnexion();
    }

    private void AfficherFenetreConnexion()
    {
        var fenetreConnexion = new LoginForm();
        fenetreConnexion.ConnexionReussie += (_, utilisateur) => AfficherMenuPrincipal(utilisateur);
        fenetreConnexion.FormClosed += (_, _) =>
        {
            // Fermeture de la fenêtre de connexion sans authentification réussie (croix,
            // échec après 3 tentatives, etc.) : on quitte l'application.
            if (AppConfig.UtilisateurConnecte == null)
                ExitThread();
        };
        fenetreConnexion.Show();
    }

    private void AfficherMenuPrincipal(Utilisateur utilisateur)
    {
        AppConfig.UtilisateurConnecte = utilisateur;

        var menuPrincipal = new FormPrincipal(utilisateur);
        menuPrincipal.FormClosed += (_, _) => ExitThread();
        menuPrincipal.Show();
    }
}
