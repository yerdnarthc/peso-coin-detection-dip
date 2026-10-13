using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CoinDenominationDIP;

internal static class AnalyzerChecks
{
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception("FAIL: " + description);
        Console.WriteLine("PASS: " + description);
    }

    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            using (var image = new Bitmap(args[0]))
            {
                int[] values = { 5, 10, 25, 100, 500 };
                int[] expected = { 7, 11, 28, 13, 5 };
                Color before = image.GetPixel(250, 50);
                using (var result = CoinAnalyzer.Analyze(image, CoinAnalyzer.DefaultThreshold))
                {
                    Check(result.Coins.Count == 64, "reference: 64 detected coins");
                    for (int i = 0; i < values.Length; i++)
                        Check(result.Coins.Count(c => c.ValueCentavos == values[i]) == expected[i],
                            "reference denomination " + values[i] + " centavos: " + expected[i]);
                    Check(result.Coins.All(c => c.ValueCentavos != 0), "reference: no unknowns");
                    Check(result.Coins.Sum(c => c.ValueCentavos) == 4645, "reference: total 4645 centavos");
                    Check(result.Coins.Count(c => c.HasHole) == 7, "reference: seven central holes");
                    result.AnnotatedImage.Save(Path.Combine(args[1], "analyzed.png"));
                    result.Mask.Save(Path.Combine(args[1], "mask.png"));
                }
                Check(image.GetPixel(250, 50) == before, "source image preserved");
                for (int run = 0; run < 3; run++)
                    using (var repeated = CoinAnalyzer.Analyze(image, 150))
                        Check(repeated.Coins.Count == 64, "repeat analysis " + (run + 1));
                foreach (double scale in new[] { 0.5, 2.0 })
                {
                    using (var resized = new Bitmap(image, (int)(image.Width * scale), (int)(image.Height * scale)))
                    using (var result = CoinAnalyzer.Analyze(resized, 150))
                    {
                        Check(result.Coins.Count == 64, "proportional resize " + scale + ": 64 coins");
                        for (int i = 0; i < values.Length; i++)
                            Check(result.Coins.Count(c => c.ValueCentavos == values[i]) == expected[i],
                                "resize " + scale + ": denomination " + values[i]);
                    }
                }
            }

            using (var blank = new Bitmap(776, 1024, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(blank)) g.Clear(Color.White);
                using (var result = CoinAnalyzer.Analyze(blank, 150))
                    Check(result.Coins.Count == 0, "blank white image");
                using (var g = Graphics.FromImage(blank))
                {
                    g.FillEllipse(Brushes.Black, 50, 50, 58, 60);
                    g.FillEllipse(Brushes.White, 72, 73, 14, 14);
                    g.FillEllipse(Brushes.Black, 150, 50, 64, 65);
                    g.FillEllipse(Brushes.Black, 250, 50, 75, 76);
                    g.FillEllipse(Brushes.Black, 350, 50, 90, 92);
                    g.FillEllipse(Brushes.Black, 460, 50, 101, 103);
                    g.FillRectangle(Brushes.Black, 50, 200, 130, 30);
                    g.FillRectangle(Brushes.Black, 10, 10, 2, 2);
                }
                using (var result = CoinAnalyzer.Analyze(blank, 150))
                {
                    Check(result.Coins.Count == 6, "five coins plus malformed candidate; speck ignored");
                    foreach (int value in new[] { 5, 10, 25, 100, 500, 0 })
                        Check(result.Coins.Count(c => c.ValueCentavos == value) == 1, "synthetic denomination " + value);
                }
                bool rejected = false;
                try { CoinAnalyzer.Analyze(blank, -1); }
                catch (ArgumentOutOfRangeException) { rejected = true; }
                Check(rejected, "invalid threshold rejected");
                using (var g = Graphics.FromImage(blank)) g.Clear(Color.FromArgb(150, 150, 150));
                using (var result = CoinAnalyzer.Analyze(blank, 150))
                    Check(result.Mask.GetPixel(0, 0).R == 0, "threshold equality is black");
                using (var result = CoinAnalyzer.Analyze(blank, 149))
                    Check(result.Mask.GetPixel(0, 0).R == 255, "above threshold is white");
            }
            bool nullRejected = false;
            try { CoinAnalyzer.Analyze(null, 150); }
            catch (ArgumentNullException) { nullRejected = true; }
            Check(nullRejected, "null image rejected");

            CheckUi(args[0], args[1]);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static T Field<T>(Form1 form, string name)
    {
        return (T)typeof(Form1).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
    }

    private static void SaveForm(Form1 form, string outputPath, string filename)
    {
        form.PerformLayout();
        Application.DoEvents();
        using (var rendered = new Bitmap(form.Width, form.Height))
        {
            form.DrawToBitmap(rendered, new Rectangle(0, 0, form.Width, form.Height));
            rendered.Save(Path.Combine(outputPath, filename));
        }
    }

    private static void CheckUi(string imagePath, string outputPath)
    {
        using (var form = new Form1())
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-20000, -20000);
            form.ShowInTaskbar = false;
            form.Show();
            Application.DoEvents();
            Check(!Field<CheckBox>(form, "darkModeCheckBox").Checked, "UI: light theme is the default");
            Check(!Field<Button>(form, "processButton").Enabled, "UI: cannot process without image");
            typeof(Form1).GetMethod("LoadSample", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(form, new object[] { form, EventArgs.Empty });
            Check(Field<Bitmap>(form, "original") != null, "UI: bundled sample loads");
            typeof(Form1).GetMethod("LoadImage", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(form, new object[] { Path.GetFullPath(imagePath) });
            Check(Field<Button>(form, "processButton").Enabled, "UI: loading enables processing");
            for (int run = 0; run < 2; run++)
            {
                typeof(Form1).GetMethod("ProcessImage", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(form, new object[] { form, EventArgs.Empty });
                Check(!Field<Button>(form, "openButton").Enabled, "UI: open disabled while processing");
                var deadline = DateTime.UtcNow.AddSeconds(30);
                while (Field<bool>(form, "busy") && DateTime.UtcNow < deadline)
                { Application.DoEvents(); Thread.Sleep(10); }
                Check(!Field<bool>(form, "busy"), "UI: async processing completes");
                Check(Field<Label>(form, "totalLabel").Text.Contains("46.45"), "UI: total displayed");
                Check(Field<DataGridView>(form, "resultsGrid").Rows.Count == 5, "UI: five result rows");
            }
            Field<CheckBox>(form, "maskCheckBox").Checked = true;
            Check(ReferenceEquals(Field<PictureBox>(form, "resultPicture").Image,
                Field<CoinAnalysis>(form, "analysis").Mask), "UI: binary mask toggle");
            Field<CheckBox>(form, "maskCheckBox").Checked = false;
            SaveForm(form, outputPath, "ui-light.png");
            Color lightBackground = form.BackColor;
            Field<CheckBox>(form, "darkModeCheckBox").Checked = true;
            Check(form.BackColor != lightBackground, "UI: dark mode changes background");
            Check(Field<DataGridView>(form, "resultsGrid").DefaultCellStyle.ForeColor.GetBrightness() > 0.8, "UI: dark grid uses light text");
            Check(Field<Label>(form, "totalLabel").Text.Contains("46.45"), "UI: theme preserves total");
            SaveForm(form, outputPath, "ui-dark.png");
            Field<CheckBox>(form, "darkModeCheckBox").Checked = false;
            Check(form.BackColor == lightBackground, "UI: light mode restored");
            form.Size = form.MinimumSize;
            SaveForm(form, outputPath, "ui-small.png");
            Field<NumericUpDown>(form, "thresholdInput").Value = 151;
            Check(Field<CoinAnalysis>(form, "analysis") == null, "UI: changing threshold clears stale results");
            Check(Field<PictureBox>(form, "resultPicture").Image == null, "UI: changing threshold clears preview");
        }
        Console.WriteLine("PASS: UI disposes images and closes after processing");
    }
}


