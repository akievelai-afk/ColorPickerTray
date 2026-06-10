using System;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using System.Windows;
using System.Windows.Forms;

namespace ColorPickerTray
{
    public partial class MainWindow : Window
    {
        private bool _colorPickerMode;
        private PickerOverlay? _pickerOverlay;
        private readonly SettingsManager _settingsManager;
        private readonly HistoryManager _historyManager;
        private HistoryWindow? _historyWindow;
        private SettingsWindow? _settingsWindow;

        public MainWindow(SettingsManager settingsManager, HistoryManager historyManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            _historyManager = historyManager;
        }

        public void ToggleColorPickerMode()
        {
            _colorPickerMode = !_colorPickerMode;
            if (_colorPickerMode)
            {
                StartColorPicker();
            }
            else
            {
                StopColorPicker();
            }
        }

        private void StartColorPicker()
        {
            if (_pickerOverlay == null)
            {
                _pickerOverlay = new PickerOverlay(_settingsManager.Current, _historyManager);
                _pickerOverlay.CopyRequested += PickerOverlay_CopyRequested;
                _pickerOverlay.CancelRequested += PickerOverlay_CancelRequested;
                _pickerOverlay.Closed += (_, _) => _pickerOverlay = null;
            }

            _pickerOverlay.Show();
            _pickerOverlay.Activate();
        }

        private void StopColorPicker()
        {
            if (_pickerOverlay != null)
            {
                _pickerOverlay.Close();
                _pickerOverlay = null;
            }
        }

        private void PickerOverlay_CopyRequested(object? sender, EventArgs e)
        {
            if (_pickerOverlay == null)
            {
                return;
            }

            var color = _pickerOverlay.CurrentColor;
            var hsv = ColorUtilities.ToHsvString(color);
            var rgb = $"RGB: {color.R}, {color.G}, {color.B}";
            var hex = ColorUtilities.ToHex(color);
            var output = _settingsManager.Current.CopyOnlyCurrentFormat ? _pickerOverlay.GetCurrentColorValue() : $"{hsv} | {rgb} | {hex}";
            Clipboard.SetText(output);

            _historyManager.AddRecord(new ColorHistoryRecord
            {
                Timestamp = DateTime.Now,
                Hsv = hsv,
                Rgb = rgb,
                Hex = hex
            });
        }

        private void PickerOverlay_CancelRequested(object? sender, EventArgs e)
        {
            _colorPickerMode = false;
            StopColorPicker();
        }

        public void OpenHistoryWindow()
        {
            if (_historyWindow == null || !_historyWindow.IsVisible)
            {
                _historyWindow = new HistoryWindow(_historyManager);
                _historyWindow.Owner = this;
                _historyWindow.Show();
            }
            else
            {
                _historyWindow.Activate();
            }
        }

        public void OpenSettingsWindow()
        {
            if (_settingsWindow == null || !_settingsWindow.IsVisible)
            {
                _settingsWindow = new SettingsWindow(_settingsManager);
                _settingsWindow.Owner = this;
                _settingsWindow.SettingsSaved += SettingsWindow_SettingsSaved;
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        }

        private void SettingsWindow_SettingsSaved(object? sender, EventArgs e)
        {
            if (Application.Current is App app)
            {
                app.ReloadSettings();
            }

            if (_pickerOverlay != null)
            {
                _pickerOverlay.UpdateSettings(_settingsManager.Current);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
