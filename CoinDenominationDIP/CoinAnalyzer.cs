using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CoinDenominationDIP
{
    public sealed class DetectedCoin
    {
        public Rectangle Bounds { get; internal set; }
        public double Diameter { get; internal set; }
        public bool HasHole { get; internal set; }
        // Zero means that this candidate could not be classified reliably.
        public int ValueCentavos { get; internal set; }
    }

    public sealed class CoinAnalysis : IDisposable
    {
        public Bitmap Mask { get; internal set; }
        public Bitmap AnnotatedImage { get; internal set; }
        public List<DetectedCoin> Coins { get; private set; }

        public CoinAnalysis() { Coins = new List<DetectedCoin>(); }

        public void Dispose()
        {
            if (Mask != null) Mask.Dispose();
            if (AnnotatedImage != null) AnnotatedImage.Dispose();
        }
    }

    public static class CoinAnalyzer
    {
        public const int DefaultThreshold = 150;
        // Manual calibration is expressed in pixels of the supplied 776px-wide image.
        private const double ReferenceWidth = 776.0;

        public static CoinAnalysis Analyze(Bitmap source, int threshold)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (threshold < 0 || threshold > 255)
                throw new ArgumentOutOfRangeException("threshold");
            if ((long)source.Width * source.Height > 12000000)
                throw new ArgumentException("Please use an image of at most 12 million pixels.");

            var result = new CoinAnalysis();
            try
            {
                // The supplied threshold routine allocates the destination; keep source intact.
                Bitmap input = source;
                Bitmap mask = null;
                HNUDIP.ImageProcess.Threshold(ref input, ref mask, threshold);
                result.Mask = mask;
                int width = mask.Width, height = mask.Height;
                bool[] dark = ReadDarkPixels(mask);
                int[] labels = new int[dark.Length];
                int[] queue = new int[dark.Length];
                double scale = width / ReferenceWidth;
                int label = 0;

                for (int start = 0; start < dark.Length; start++)
                {
                    if (!dark[start] || labels[start] != 0) continue;
                    label++;
                    int head = 0, tail = 1;
                    queue[0] = start;
                    labels[start] = label;
                    int minX = start % width, maxX = minX;
                    int minY = start / width, maxY = minY;
                    while (head < tail)
                    {
                        int pixel = queue[head++], x = pixel % width, y = pixel / width;
                        minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                                int next = ny * width + nx;
                                if (dark[next] && labels[next] == 0)
                                {
                                    labels[next] = label;
                                    queue[tail++] = next;
                                }
                            }
                    }

                    // Ignore small specks, but retain substantial malformed objects as Unknown.
                    if (tail < 350 * scale * scale) continue;
                    var bounds = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
                    double diameter = (bounds.Width + bounds.Height) / (2.0 * scale);
                    double aspect = bounds.Width / (double)bounds.Height;
                    double fill = tail / (double)(bounds.Width * bounds.Height);
                    bool hole = HasCentralHole(labels, width, bounds, label);
                    bool validShape = aspect >= 0.8 && aspect <= 1.2 && fill >= 0.55
                        && minX > 0 && minY > 0 && maxX < width - 1 && maxY < height - 1;
                    result.Coins.Add(new DetectedCoin
                    {
                        Bounds = bounds,
                        Diameter = diameter,
                        HasHole = hole,
                        ValueCentavos = validShape ? Classify(diameter, hole) : 0
                    });
                }

                result.Coins.Sort((a, b) => a.Bounds.Top.CompareTo(b.Bounds.Top));
                result.AnnotatedImage = CreateOverlay(source, result.Coins);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        public static Bitmap CreateOverlay(Bitmap source, IEnumerable<DetectedCoin> coins)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (coins == null) throw new ArgumentNullException("coins");
            var overlay = new Bitmap(source);
            double scale = source.Width / ReferenceWidth;
            try
            {
                using (Graphics graphics = Graphics.FromImage(overlay))
                using (var font = new Font(FontFamily.GenericSansSerif, Math.Max(8, (float)(11 * scale)), FontStyle.Bold))
                using (var pen = new Pen(Color.LimeGreen, Math.Max(1, (float)(2 * scale))))
                {
                    foreach (DetectedCoin coin in coins)
                    {
                        pen.Color = coin.ValueCentavos == 0 ? Color.OrangeRed : Color.LimeGreen;
                        graphics.DrawRectangle(pen, coin.Bounds);
                        string caption = coin.ValueCentavos == 0 ? "Unknown"
                            : coin.ValueCentavos < 100 ? coin.ValueCentavos + "c"
                            : "P" + coin.ValueCentavos / 100;
                        SizeF textSize = graphics.MeasureString(caption, font);
                        float y = Math.Max(0, coin.Bounds.Top - textSize.Height);
                        graphics.FillRectangle(Brushes.Black, coin.Bounds.Left, y, textSize.Width, textSize.Height);
                        graphics.DrawString(caption, font, Brushes.White, coin.Bounds.Left, y);
                    }
                }
                return overlay;
            }
            catch { overlay.Dispose(); throw; }
        }

        private static int Classify(double diameter, bool hole)
        {
            if (hole) return diameter >= 50 && diameter < 64 ? 5 : 0;
            if (diameter < 60 || diameter > 112) return 0;
            if (diameter < 70) return 10;
            if (diameter < 83.25) return 25;
            if (diameter < 96.5) return 100;
            return 500;
        }

        private static bool[] ReadDarkPixels(Bitmap bitmap)
        {
            var dark = new bool[bitmap.Width * bitmap.Height];
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                var row = new byte[bitmap.Width * 3];
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, row.Length);
                    for (int x = 0; x < bitmap.Width; x++)
                        dark[y * bitmap.Width + x] = row[x * 3] == 0;
                }
            }
            finally { bitmap.UnlockBits(data); }
            return dark;
        }

        private static bool HasCentralHole(int[] labels, int imageWidth, Rectangle bounds, int label)
        {
            int width = bounds.Width, height = bounds.Height;
            var visited = new bool[width * height];
            var queue = new int[visited.Length];
            for (int start = 0; start < visited.Length; start++)
            {
                int sx = start % width, sy = start / width;
                if (visited[start] || labels[(bounds.Y + sy) * imageWidth + bounds.X + sx] == label) continue;
                int head = 0, tail = 1;
                long sumX = 0, sumY = 0;
                bool touchesEdge = false;
                visited[start] = true; queue[0] = start;
                while (head < tail)
                {
                    int pixel = queue[head++], x = pixel % width, y = pixel / width;
                    sumX += x; sumY += y;
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1) touchesEdge = true;
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int nx = x + (direction == 0 ? -1 : direction == 1 ? 1 : 0);
                        int ny = y + (direction == 2 ? -1 : direction == 3 ? 1 : 0);
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                        int next = ny * width + nx;
                        if (!visited[next] && labels[(bounds.Y + ny) * imageWidth + bounds.X + nx] != label)
                        { visited[next] = true; queue[tail++] = next; }
                    }
                }
                double areaFraction = tail / (double)visited.Length;
                double cx = sumX / (double)tail - (width - 1) / 2.0;
                double cy = sumY / (double)tail - (height - 1) / 2.0;
                if (!touchesEdge && areaFraction >= 0.008 && areaFraction <= 0.15
                    && Math.Abs(cx) < width * 0.2 && Math.Abs(cy) < height * 0.2)
                    return true;
            }
            return false;
        }
    }
}
