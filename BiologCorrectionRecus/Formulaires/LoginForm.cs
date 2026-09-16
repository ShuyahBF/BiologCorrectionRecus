using System.Runtime.InteropServices;
using BiologCorrectionRecus.Configuration;
using BiologCorrectionRecus.Data;
using BiologCorrectionRecus.Modeles;

namespace BiologCorrectionRecus.Formulaires;

/// <summary>
/// Fenêtre de connexion : l'utilisateur choisit son nom dans une liste alimentée par la table
/// HFSQL des utilisateurs (UtilisateurBlg), saisit son mot de passe puis clique sur
/// "Se connecter". Reprend telle quelle la solution éprouvée du projet "HFSQL_LoginApp"
/// (dépôt Claude), à réutiliser dans tout futur projet Biolog ayant besoin de se connecter à
/// un serveur HFSQL : ComboBox d'utilisateurs, mot de passe, 3 tentatives maximum.
/// </summary>
public partial class LoginForm : Form
{
    private const int NombreMaxTentatives = 3;
    private int tentativesRestantes = NombreMaxTentatives;

    /// <summary>
    /// Déclenché lorsque l'authentification a réussi. LoginApplicationContext (voir
    /// LoginApplicationContext.cs) écoute cet évènement pour ouvrir FormPrincipal ensuite.
    /// </summary>
    public event EventHandler<Utilisateur>? ConnexionReussie;

    public LoginForm()
    {
        InitializeComponent();
    }

    private void LoginForm_Load(object? sender, EventArgs e)
    {
        // Le nom affiché ("Biolog" par défaut) suit désormais AppConfig.NomLogiciel : le même
        // écran de connexion peut ainsi être réutilisé tel quel pour Aizenta, eKol, etc. en ne
        // changeant que ce paramètre (voir FormParametresLogiciel).
        lblBienvenue.Text = AppConfig.NomLogiciel;
        Text = $"{AppConfig.NomLogiciel} — Connexion";
        AfficherResumeConnexion();

        if (!string.IsNullOrEmpty(AppConfig.DerniereErreurIni))
        {
            AfficherMessage(AppConfig.DerniereErreurIni);
        }

        ChargerListeUtilisateurs();
    }

    /// <summary>
    /// Affiche "serveur[:port] · base" à côté du lien "Paramètres du logiciel", pour voir en un
    /// coup d'œil à quel serveur/base HFSQL l'application est actuellement connectée
    /// (résolu par AppConfig depuis appsettings.json puis le fichier .ini, voir AppConfig.cs).
    /// </summary>
    private void AfficherResumeConnexion() => lblResumeConnexion.Text = AppConfig.ResumeConnexion;

    private void ChargerListeUtilisateurs()
    {
        try
        {
            cboUtilisateur.DataSource = HfsqlConnectionManager.ChargerUtilisateurs();

            if (cboUtilisateur.Items.Count == 0)
            {
                AfficherMessage("Aucun utilisateur trouvé dans la table des utilisateurs.");
            }
        }
        catch (Exception ex)
        {
            // Erreur de connexion au serveur HFSQL lui-même (mauvais nom de serveur/base,
            // pilote ODBC introuvable, etc.) : c'est ce message qu'il faut remonter pour
            // ajuster appsettings.json.
            AfficherMessage("Connexion au serveur HFSQL impossible : " + ex.Message);
        }
    }

    private void BtnConnexion_Click(object? sender, EventArgs e)
    {
        if (cboUtilisateur.SelectedItem is not Utilisateur utilisateurSelectionne)
        {
            AfficherMessage("Veuillez sélectionner un utilisateur dans la liste.");
            return;
        }

        if (string.IsNullOrEmpty(txtMotDePasse.Text))
        {
            AfficherMessage("Veuillez saisir votre mot de passe.");
            return;
        }

        btnConnexion.Enabled = false;
        try
        {
            Utilisateur? utilisateurAuthentifie = HfsqlConnectionManager.Authentifier(
                utilisateurSelectionne.Login, txtMotDePasse.Text);

            if (utilisateurAuthentifie != null)
            {
                ConnexionReussie?.Invoke(this, utilisateurAuthentifie);
                Close();
                return;
            }

            TraiterEchecConnexion();
        }
        catch (Exception ex)
        {
            AfficherMessage("Erreur de connexion au serveur HFSQL : " + ex.Message);
        }
        finally
        {
            btnConnexion.Enabled = true;
        }
    }

    private void TraiterEchecConnexion()
    {
        tentativesRestantes--;

        if (tentativesRestantes <= 0)
        {
            MessageBox.Show(
                "Identifiants incorrects. Nombre maximal de tentatives atteint (" + NombreMaxTentatives + ").",
                "Accès refusé",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            Close();
            return;
        }

        AfficherMessage($"Login ou mot de passe incorrect. Tentative(s) restante(s) : {tentativesRestantes}.");
        txtMotDePasse.Clear();
        txtMotDePasse.Focus();
    }

    private void AfficherMessage(string message) => lblMessage.Text = message;

    private void TxtMotDePasse_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            BtnConnexion_Click(sender, EventArgs.Empty);
        }
    }

    private void LblFermer_Click(object? sender, EventArgs e) => Close();

    /// <summary>
    /// Ouvre la fenêtre d'administration (nom du logiciel + emplacement de son fichier .ini).
    /// Si l'administrateur enregistre une modification, on relit aussitôt la liste des
    /// utilisateurs : elle pointera alors vers la base HFSQL indiquée dans le nouveau .ini.
    /// </summary>
    private void LblParametres_Click(object? sender, EventArgs e)
    {
        using var formulaireParametres = new FormParametresLogiciel();
        if (formulaireParametres.ShowDialog(this) == DialogResult.OK)
        {
            lblBienvenue.Text = AppConfig.NomLogiciel;
            Text = $"{AppConfig.NomLogiciel} — Connexion";
            AfficherResumeConnexion();
            AfficherMessage(string.Empty);
            ChargerListeUtilisateurs();
        }
    }

    // ----- Déplacement de la fenêtre (FormBorderStyle = None) via la barre de titre personnalisée -----
    // Windows ne sait pas déplacer une fenêtre sans bordure au glisser-déposer : on simule un
    // clic sur la "barre de titre système" (HT_CAPTION) via l'API Windows (user32.dll) dès que
    // l'utilisateur clique-glisse sur notre bandeau du haut.

    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HT_CAPTION = 0x2;

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    private void PanelBarreTitre_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        ReleaseCapture();
        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
    }
}
