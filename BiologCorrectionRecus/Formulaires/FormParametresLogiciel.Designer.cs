namespace BiologCorrectionRecus.Formulaires;

partial class FormParametresLogiciel
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

    private Label lblEntete = null!;
    private Label lblNomLogiciel = null!;
    private TextBox txtNomLogiciel = null!;
    private Label lblCheminIni = null!;
    private TextBox txtCheminIni = null!;
    private Button btnParcourir = null!;
    private Label lblMessage = null!;
    private Button btnEnregistrer = null!;
    private Button btnAnnuler = null!;
    private OpenFileDialog dialogueOuvrirFichier = null!;

    private void InitializeComponent()
    {
        lblEntete = new Label();
        lblNomLogiciel = new Label();
        txtNomLogiciel = new TextBox();
        lblCheminIni = new Label();
        txtCheminIni = new TextBox();
        btnParcourir = new Button();
        lblMessage = new Label();
        btnEnregistrer = new Button();
        btnAnnuler = new Button();
        dialogueOuvrirFichier = new OpenFileDialog();
        SuspendLayout();

        lblEntete.AutoSize = true;
        lblEntete.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblEntete.ForeColor = Color.FromArgb(31, 42, 68);
        lblEntete.Location = new Point(24, 20);
        lblEntete.Text = "Paramètres du logiciel";

        lblNomLogiciel.AutoSize = true;
        lblNomLogiciel.Font = new Font("Segoe UI", 9F);
        lblNomLogiciel.ForeColor = Color.FromArgb(90, 98, 115);
        lblNomLogiciel.Location = new Point(25, 68);
        lblNomLogiciel.Text = "Nom du logiciel";

        txtNomLogiciel.Font = new Font("Segoe UI", 10F);
        txtNomLogiciel.Location = new Point(24, 88);
        txtNomLogiciel.Size = new Size(400, 27);

        lblCheminIni.AutoSize = true;
        lblCheminIni.Font = new Font("Segoe UI", 9F);
        lblCheminIni.ForeColor = Color.FromArgb(90, 98, 115);
        lblCheminIni.Location = new Point(25, 128);
        lblCheminIni.Text = "Fichier d'initialisation (.ini) du logiciel";

        txtCheminIni.Font = new Font("Segoe UI", 10F);
        txtCheminIni.Location = new Point(24, 148);
        txtCheminIni.Size = new Size(320, 27);

        // dialogueOuvrirFichier : filtre les fichiers .ini uniquement, pour aider
        // l'administrateur à retrouver le bon fichier sans se tromper d'extension.
        dialogueOuvrirFichier.Filter = "Fichiers d'initialisation (*.ini)|*.ini|Tous les fichiers (*.*)|*.*";
        dialogueOuvrirFichier.Title = "Sélectionner le fichier d'initialisation du logiciel";

        btnParcourir.Location = new Point(350, 147);
        btnParcourir.Size = new Size(74, 29);
        btnParcourir.Text = "...";
        btnParcourir.UseVisualStyleBackColor = true;
        btnParcourir.Click += BtnParcourir_Click;

        lblMessage.Font = new Font("Segoe UI", 9F);
        lblMessage.ForeColor = Color.FromArgb(199, 44, 44);
        lblMessage.Location = new Point(24, 185);
        lblMessage.Size = new Size(400, 40);
        lblMessage.Text = "";

        btnEnregistrer.BackColor = Color.FromArgb(45, 108, 223);
        btnEnregistrer.FlatAppearance.BorderSize = 0;
        btnEnregistrer.FlatStyle = FlatStyle.Flat;
        btnEnregistrer.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnEnregistrer.ForeColor = Color.White;
        btnEnregistrer.Location = new Point(24, 235);
        btnEnregistrer.Size = new Size(190, 38);
        btnEnregistrer.Text = "Enregistrer";
        btnEnregistrer.UseVisualStyleBackColor = false;
        btnEnregistrer.Click += BtnEnregistrer_Click;

        btnAnnuler.Location = new Point(234, 235);
        btnAnnuler.Size = new Size(190, 38);
        btnAnnuler.Text = "Annuler";
        btnAnnuler.UseVisualStyleBackColor = true;
        btnAnnuler.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        // FormParametresLogiciel
        AcceptButton = btnEnregistrer;
        CancelButton = btnAnnuler;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(448, 294);
        Controls.Add(lblEntete);
        Controls.Add(lblNomLogiciel);
        Controls.Add(txtNomLogiciel);
        Controls.Add(lblCheminIni);
        Controls.Add(txtCheminIni);
        Controls.Add(btnParcourir);
        Controls.Add(lblMessage);
        Controls.Add(btnEnregistrer);
        Controls.Add(btnAnnuler);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Paramètres du logiciel";
        Load += FormParametresLogiciel_Load;
        ResumeLayout(false);
        PerformLayout();
    }
}
