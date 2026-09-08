namespace BiologCorrectionRecus.Formulaires;

partial class FormPrincipal
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

    private DataGridView grilleRecus = null!;
    private Button boutonVerifier = null!;
    private Button boutonImprimer = null!;
    private Button boutonActualiser = null!;
    private StatusStrip barreStatut = null!;
    private ToolStripStatusLabel labelStatut = null!;

    private void InitializeComponent()
    {
        grilleRecus = new DataGridView();
        boutonVerifier = new Button();
        boutonImprimer = new Button();
        boutonActualiser = new Button();
        barreStatut = new StatusStrip();
        labelStatut = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)grilleRecus).BeginInit();
        barreStatut.SuspendLayout();
        SuspendLayout();

        // grilleRecus : liste des reçus, remplie en lecture seule (on ne modifie jamais
        // les données directement dans la grille — toute écriture passe par le repository).
        grilleRecus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grilleRecus.Location = new Point(12, 12);
        grilleRecus.Size = new Size(760, 400);
        grilleRecus.ReadOnly = true;
        grilleRecus.AllowUserToAddRows = false;
        grilleRecus.AllowUserToDeleteRows = false;
        grilleRecus.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grilleRecus.MultiSelect = false;
        grilleRecus.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grilleRecus.DoubleClick += GrilleRecus_DoubleClick;

        // boutonActualiser
        boutonActualiser.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        boutonActualiser.Location = new Point(12, 424);
        boutonActualiser.Size = new Size(140, 32);
        boutonActualiser.Text = "Actualiser la liste";
        boutonActualiser.Click += BoutonActualiser_Click;

        // boutonVerifier
        boutonVerifier.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        boutonVerifier.Location = new Point(162, 424);
        boutonVerifier.Size = new Size(160, 32);
        boutonVerifier.Text = "Vérifier maintenant";
        boutonVerifier.Click += BoutonVerifier_Click;

        // boutonImprimer
        boutonImprimer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        boutonImprimer.Location = new Point(332, 424);
        boutonImprimer.Size = new Size(220, 32);
        boutonImprimer.Text = "Imprimer le reçu sélectionné";
        boutonImprimer.Click += BoutonImprimer_Click;

        // barreStatut
        barreStatut.Items.Add(labelStatut);
        labelStatut.Text = "Prêt.";
        labelStatut.Spring = true;
        labelStatut.TextAlign = ContentAlignment.MiddleLeft;

        // FormPrincipal
        ClientSize = new Size(784, 491);
        Controls.Add(grilleRecus);
        Controls.Add(boutonActualiser);
        Controls.Add(boutonVerifier);
        Controls.Add(boutonImprimer);
        Controls.Add(barreStatut);
        MinimumSize = new Size(600, 400);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Biolog — Correction des reçus";
        ((System.ComponentModel.ISupportInitialize)grilleRecus).EndInit();
        barreStatut.ResumeLayout(false);
        barreStatut.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
