using BiologCorrectionRecus.Configuration;

namespace BiologCorrectionRecus;

/// <summary>
/// Point d'entrée de l'application (équivalent du "code du projet" en WinDev).
/// C'est ici que la configuration HFSQL est chargée, puis que la fenêtre de connexion est
/// lancée en premier.
/// </summary>
internal static class Program
{
    [STAThread] // Obligatoire pour toute application WinForms (modèle COM mono-thread)
    private static void Main()
    {
        // Active le rendu visuel moderne des contrôles Windows (thème du système)
        ApplicationConfiguration.Initialize();

        // Chargement des paramètres de connexion HFSQL depuis appsettings.json (voir
        // Configuration/AppConfig.cs). A faire avant toute tentative de connexion au serveur.
        AppConfig.Charger();

        // L'enchaînement des fenêtres (Connexion -> Menu principal) est piloté par
        // LoginApplicationContext, ce qui évite d'avoir une fenêtre de connexion "fantôme"
        // ouverte en tâche de fond une fois l'utilisateur authentifié.
        Application.Run(new LoginApplicationContext());
    }
}
