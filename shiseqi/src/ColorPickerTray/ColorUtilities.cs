using System;
using System.Drawing;

namespace ColorPickerTray
{
    public static class ColorUtilities
    {
        public static string ToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static (double H, double S, double V) ToHsv(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = new[] { r, g, b }.Max();
            double min = new[] { r, g, b }.Min();
            double delta = max - min;

            double h = 0;
            if (delta > 0)
            {
                if (max == r)
                {
                    h = 60 * (((g - b) / delta) % 6);
                }
                else if (max == g)
                {
                    h = 60 * (((b - r) / delta) + 2);
                }
                else
                {
                    h = 60 * (((r - g) / delta) + 4);
                }
            }

            if (h < 0)
            {
                h += 360;
            }

            double s = max == 0 ? 0 : delta / max;
            double v = max;
            return (h, s, v);
        }

        public static string ToHsvString(Color color)
        {
            var (h, s, v) = ToHsv(color);
            return $"HSV: {h:F0}°, {(s * 100):F0}%, {(v * 100):F0}%";
        }

        public static Bitmap GenerateHueRing(int size, int ringThickness = 28, int renderScale = 1)
        {
            int rawSize = size * renderScale;
            var rawBmp = new Bitmap(rawSize, rawSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int cx = rawSize / 2;
            int cy = rawSize / 2;
            int outerR = rawSize / 2 - 1;
            int innerR = Math.Max(0, outerR - ringThickness * renderScale);

            for (int y = 0; y < rawSize; y++)
            {
                for (int x = 0; x < rawSize; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist >= innerR && dist <= outerR)
                    {
                        double angle = Math.Atan2(dy, dx);
                        double hue = (angle * 180 / Math.PI + 360 + 150) % 360;
                        var c = HsvToRgb(hue, 1.0, 1.0);
                        rawBmp.SetPixel(x, y, c);
                    }
                    else
                    {
                        rawBmp.SetPixel(x, y, Color.Transparent);
                    }
                }
            }

            if (renderScale == 1)
            {
                return rawBmp;
            }

            var result = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = System.Drawing.Graphics.FromImage(result))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawImage(rawBmp, 0, 0, size, size);
            }

            rawBmp.Dispose();
            return result;
        }

        public static Bitmap GenerateSvSquare(int size, double hue)
        {
            var bmp = new Bitmap(size, size);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double s = x / (double)(size - 1);
                    double v = 1.0 - y / (double)(size - 1);
                    bmp.SetPixel(x, y, HsvToRgb(hue, s, v));
                }
            }
            return bmp;
        }

        public static Bitmap GenerateColorWheel(int size)
        {
            var wheel = new Bitmap(size, size);
            int centerX = size / 2;
            int centerY = size / 2;
            int radius = size / 2 - 1;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= radius)
                    {
                        // 参考绘画软件色轮：圆周是色相，内向外是饱和度
                        double angle = Math.Atan2(dy, dx);
                        double hue = (angle * 180 / Math.PI + 360) % 360;
                        double saturation = Math.Min(distance / radius, 1.0);
                        double value = 1.0;

                        wheel.SetPixel(x, y, HsvToRgb(hue, saturation, value));
                    }
                    else
                    {
                        wheel.SetPixel(x, y, Color.Transparent);
                    }
                }
            }

            return wheel;
        }

        public static Color HsvToRgb(double hue, double saturation, double value)
        {
            double h = hue / 60.0;
            int i = (int)Math.Floor(h);
            double f = h - i;
            double p = value * (1 - saturation);
            double q = value * (1 - f * saturation);
            double t = value * (1 - (1 - f) * saturation);

            double r, g, b;
            switch (i % 6)
            {
                case 0: r = value; g = t; b = p; break;
                case 1: r = q; g = value; b = p; break;
                case 2: r = p; g = value; b = t; break;
                case 3: r = p; g = q; b = value; break;
                case 4: r = t; g = p; b = value; break;
                default: r = value; g = p; b = q; break;
            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public static Bitmap GenerateHueBar(int width, int height)
        {
            var bmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                double hue = x / (double)(width - 1) * 360.0;
                var c = HsvToRgb(hue, 1.0, 1.0);
                for (int y = 0; y < height; y++) bmp.SetPixel(x, y, c);
            }
            return bmp;
        }

        public static Bitmap GenerateSatBar(int width, int height, double hue, double value)
        {
            var bmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                double s = x / (double)(width - 1);
                var c = HsvToRgb(hue, s, value);
                for (int y = 0; y < height; y++) bmp.SetPixel(x, y, c);
            }
            return bmp;
        }

        public static Bitmap GenerateValBar(int width, int height, double hue, double saturation)
        {
            var bmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                double v = x / (double)(width - 1);
                var c = HsvToRgb(hue, saturation, v);
                for (int y = 0; y < height; y++) bmp.SetPixel(x, y, c);
            }
            return bmp;
        }
    }
}
