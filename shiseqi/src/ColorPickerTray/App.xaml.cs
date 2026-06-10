using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;
using WinForms = System.Windows.Forms;

namespace ColorPickerTray
{
    public partial class App : Application
    {
        private TrayIconManager? _trayIconManager;
        private HotKeyManager? _hotKeyManager;
        private HotKeyManager? _historyHotKeyManager;
        private SettingsManager? _settingsManager;
        private HistoryManager? _historyManager;

        public SettingsManager SettingsManager => _settingsManager!;
        public HistoryManager HistoryManager => _historyManager!;

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetProcessDPIAware();

            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData");
            _settingsManager = new SettingsManager(dataDirectory);
            _historyManager = new HistoryManager(dataDirectory);

            var mainWindow = new MainWindow(_settingsManager, _historyManager);
            MainWindow = mainWindow;
            MainWindow.Hide();

            _trayIconManager = new TrayIconManager(mainWindow);
            _trayIconManager.Initialize();

            _hotKeyManager = new HotKeyManager(mainWindow);
            _hotKeyManager.HotKeyPressed += OnHotKeyPressed;
            _historyHotKeyManager = new HotKeyManager(mainWindow);
            _historyHotKeyManager.HotKeyPressed += OnHistoryHotKeyPressed;
            RegisterHotKeyFromSettings();

            ApplyTheme(_settingsManager.Current.Theme);
        }

        private void RegisterHotKeyFromSettings()
        {
            if (_hotKeyManager == null || _settingsManager == null) return;

            _hotKeyManager.UnregisterHotKey();
            if (HotKeyParser.TryParse(_settingsManager.Current.HotKey, out var def))
            {
                if (!_hotKeyManager.RegisterHotKey(def.Modifiers, def.Key))
                    WinForms.MessageBox.Show("无法注册拾色热键，请尝试更换。", "ColorPickerTray", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
            }
            else
            {
                if (!_hotKeyManager.RegisterHotKey(ModifierKeys.Control, WinForms.Keys.F1))
                    WinForms.MessageBox.Show("无法注册默认热键 Ctrl+F1。", "ColorPickerTray", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
            }

            _historyHotKeyManager?.UnregisterHotKey();
            if (HotKeyParser.TryParse(_settingsManager.Current.HistoryHotKey, out var hDef))
            {
                _historyHotKeyManager?.RegisterHotKey(hDef.Modifiers, hDef.Key);
            }
            else
            {
                _historyHotKeyManager?.RegisterHotKey(ModifierKeys.Control, WinForms.Keys.H);
            }
        }

        private void OnHotKeyPressed(object? sender, EventArgs e)
        {
            if (MainWindow is MainWindow window)
            {
                window.ToggleColorPickerMode();
            }
        }

        private void OnHistoryHotKeyPressed(object? sender, EventArgs e)
        {
            if (MainWindow is MainWindow window)
            {
                window.OpenHistoryWindow();
            }
        }

        public void ReloadSettings()
        {
            _settingsManager?.Save();
            RegisterHotKeyFromSettings();
            ApplyTheme(_settingsManager?.Current.Theme ?? "LightBlue");
        }

        private void ApplyTheme(string theme)
        {
            Resources.Clear();
            switch (theme)
            {
                case "Dark":
                    Resources["BaseBackground"] = System.Windows.Media.Brushes.Black;
                    Resources["BaseForeground"] = System.Windows.Media.Brushes.White;
                    Resources["AccentBrush"] = System.Windows.Media.Brushes.DeepSkyBlue;
                    Resources["OverlayPanelBrush"] = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(0xDD, 0x2A, 0x2A, 0x40));
                    break;
                case "Light":
                    Resources["BaseBackground"] = System.Windows.Media.Brushes.WhiteSmoke;
                    Resources["BaseForeground"] = System.Windows.Media.Brushes.Black;
                    Resources["AccentBrush"] = System.Windows.Media.Brushes.SteelBlue;
                    Resources["OverlayPanelBrush"] = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(0xDD, 0x58, 0x95, 0x8A));
                    break;
                default:
                    Resources["BaseBackground"] = System.Windows.Media.Brushes.White;
                    Resources["BaseForeground"] = System.Windows.Media.Brushes.Black;
                    Resources["AccentBrush"] = System.Windows.Media.Brushes.LightSkyBlue;
                    Resources["OverlayPanelBrush"] = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(0xDD, 0x1E, 0x2F, 0x79));
                    break;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _hotKeyManager?.Dispose();
            _historyHotKeyManager?.Dispose();
            _trayIconManager?.Dispose();
        }
    }
}
