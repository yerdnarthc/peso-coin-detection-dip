using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoinDenominationDIP
{
    public partial class Form1 : Form
    {
        private Bitmap original;
        private CoinAnalysis analysis;
        private bool busy;
        private readonly int[] denominations = { 5, 10, 25, 100, 500 };

        public Form1()
        {
            InitializeComponent();
            openButton.Click += OpenImage;
            sampleButton.Click += LoadSample;
            processButton.Click += ProcessImage;
            maskCheckBox.CheckedChanged += delegate { UpdatePreview(); };
            darkModeCheckBox.CheckedChanged += delegate { ApplyTheme(); };
            thresholdInput.ValueChanged += delegate
            {
                ClearResults();
                statusLabel.Text = original == null ? "Open an image or load the sample." : "Threshold changed. Click Count Coins.";
            };
            FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                // Do not dispose the form while the background task owns its working image.
                if (busy) { e.Cancel = true; statusLabel.Text = "Counting coins. Please wait before closing."; }
            };
            ClearResults();
            ApplyTheme();
        }

        private void OpenImage(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Open a coin image";
                dialog.Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK) LoadImage(dialog.FileName);
            }
        }

        private void LoadSample(object sender, EventArgs e)
        {
            LoadImage(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input-images", "philippine-peso-coins.jpeg"));
        }

        private void LoadImage(string path)
        {
            Bitmap loaded = null;
            try
            {
                // Copy decoded pixels so the source file is not kept locked.
                using (var image = Image.FromFile(path))
                {
                    if ((long)image.Width * image.Height > 12000000)
                        throw new ArgumentException("Please use an image of at most 12 million pixels.");
                    loaded = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
                    using (Graphics graphics = Graphics.FromImage(loaded))
                    {
                        graphics.Clear(Color.White);
                        graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                    }
                }
                ClearResults();
                originalPicture.Image = null;
                if (original != null) original.Dispose();
                original = loaded;
                loaded = null;
                originalPicture.Image = original;
                processButton.Enabled = true;
                statusLabel.Text = Path.GetFileName(path) + " — " + original.Width + " × " + original.Height + ". Click Count Coins.";
            }
            catch (Exception error)
            {
                if (loaded != null) loaded.Dispose();
                ClearResults();
                statusLabel.Text = "Could not open the image.";
                MessageBox.Show(this, "Could not open this image.\n\n" + error.Message,
                    "Image error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ProcessImage(object sender, EventArgs e)
        {
            if (original == null || busy) return;
            ClearResults();
            SetBusy(true);
            statusLabel.Text = "Counting coins…";
            Bitmap working = null;
            try
            {
                working = new Bitmap(original);
                int threshold = (int)thresholdInput.Value;
                analysis = await Task.Run(() => CoinAnalyzer.Analyze(working, threshold));
                UpdatePreview();
                int totalCentavos = 0;
                for (int row = 0; row < denominations.Length; row++)
                {
                    int value = denominations[row];
                    int count = analysis.Coins.Count(coin => coin.ValueCentavos == value);
                    resultsGrid.Rows[row].Cells[1].Value = count;
                    resultsGrid.Rows[row].Cells[2].Value = Peso(count * value);
                    totalCentavos += count * value;
                }
                int unknown = analysis.Coins.Count(coin => coin.ValueCentavos == 0);
                totalLabel.Text = "Total: " + Peso(totalCentavos);
                unknownLabel.Visible = unknown > 0;
                countLabel.Text = "Coins: " + analysis.Coins.Count + "    Unknown: " + unknown;
                statusLabel.Text = analysis.Coins.Count == 0 ? "No coins detected. Check the image and threshold."
                    : unknown > 0 ? "Count complete. Orange boxes mark unknown coins."
                    : analysis.Coins.Count + " coins counted.";
            }
            catch (Exception error)
            {
                ClearResults();
                statusLabel.Text = "Could not count coins. Try another image or threshold.";
                MessageBox.Show(this, "Could not process the image.\n\n" + error.Message,
                    "Processing error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (working != null) working.Dispose();
                SetBusy(false);
            }
        }

        private static string Peso(int centavos)
        {
            return "₱" + (centavos / 100m).ToString("N2");
        }

        private void SetBusy(bool value)
        {
            busy = value;
            openButton.Enabled = sampleButton.Enabled = thresholdInput.Enabled = !value;
            processButton.Enabled = !value && original != null;
            maskCheckBox.Enabled = !value && analysis != null;
            progressBar.Visible = value;
            UseWaitCursor = value;
        }

        private void UpdatePreview()
        {
            resultPicture.Image = analysis == null ? null
                : maskCheckBox.Checked ? analysis.Mask : analysis.AnnotatedImage;
            resultGroup.Text = maskCheckBox.Checked ? "Black-and-white" : "Coins";
        }

        private void ClearResults()
        {
            resultPicture.Image = null;
            if (analysis != null) { analysis.Dispose(); analysis = null; }
            maskCheckBox.Enabled = false;
            maskCheckBox.Checked = false;
            resultGroup.Text = "Coins";
            resultsGrid.Rows.Clear();
            foreach (int value in denominations)
                resultsGrid.Rows.Add(value < 100 ? value + " centavos" : "₱" + value / 100, "—", "—");
            totalLabel.Text = "Total: —";
            countLabel.Text = "Coins: —";
            unknownLabel.Visible = false;
            resultsGrid.ClearSelection();
        }

        private void ApplyTheme()
        {
            bool dark = darkModeCheckBox.Checked;
            Color background = dark ? Color.FromArgb(30, 30, 30) : Color.FromArgb(245, 245, 245);
            Color surface = dark ? Color.FromArgb(42, 42, 42) : Color.White;
            Color text = dark ? Color.FromArgb(235, 235, 235) : Color.FromArgb(30, 30, 30);
            Color border = dark ? Color.FromArgb(85, 85, 85) : Color.FromArgb(190, 190, 190);
            ApplyControlColors(this, background, surface, text, border);
            resultsGrid.EnableHeadersVisualStyles = false;
            resultsGrid.BackgroundColor = surface;
            resultsGrid.GridColor = border;
            resultsGrid.DefaultCellStyle.BackColor = surface;
            resultsGrid.DefaultCellStyle.ForeColor = text;
            resultsGrid.DefaultCellStyle.SelectionBackColor = dark ? Color.FromArgb(40, 80, 120) : Color.FromArgb(210, 230, 250);
            resultsGrid.DefaultCellStyle.SelectionForeColor = text;
            resultsGrid.ColumnHeadersDefaultCellStyle.BackColor = background;
            resultsGrid.ColumnHeadersDefaultCellStyle.ForeColor = text;
            resultsGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = background;
            resultsGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = text;
            helpTip.BackColor = surface;
            helpTip.ForeColor = text;
        }

        private static void ApplyControlColors(Control control, Color background, Color surface, Color text, Color border)
        {
            control.BackColor = control is PictureBox || control is ComboBox
                || control is NumericUpDown || control is Button ? surface : background;
            control.ForeColor = text;
            var button = control as Button;
            if (button != null)
            {
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = border;
            }
            foreach (Control child in control.Controls)
                ApplyControlColors(child, background, surface, text, border);
        }

        private void ReleaseImages()
        {
            originalPicture.Image = resultPicture.Image = null;
            if (analysis != null) { analysis.Dispose(); analysis = null; }
            if (original != null) { original.Dispose(); original = null; }
        }
    }
}
