namespace BiologCorrectionRecus.Formulaires;

partial class FormConnexion
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    // Déclaration des contrôles du formulaire (généralement générée par le concepteur
    // visuel de Visual Studio quand on glisse-dépose des contrôles sur la fenêtre).
    private Label labelLogin = null!;
    private TextBox texteLogin = null!;
    private Label labelMotDePasse = null!;
    private TextBox texteMotDePasse = null!;
    private Button boutonConnexion = null!;
    private Label labelMessage = null!;

    /// <summary>
    /// Construit et positionne tous les contrôles visuels du formulaire.
    /// Fait "à la main" ici (plutôt que via le concepteur graphique) puisque ce
    /// fichier a été écrit sans accès à Visual Studio — vous pourrez librement
    /// rouvrir ce formulaire dans le concepteur visuel pour l'ajuster ensuite.
    /// </summary>
    private void InitializeComponent()
    {
        labelLogin = new Label();
        texteLogin = new TextBox();
        labelMotDePasse = new Label();
        texteMotDePasse = new TextBox();
        boutonConnexion = new Button();
        labelMessage = new Label();
        SuspendLayout();

        // labelLogin
        labelLogin.AutoSize = true;
        labelLogin.Location = new Point(30, 30);
        labelLogin.Text = "Identifiant :";

        // texteLogin
        texteLogin.Location = new Point(130, 27);
        texteLogin.Size = new Size(200, 23);

        // labelMotDePasse
        labelMotDePasse.AutoSize = true;
        labelMotDePasse.Location = new Point(30, 65);
        labelMotDePasse.Text = "Mot de passe :";

        // texteMotDePasse
        texteMotDePasse.Location = new Point(130, 62);
        texteMotDePasse.Size = new Size(200, 23);
        texteMotDePasse.UseSystemPasswordChar = true; // masque la saisie du mot de passe

        // boutonConnexion
        boutonConnexion.Location = new Point(130, 100);
        boutonConnexion.Size = new Size(120, 30);
        boutonConnexion.Text = "Se connecter";
        boutonConnexion.Click += BoutonConnexion_Click;

        // labelMessage (affiche les erreurs d'authentification)
        labelMessage.AutoSize = true;
        labelMessage.ForeColor = Color.DarkRed;
        labelMessage.Location = new Point(30, 140);
        labelMessage.Size = new Size(300, 20);

        // FormConnexion
        AcceptButton = boutonConnexion; // Entrée = valider le formulaire
        ClientSize = new Size(370, 190);
        Controls.Add(labelLogin);
        Controls.Add(texteLogin);
        Controls.Add(labelMotDePasse);
        Controls.Add(texteMotDePasse);
        Controls.Add(boutonConnexion);
        Controls.Add(labelMessage);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Biolog — Connexion";
        ResumeLayout(false);
        PerformLayout();
    }
}
