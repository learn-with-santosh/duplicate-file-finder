using Microsoft.VisualBasic.FileIO;

namespace DuplicateFileFinder;

public partial class MainForm : Form
{
    private DuplicateScanner? _scanner;
    private Dictionary<string, List<FileItem>>? _duplicates;

    // Extension sets
    private static readonly HashSet<string> ImageExtensions = new()
    { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".svg", ".ico", ".raw", ".heic", ".heif" };

    private static readonly HashSet<string> VideoExtensions = new()
    { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg", ".3gp", ".ts" };

    private static readonly HashSet<string> PdfExtensions = new()
    { ".pdf" };

    private static readonly HashSet<string> DocExtensions = new()
    { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".ods", ".odp", ".txt", ".rtf", ".csv" };

    private static readonly HashSet<string> AudioExtensions = new()
    { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".opus" };

    public MainForm()
    {
        InitializeComponent();
        WireEvents();
    }

    private void WireEvents()
    {
        btnBrowse.Click += BtnBrowse_Click;
        btnScan.Click += BtnScan_Click;
        btnCancel.Click += BtnCancel_Click;
        btnDeleteSelected.Click += BtnDeleteSelected_Click;
        btnSelectAllDuplicates.Click += BtnSelectAllDuplicates_Click;
        chkAllFiles.CheckedChanged += ChkAllFiles_CheckedChanged;
        listViewResults.ItemChecked += ListViewResults_ItemChecked;
        listViewResults.MouseDoubleClick += ListViewResults_MouseDoubleClick;
        listViewResults.SelectedIndexChanged += ListViewResults_SelectedIndexChanged;
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Select a drive or folder to scan for duplicates",
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtPath.Text = dlg.SelectedPath;
        }
    }

    private void ChkAllFiles_CheckedChanged(object? sender, EventArgs e)
    {
        bool allChecked = chkAllFiles.Checked;
        chkImages.Enabled = !allChecked;
        chkVideos.Enabled = !allChecked;
        chkPdf.Enabled = !allChecked;
        chkDocuments.Enabled = !allChecked;
        chkAudio.Enabled = !allChecked;
    }

    private HashSet<string> GetSelectedExtensions()
    {
        if (chkAllFiles.Checked)
            return new HashSet<string>(); // empty = all files

        var extensions = new HashSet<string>();

        if (chkImages.Checked)
            foreach (var ext in ImageExtensions) extensions.Add(ext);

        if (chkVideos.Checked)
            foreach (var ext in VideoExtensions) extensions.Add(ext);

        if (chkPdf.Checked)
            foreach (var ext in PdfExtensions) extensions.Add(ext);

        if (chkDocuments.Checked)
            foreach (var ext in DocExtensions) extensions.Add(ext);

        if (chkAudio.Checked)
            foreach (var ext in AudioExtensions) extensions.Add(ext);

        return extensions;
    }

