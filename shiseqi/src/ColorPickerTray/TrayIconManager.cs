using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Application = System.Windows.Application;
using System.Windows.Forms;

namespace ColorPickerTray
{
    public class TrayIconManager : IDisposable
    {
        private readonly MainWindow _mainWindow;
        private readonly NotifyIcon _notifyIcon;
        private Icon? _trayIcon;

        public TrayIconManager(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "ColorPickerTray";
        }

        public void Initialize()
        {
            _trayIcon = CreateTrayIcon();
            _notifyIcon.Icon = _trayIcon;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenuStrip = BuildContextMenu();
            _notifyIcon.DoubleClick += OnIconDoubleClick;
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            var pickItem = new ToolStripMenuItem("启动拾色");
            pickItem.Click += (_, _) => _mainWindow.ToggleColorPickerMode();
            var historyItem = new ToolStripMenuItem("打开历史");
            historyItem.Click += (_, _) => _mainWindow.OpenHistoryWindow();
            var themeItem = new ToolStripMenuItem("设置主题");
            themeItem.Click += (_, _) => _mainWindow.OpenSettingsWindow();
            var exitItem = new ToolStripMenuItem("退出");
            exitItem.Click += (_, _) => Application.Current.Shutdown();

            menu.Items.Add(pickItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(historyItem);
            menu.Items.Add(themeItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);
            return menu;
        }

        private static Icon CreateTrayIcon()
        {
            const int iconSize = 48;
            using var bmp = new Bitmap(iconSize, iconSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int squareSize = 14;
                int spacing = 4;
                int totalWidth = squareSize * 3 + spacing * 2;
                int startX = (iconSize - totalWidth) / 2;
                int startY = (iconSize - squareSize) / 2;

                using var redBrush = new SolidBrush(Color.FromArgb(255, 229, 56, 70));
                using var yellowBrush = new SolidBrush(Color.FromArgb(255, 245, 186, 50));
                using var blueBrush = new SolidBrush(Color.FromArgb(255, 60, 120, 220));
                using var shadowBrush = new SolidBrush(Color.FromArgb(48, 0, 0, 0));
                using var outlinePen = new Pen(Color.FromArgb(200, 255, 255, 255), 1f);

                g.FillRectangle(shadowBrush, startX + 1, startY + 2, squareSize, squareSize);
                g.FillRectangle(shadowBrush, startX + squareSize + spacing + 1, startY + 2, squareSize, squareSize);
                g.FillRectangle(shadowBrush, startX + (squareSize + spacing) * 2 + 1, startY + 2, squareSize, squareSize);

                g.FillRectangle(redBrush, startX, startY, squareSize, squareSize);
                g.FillRectangle(yellowBrush, startX + squareSize + spacing, startY, squareSize, squareSize);
                g.FillRectangle(blueBrush, startX + (squareSize + spacing) * 2, startY, squareSize, squareSize);

                g.DrawRectangle(outlinePen, startX, startY, squareSize, squareSize);
                g.DrawRectangle(outlinePen, startX + squareSize + spacing, startY, squareSize, squareSize);
                g.DrawRectangle(outlinePen, startX + (squareSize + spacing) * 2, startY, squareSize, squareSize);
            }

            IntPtr hIcon = bmp.GetHicon();
            return Icon.FromHandle(hIcon);
        }


        private void OnIconDoubleClick(object? sender, EventArgs e)
        {
            _mainWindow.ToggleColorPickerMode();
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _trayIcon?.Dispose();
        }
    }
}
