namespace BiologCorrectionRecus.Formulaires;

partial class LoginForm
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

    #region Code généré par le Concepteur Windows Form

    private Panel panelBarreTitre = null!;
    private Label lblFermer = null!;
    private Label lblTitreBarre = null!;
    private Panel panelCote = null!;
    private Label lblBienvenue = null!;
    private Label lblBienvenueSousTitre = null!;
    private Label lblParametres = null!;
    private Panel panelFormulaire = null!;
    private Label lblEnTeteFormulaire = null!;
    private Label lblUtilisateur = null!;
    private ComboBox cboUtilisateur = null!;
    private Label lblMotDePasse = null!;
    private TextBox txtMotDePasse = null!;
    private Label lblMessage = null!;
    private Button btnConnexion = null!;

    private void InitializeComponent()
    {
        panelBarreTitre = new Panel();
        lblFermer = new Label();
        lblTitreBarre = new Label();
        panelCote = new Panel();
        lblBienvenueSousTitre = new Label();
        lblBienvenue = new Label();
        lblParametres = new Label();
        panelFormulaire = new Panel();
        lblMessage = new Label();
        btnConnexion = new Button();
        txtMotDePasse = new TextBox();
        lblMotDePasse = new Label();
        cboUtilisateur = new ComboBox();
        lblUtilisateur = new Label();
        lblEnTeteFormulaire = new Label();
        panelBarreTitre.SuspendLayout();
        panelCote.SuspendLayout();
        panelFormulaire.SuspendLayout();
        SuspendLayout();

        // panelBarreTitre : bandeau du haut, sert aussi de "poignée" pour déplacer la fenêtre
        // (FormBorderStyle = None ci-dessous supprime la barre de titre système de Windows).
        panelBarreTitre.BackColor = Color.FromArgb(31, 42, 68);
        panelBarreTitre.Controls.Add(lblFermer);
        panelBarreTitre.Controls.Add(lblTitreBarre);
        panelBarreTitre.Dock = DockStyle.Top;
        panelBarreTitre.Location = new Point(0, 0);
        panelBarreTitre.Size = new Size(900, 36);
        panelBarreTitre.MouseDown += PanelBarreTitre_MouseDown;

        // lblFermer : croix de fermeture personnalisée (il n'y a plus de bouton système puisque
        // FormBorderStyle = None).
        lblFermer.Cursor = Cursors.Hand;
        lblFermer.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblFermer.ForeColor = Color.White;
        lblFermer.Location = new Point(860, 0);
        lblFermer.Size = new Size(40, 36);
        lblFermer.Text = "✕";
        lblFermer.TextAlign = ContentAlignment.MiddleCenter;
        lblFermer.Click += LblFermer_Click;

        lblTitreBarre.AutoSize = true;
        lblTitreBarre.Font = new Font("Segoe UI", 10F);
        lblTitreBarre.ForeColor = Color.White;
        lblTitreBarre.Location = new Point(16, 9);
        lblTitreBarre.Text = "Connexion";

        // panelCote : bandeau latéral gauche, purement visuel (message de bienvenue) + accès
        // discret aux paramètres du logiciel (nom du logiciel, emplacement du fichier .ini).
        panelCote.BackColor = Color.FromArgb(31, 42, 68);
        panelCote.Controls.Add(lblParametres);
        panelCote.Controls.Add(lblBienvenueSousTitre);
        panelCote.Controls.Add(lblBienvenue);
        panelCote.Dock = DockStyle.Left;
        panelCote.Location = new Point(0, 36);
        panelCote.Size = new Size(340, 484);

        lblBienvenueSousTitre.Font = new Font("Segoe UI", 10F);
        lblBienvenueSousTitre.ForeColor = Color.FromArgb(190, 200, 216);
        lblBienvenueSousTitre.Location = new Point(40, 230);
        lblBienvenueSousTitre.Size = new Size(260, 80);
        lblBienvenueSousTitre.Text = "Sélectionnez votre nom dans la liste et saisissez votre mot de passe pour accéder à l'application.";

        lblBienvenue.AutoSize = true;
        lblBienvenue.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
        lblBienvenue.ForeColor = Color.White;
        lblBienvenue.Location = new Point(37, 165);
        lblBienvenue.Text = "Biolog";

        // lblParametres : discret, en bas du bandeau, réservé à l'administrateur (nom du
        // logiciel + emplacement du fichier .ini — voir FormParametresLogiciel).
        lblParametres.AutoSize = true;
        lblParametres.Cursor = Cursors.Hand;
        lblParametres.Font = new Font("Segoe UI", 8.5F);
        lblParametres.ForeColor = Color.FromArgb(140, 152, 176);
        lblParametres.Location = new Point(40, 450);
        lblParametres.Text = "⚙ Paramètres du logiciel";
        lblParametres.Click += LblParametres_Click;

        // panelFormulaire : la partie droite, avec les vrais champs de connexion.
        panelFormulaire.BackColor = Color.White;
        panelFormulaire.Controls.Add(lblMessage);
        panelFormulaire.Controls.Add(btnConnexion);
        panelFormulaire.Controls.Add(txtMotDePasse);
        panelFormulaire.Controls.Add(lblMotDePasse);
        panelFormulaire.Controls.Add(cboUtilisateur);
        panelFormulaire.Controls.Add(lblUtilisateur);
        panelFormulaire.Controls.Add(lblEnTeteFormulaire);
        panelFormulaire.Dock = DockStyle.Fill;
        panelFormulaire.Location = new Point(340, 36);
        panelFormulaire.Size = new Size(560, 484);

        lblEnTeteFormulaire.AutoSize = true;
        lblEnTeteFormulaire.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblEnTeteFormulaire.ForeColor = Color.FromArgb(31, 42, 68);
        lblEnTeteFormulaire.Location = new Point(60, 50);
        lblEnTeteFormulaire.Text = "Correction des reçus";

        lblUtilisateur.AutoSize = true;
        lblUtilisateur.Font = new Font("Segoe UI", 9F);
        lblUtilisateur.ForeColor = Color.FromArgb(90, 98, 115);
        lblUtilisateur.Location = new Point(61, 112);
        lblUtilisateur.Text = "Utilisateur";

        // cboUtilisateur : DropDownList (pas Simple/DropDown) pour empêcher toute saisie libre
        // -- on ne peut choisir qu'un utilisateur existant dans la liste chargée depuis HFSQL.
        cboUtilisateur.DropDownStyle = ComboBoxStyle.DropDownList;
        cboUtilisateur.Font = new Font("Segoe UI", 10F);
        cboUtilisateur.Location = new Point(60, 132);
        cboUtilisateur.Size = new Size(440, 28);

        lblMotDePasse.AutoSize = true;
        lblMotDePasse.Font = new Font("Segoe UI", 9F);
        lblMotDePasse.ForeColor = Color.FromArgb(90, 98, 115);
        lblMotDePasse.Location = new Point(61, 182);
        lblMotDePasse.Text = "Mot de passe";

        txtMotDePasse.Font = new Font("Segoe UI", 10F);
        txtMotDePasse.Location = new Point(60, 202);
        txtMotDePasse.Size = new Size(440, 27);
        txtMotDePasse.UseSystemPasswordChar = true;
        txtMotDePasse.KeyDown += TxtMotDePasse_KeyDown;

        btnConnexion.BackColor = Color.FromArgb(45, 108, 223);
        btnConnexion.Cursor = Cursors.Hand;
        btnConnexion.FlatAppearance.BorderSize = 0;
        btnConnexion.FlatStyle = FlatStyle.Flat;
        btnConnexion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnConnexion.ForeColor = Color.White;
        btnConnexion.Location = new Point(60, 300);
        btnConnexion.Size = new Size(440, 42);
        btnConnexion.Text = "Se connecter";
        btnConnexion.UseVisualStyleBackColor = false;
        btnConnexion.Click += BtnConnexion_Click;

        lblMessage.Font = new Font("Segoe UI", 9F);
        lblMessage.ForeColor = Color.FromArgb(199, 44, 44);
        lblMessage.Location = new Point(60, 250);
        lblMessage.Size = new Size(440, 42);
        lblMessage.Text = "";

        // LoginForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(900, 520);
        Controls.Add(panelFormulaire);
        Controls.Add(panelCote);
        Controls.Add(panelBarreTitre);
        // FormBorderStyle = None : fenêtre "sans bordure système" (style moderne). C'est pour
        // ça qu'on redessine nous-mêmes une barre de titre (panelBarreTitre) avec sa propre
        // croix de fermeture et sa propre poignée de déplacement (voir PanelBarreTitre_MouseDown).
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Biolog — Connexion";
        Load += LoginForm_Load;
        panelBarreTitre.ResumeLayout(false);
        panelBarreTitre.PerformLayout();
        panelCote.ResumeLayout(false);
        panelCote.PerformLayout();
        panelFormulaire.ResumeLayout(false);
        panelFormulaire.PerformLayout();
        ResumeLayout(false);
    }

    #endregion
}