    private async void BtnScan_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtPath.Text))
        {
            MessageBox.Show("Please select a folder or drive to scan.", "No Path Selected",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Directory.Exists(txtPath.Text))
        {
            MessageBox.Show("The selected path does not exist.", "Invalid Path",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var extensions = GetSelectedExtensions();
        if (!chkAllFiles.Checked && extensions.Count == 0)
        {
            MessageBox.Show("Please select at least one file type or check 'All Files'.", "No File Type",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // UI state: scanning
        SetScanningUI(true);
        listViewResults.Items.Clear();
        lblSummary.Text = "";
        progressBar.Value = 0;

        _scanner = new DuplicateScanner();
        _scanner.StatusUpdate += msg => BeginInvoke(() => lblStatus.Text = msg);
        _scanner.ProgressUpdate += pct => BeginInvoke(() =>
        {
            if (pct >= 0 && pct <= 100) progressBar.Value = pct;
        });

        try
        {
            _duplicates = await Task.Run(() => _scanner.ScanForDuplicatesAsync(txtPath.Text, extensions));
            DisplayResults(_duplicates);
        }
        catch (OperationCanceledException)
        {
            lblStatus.Text = "Scan cancelled.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during scan: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetScanningUI(false);
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        _scanner?.Cancel();
        lblStatus.Text = "Cancelling...";
    }

    private void DisplayResults(Dictionary<string, List<FileItem>> duplicates)
    {
        listViewResults.BeginUpdate();
        listViewResults.Items.Clear();

        int groupNum = 1;
        int totalDuplicateFiles = 0;
        long totalWastedSpace = 0;

        // Alternate colors for groups
        Color[] groupColors = { Color.White, Color.FromArgb(240, 248, 255) };

        foreach (var group in duplicates.Values.OrderByDescending(g => g[0].FileSize))
        {
            Color bgColor = groupColors[groupNum % 2];
            bool isFirst = true;

            foreach (var file in group)
            {
                var item = new ListViewItem("") // checkbox column
                {
                    Tag = file,
                    BackColor = bgColor
                };

                item.SubItems.Add($"Group {groupNum}");
                item.SubItems.Add(file.FileName);
                item.SubItems.Add(file.FileSizeFormatted);
                item.SubItems.Add(file.FilePath);

                // Mark first file in group with bold (the "original")
                if (isFirst)
                {
                    item.Font = new Font(listViewResults.Font, FontStyle.Bold);
                    isFirst = false;
                }

                listViewResults.Items.Add(item);
                totalDuplicateFiles++;
            }

            // Each group wastes (count-1) * filesize
            totalWastedSpace += (group.Count - 1) * group[0].FileSize;
            groupNum++;
        }

        listViewResults.EndUpdate();
        progressBar.Value = 100;

        int groupCount = duplicates.Count;
        string wastedStr = FormatBytes(totalWastedSpace);

        lblSummary.Text = $"Found {groupCount} duplicate groups ({totalDuplicateFiles} files). " +
                          $"Potential space savings: {wastedStr}";
        lblStatus.Text = groupCount > 0
            ? "Scan complete. Review duplicates below. Bold = first occurrence (kept by 'Select All Duplicates')."
            : "Scan complete. No duplicates found!";

        btnSelectAllDuplicates.Enabled = groupCount > 0;
        btnDeleteSelected.Enabled = groupCount > 0;
    }

    private void BtnSelectAllDuplicates_Click(object? sender, EventArgs e)
    {
        if (_duplicates == null) return;

        // Build a set of "first file" paths to keep
        var keepPaths = new HashSet<string>();
        foreach (var group in _duplicates.Values)
        {
            if (group.Count > 0)
                keepPaths.Add(group[0].FilePath);
        }

        listViewResults.BeginUpdate();
        foreach (ListViewItem item in listViewResults.Items)
        {
            var fileItem = (FileItem)item.Tag;
            item.Checked = !keepPaths.Contains(fileItem.FilePath);
        }
        listViewResults.EndUpdate();

        UpdateDeleteButtonText();
    }

    private void ListViewResults_ItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        UpdateDeleteButtonText();
    }

    private void ListViewResults_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (listViewResults.SelectedItems.Count > 0)
        {
            var fileItem = (FileItem)listViewResults.SelectedItems[0].Tag;
            try
            {
                // Open folder and select file
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{fileItem.FilePath}\"");
            }
            catch { }
        }
    }

    private void ListViewResults_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (listViewResults.SelectedItems.Count > 0)
        {
            var fileItem = (FileItem)listViewResults.SelectedItems[0].Tag;
            UpdatePreview(fileItem);
        }
        else
        {
            ClearPreview();
        }
    }

    private void UpdatePreview(FileItem file)
    {
        try
        {
            lblFileNamePreview.Text = file.FileName;

            // Dispose old image if any
            var oldImage = picPreview.Image;
            picPreview.Image = null;
            oldImage?.Dispose();

            if (!File.Exists(file.FilePath)) return;

            string ext = file.Extension;
            if (ImageExtensions.Contains(ext))
            {
                // Loading image from stream to avoid locking the file
                using (var stream = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read))
                {
                    picPreview.Image = Image.FromStream(stream);
                }
            }
            else
            {
                // Show associated icon for other files
                try
                {
                    using (Icon? icon = Icon.ExtractAssociatedIcon(file.FilePath))
                    {
                        if (icon != null)
                            picPreview.Image = icon.ToBitmap();
                    }
                }
                catch
                {
                    picPreview.Image = null;
                }
            }
        }
        catch
        {
            lblFileNamePreview.Text = "Preview not available";
        }
    }

    private void ClearPreview()
    {
        lblFileNamePreview.Text = "Select a file to preview";
        var oldImage = picPreview.Image;
        picPreview.Image = null;
        oldImage?.Dispose();
    }

    private void UpdateDeleteButtonText()
    {
        int checkedCount = listViewResults.CheckedItems.Count;
        btnDeleteSelected.Text = $"🗑 Delete Selected ({checkedCount}) to Recycle Bin";
        btnDeleteSelected.Enabled = checkedCount > 0;
    }

    private void BtnDeleteSelected_Click(object? sender, EventArgs e)
    {
        var checkedItems = listViewResults.CheckedItems;
        if (checkedItems.Count == 0)
        {
            MessageBox.Show("No files selected for deletion.", "Nothing Selected",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Calculate total size
        long totalSize = 0;
        foreach (ListViewItem item in checkedItems)
        {
            var fi = (FileItem)item.Tag;
            totalSize += fi.FileSize;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to move {checkedItems.Count} file(s) ({FormatBytes(totalSize)}) to the Recycle Bin?\n\n" +
            "You can restore them from the Recycle Bin if needed.",
            "Confirm Deletion",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        int deleted = 0;
        int failed = 0;
        var itemsToRemove = new List<ListViewItem>();

        foreach (ListViewItem item in checkedItems)
        {
            var fileItem = (FileItem)item.Tag;
            try
            {
                if (File.Exists(fileItem.FilePath))
                {
                    // Send to Recycle Bin
                    FileSystem.DeleteFile(fileItem.FilePath,
                        UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin);
                    itemsToRemove.Add(item);
                    deleted++;
                }
                else
                {
                    itemsToRemove.Add(item); // file already gone
                    deleted++;
                }
            }
            catch (Exception ex)
            {
                failed++;
                System.Diagnostics.Debug.WriteLine($"Failed to delete {fileItem.FilePath}: {ex.Message}");
            }
        }

        // Remove deleted items from list
        listViewResults.BeginUpdate();
        foreach (var item in itemsToRemove)
            listViewResults.Items.Remove(item);
        listViewResults.EndUpdate();

        // Also clean up _duplicates dictionary
        if (_duplicates != null)
        {
            var deletedPaths = itemsToRemove
                .Select(i => ((FileItem)i.Tag).FilePath)
                .ToHashSet();

            foreach (var key in _duplicates.Keys.ToList())
            {
                _duplicates[key].RemoveAll(f => deletedPaths.Contains(f.FilePath));
                if (_duplicates[key].Count < 2)
                    _duplicates.Remove(key);
            }
        }

        string msg = $"Successfully moved {deleted} file(s) to Recycle Bin.";
        if (failed > 0)
            msg += $"\n{failed} file(s) could not be deleted.";

        lblStatus.Text = msg;
        UpdateDeleteButtonText();

        MessageBox.Show(msg, "Deletion Complete",
            MessageBoxButtons.OK, failed > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
    }

    private void SetScanningUI(bool scanning)
    {
        btnScan.Enabled = !scanning;
        btnBrowse.Enabled = !scanning;
        btnCancel.Enabled = scanning;
        btnDeleteSelected.Enabled = !scanning && listViewResults.CheckedItems.Count > 0;
        btnSelectAllDuplicates.Enabled = !scanning;
        grpFileTypes.Enabled = !scanning;

        if (scanning)
        {
            this.Cursor = Cursors.WaitCursor;
        }
        else
        {
            this.Cursor = Cursors.Default;
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
