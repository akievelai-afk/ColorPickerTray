using System;
using System.Linq;
using MessageBox = System.Windows.MessageBox;
using System.Windows;

namespace ColorPickerTray
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsManager _settingsManager;

        public event EventHandler? SettingsSaved;

        public SettingsWindow(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            LoadSettings();
        }

        private void LoadSettings()
        {
            HotKeyText.Text = _settingsManager.Current.HotKey;
            HistoryHotKeyText.Text = _settingsManager.Current.HistoryHotKey;
            HsvCheck.IsChecked = _settingsManager.Current.EnabledFormats.Contains("HSV");
            RgbCheck.IsChecked = _settingsManager.Current.EnabledFormats.Contains("RGB");
            HexCheck.IsChecked = _settingsManager.Current.EnabledFormats.Contains("HEX");
            CopyCurrentOnlyCheck.IsChecked = _settingsManager.Current.CopyOnlyCurrentFormat;

            ThemeLightBlue.IsChecked = _settingsManager.Current.Theme == "LightBlue";
            ThemeDark.IsChecked = _settingsManager.Current.Theme == "Dark";
            ThemeLight.IsChecked = _settingsManager.Current.Theme == "Light";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var enabledFormats = new System.Collections.Generic.List<string>();
            if (HsvCheck.IsChecked == true) enabledFormats.Add("HSV");
            if (RgbCheck.IsChecked == true) enabledFormats.Add("RGB");
            if (HexCheck.IsChecked == true) enabledFormats.Add("HEX");
            if (!enabledFormats.Any())
            {
                MessageBox.Show(this, "请至少选择一种颜色格式。", "设置", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rawHotKey = string.IsNullOrWhiteSpace(HotKeyText.Text) ? "F1" : HotKeyText.Text.Trim();
            if (!HotKeyParser.TryParse(rawHotKey, out _))
            {
                MessageBox.Show(this, "拾色热键格式无效。", "设置", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rawHistoryHotKey = string.IsNullOrWhiteSpace(HistoryHotKeyText.Text) ? "H" : HistoryHotKeyText.Text.Trim();
            if (!HotKeyParser.TryParse(rawHistoryHotKey, out _))
            {
                MessageBox.Show(this, "历史热键格式无效。", "设置", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _settingsManager.Current.HotKey = rawHotKey.ToUpper();
            _settingsManager.Current.HistoryHotKey = rawHistoryHotKey.ToUpper();
            _settingsManager.Current.EnabledFormats = enabledFormats;
            _settingsManager.Current.CopyOnlyCurrentFormat = CopyCurrentOnlyCheck.IsChecked == true;

            if (ThemeLightBlue.IsChecked == true)
            {
                _settingsManager.Current.Theme = "LightBlue";
            }
            else if (ThemeDark.IsChecked == true)
            {
                _settingsManager.Current.Theme = "Dark";
            }
            else
            {
                _settingsManager.Current.Theme = "Light";
            }

            _settingsManager.Save();
            SettingsSaved?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
