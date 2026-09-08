using BiologCorrectionRecus.Formulaires;

namespace BiologCorrectionRecus;

/// <summary>
/// Point d'entrée de l'application (équivalent du "code du projet" en WinDev).
/// C'est ici que la fenêtre de connexion est lancée en premier.
/// </summary>
internal static class Program
{
    [STAThread] // Obligatoire pour toute application WinForms (modèle COM mono-thread)
    private static void Main()
    {
        // Active le rendu visuel moderne des contrôles Windows (thème du système)
        ApplicationConfiguration.Initialize();

        // On démarre toujours par l'écran de connexion (authentification UtilisateurBlg).
        // FormPrincipal n'est ouvert qu'après une authentification réussie.
        Application.Run(new FormConnexion());
    }
}
