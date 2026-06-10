using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ColorPickerTray
{
    public static class ColorSampler
    {
        public static Color GetColorAt(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
            {
                return Color.Black;
            }

            try
            {
                int pixel = GetPixel(hdc, x, y);
                int r = pixel & 0xFF;
                int g = (pixel >> 8) & 0xFF;
                int b = (pixel >> 16) & 0xFF;
                return Color.FromArgb(r, g, b);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        public static Bitmap GetMagnifiedRegion(int centerX, int centerY, int size, int magnification)
        {
            int half = size / 2;
            int sourceX = Math.Max(0, centerX - half);
            int sourceY = Math.Max(0, centerY - half);
            var sourceSize = new System.Drawing.Size(size, size);

            using var sourceBmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(sourceBmp))
            {
                g.CopyFromScreen(sourceX, sourceY, 0, 0, sourceSize);
            }

            var magnified = new Bitmap(size * magnification, size * magnification, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(magnified))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(sourceBmp, new Rectangle(0, 0, magnified.Width, magnified.Height), new Rectangle(0, 0, sourceSize.Width, sourceSize.Height), GraphicsUnit.Pixel);
                using var pen = new Pen(Color.FromArgb(192, Color.White), 2);
                int line = (magnified.Width / 2) - 1;
                g.DrawLine(pen, line, 0, line, magnified.Height);
                g.DrawLine(pen, 0, line, magnified.Width, line);
            }

            return magnified;
        }

        public static BitmapSource ToBitmapSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            try
            {
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();
                return bitmapSource;
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);
    }
}
