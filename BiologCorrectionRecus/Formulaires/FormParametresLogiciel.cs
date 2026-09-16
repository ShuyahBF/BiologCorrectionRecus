using BiologCorrectionRecus.Configuration;

namespace BiologCorrectionRecus.Formulaires;

/// <summary>
/// Fenêtre d'administration accessible depuis l'écran de connexion : permet de modifier le nom
/// du logiciel et l'emplacement de son fichier d'initialisation (.ini) — la même logique que
/// pour vos autres logiciels (Aizenta, eKol, ...). Une fois enregistrés, ces deux paramètres
/// sont écrits dans appsettings.json, puis le fichier .ini est relu immédiatement pour mettre à
/// jour la base HFSQL utilisée (section [Serveur], clé "Nom").
/// </summary>
public partial class FormParametresLogiciel : Form
{
    public FormParametresLogiciel()
    {
        InitializeComponent();
    }

    private void FormParametresLogiciel_Load(object? sender, EventArgs e)
    {
        // Pré-remplit les champs avec la configuration actuellement chargée en mémoire
        // (voir AppConfig.Charger(), appelé au démarrage dans Program.cs).
        txtNomLogiciel.Text = AppConfig.NomLogiciel;
        txtCheminIni.Text = AppConfig.CheminFichierIni;
    }

    private void BtnParcourir_Click(object? sender, EventArgs e)
    {
        // Pré-positionne la boîte de dialogue sur le chemin déjà saisi, s'il existe, pour éviter
        // à l'administrateur de renaviguer depuis zéro à chaque modification.
        string dossierInitial = Path.GetDirectoryName(txtCheminIni.Text) ?? string.Empty;
        if (Directory.Exists(dossierInitial))
        {
            dialogueOuvrirFichier.InitialDirectory = dossierInitial;
        }

        if (dialogueOuvrirFichier.ShowDialog(this) == DialogResult.OK)
        {
            txtCheminIni.Text = dialogueOuvrirFichier.FileName;
        }
    }

    private void BtnEnregistrer_Click(object? sender, EventArgs e)
    {
        string nomLogiciel = txtNomLogiciel.Text.Trim();
        string cheminIni = txtCheminIni.Text.Trim();

        if (string.IsNullOrEmpty(nomLogiciel))
        {
            AfficherMessage("Veuillez saisir le nom du logiciel.");
            return;
        }

        if (string.IsNullOrEmpty(cheminIni))
        {
            AfficherMessage("Veuillez indiquer l'emplacement du fichier d'initialisation (.ini).");
            return;
        }

        try
        {
            // Met à jour la configuration en mémoire, puis la persiste dans appsettings.json
            // (AppConfig.Enregistrer()) pour qu'elle soit reprise au prochain démarrage.
            AppConfig.NomLogiciel = nomLogiciel;
            AppConfig.CheminFichierIni = cheminIni;
            AppConfig.Enregistrer();

            // Relit immédiatement le fichier .ini pointé, pour que l'écran de connexion se
            // reconnecte tout de suite avec la bonne base (sans avoir à relancer l'application).
            AppConfig.ChargerDepuisFichierIni();

            if (!string.IsNullOrEmpty(AppConfig.DerniereErreurIni))
            {
                // Les paramètres sont bien enregistrés ; seule la lecture du .ini a échoué
                // (chemin incorrect, section/clé absente...) — on prévient sans bloquer.
                AfficherMessage(AppConfig.DerniereErreurIni);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            AfficherMessage("Impossible d'enregistrer les paramètres : " + ex.Message);
        }
    }

    private void AfficherMessage(string message) => lblMessage.Text = message;
}
