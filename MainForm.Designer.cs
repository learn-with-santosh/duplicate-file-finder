namespace DuplicateFileFinder;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    // Controls
    private Label lblSelectPath;
    private TextBox txtPath;
    private Button btnBrowse;
    private GroupBox grpFileTypes;
    private CheckBox chkImages;
    private CheckBox chkVideos;
    private CheckBox chkPdf;
    private CheckBox chkDocuments;
    private CheckBox chkAudio;
    private CheckBox chkAllFiles;
    private Button btnScan;
    private Button btnCancel;
    private Button btnDeleteSelected;
    private Button btnSelectAllDuplicates;
    private ProgressBar progressBar;
    private Label lblStatus;
    private ListView listViewResults;
    private ColumnHeader colCheck;
    private ColumnHeader colFileName;
    private ColumnHeader colFilePath;
    private ColumnHeader colSize;
    private ColumnHeader colGroup;
    private Label lblSummary;
    private Panel pnlPreview;
    private PictureBox picPreview;
    private Label lblFileNamePreview;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        // ===== Form =====
        this.Text = "Duplicate File Finder";
        this.Size = new Size(1050, 720);
        this.MinimumSize = new Size(850, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 9F);

        int y = 15;

        // ===== Path Selection =====
        lblSelectPath = new Label
        {
            Text = "Scan Location:",
            Location = new Point(15, y + 3),
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        this.Controls.Add(lblSelectPath);

        txtPath = new TextBox
        {
            Location = new Point(130, y),
            Size = new Size(750, 25),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            BackColor = Color.White
        };
        this.Controls.Add(txtPath);

        btnBrowse = new Button
        {
            Text = "Browse...",
            Location = new Point(890, y - 1),
            Size = new Size(120, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.System
        };
        this.Controls.Add(btnBrowse);

        y += 40;

        // ===== File Types Group =====
        grpFileTypes = new GroupBox
        {
            Text = "File Types to Scan",
            Location = new Point(15, y),
            Size = new Size(995, 55),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        this.Controls.Add(grpFileTypes);

        chkImages = new CheckBox { Text = "Images", Location = new Point(15, 22), AutoSize = true, Checked = true };
        chkVideos = new CheckBox { Text = "Videos", Location = new Point(120, 22), AutoSize = true, Checked = true };
        chkPdf = new CheckBox { Text = "PDF", Location = new Point(220, 22), AutoSize = true, Checked = true };
        chkDocuments = new CheckBox { Text = "Documents", Location = new Point(300, 22), AutoSize = true, Checked = false };
        chkAudio = new CheckBox { Text = "Audio", Location = new Point(420, 22), AutoSize = true, Checked = false };
        chkAllFiles = new CheckBox { Text = "All Files", Location = new Point(530, 22), AutoSize = true, Checked = false, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

        grpFileTypes.Controls.AddRange(new Control[] { chkImages, chkVideos, chkPdf, chkDocuments, chkAudio, chkAllFiles });

        y += 65;

        // ===== Buttons Row =====
        btnScan = new Button
        {
            Text = "🔍 Scan for Duplicates",
            Location = new Point(15, y),
            Size = new Size(170, 35),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };
        this.Controls.Add(btnScan);

        btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(195, y),
            Size = new Size(90, 35),
            Enabled = false,
            FlatStyle = FlatStyle.Flat
        };
        this.Controls.Add(btnCancel);

        btnSelectAllDuplicates = new Button
        {
            Text = "Select All Duplicates (Keep First)",
            Location = new Point(295, y),
            Size = new Size(230, 35),
            Enabled = false,
            FlatStyle = FlatStyle.Flat
        };
        this.Controls.Add(btnSelectAllDuplicates);

        btnDeleteSelected = new Button
        {
            Text = "🗑 Delete Selected to Recycle Bin",
            Location = new Point(535, y),
            Size = new Size(230, 35),
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Enabled = false
        };
        this.Controls.Add(btnDeleteSelected);

        y += 45;

        // ===== Progress =====
        progressBar = new ProgressBar
        {
            Location = new Point(15, y),
            Size = new Size(995, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Style = ProgressBarStyle.Continuous
        };
        this.Controls.Add(progressBar);

        y += 26;

        lblStatus = new Label
        {
            Text = "Ready. Select a folder or drive and click Scan.",
            Location = new Point(15, y),
            Size = new Size(995, 20),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ForeColor = Color.DarkSlateGray
        };
        this.Controls.Add(lblStatus);

        y += 25;

        // ===== Results ListView =====
        listViewResults = new ListView
        {
            Location = new Point(15, y),
            Size = new Size(735, 340),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false // Selection for preview
        };

        // ===== Preview Panel =====
        pnlPreview = new Panel
        {
            Location = new Point(765, y),
            Size = new Size(245, 340),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(250, 250, 250)
        };
        this.Controls.Add(pnlPreview);

        lblFileNamePreview = new Label
        {
            Text = "Select a file to preview",
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
        pnlPreview.Controls.Add(lblFileNamePreview);

        picPreview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White
        };
        pnlPreview.Controls.Add(picPreview);

        colCheck = new ColumnHeader { Text = "", Width = 30 };
        colGroup = new ColumnHeader { Text = "Group", Width = 60 };
        colFileName = new ColumnHeader { Text = "File Name", Width = 250 };
        colSize = new ColumnHeader { Text = "Size", Width = 100 };
        colFilePath = new ColumnHeader { Text = "Full Path", Width = 540 };

        listViewResults.Columns.AddRange(new[] { colCheck, colGroup, colFileName, colSize, colFilePath });
        this.Controls.Add(listViewResults);

        y += 345;

        // ===== Summary =====
        lblSummary = new Label
        {
            Text = "",
            Location = new Point(15, y),
            Size = new Size(995, 25),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.DarkGreen
        };
        this.Controls.Add(lblSummary);

        this.ResumeLayout(false);
    }
}
