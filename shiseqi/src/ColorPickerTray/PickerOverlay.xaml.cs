using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Color = System.Drawing.Color;

namespace ColorPickerTray
{
    public partial class PickerOverlay : Window
    {
        private bool _isLocked;
        private bool _isUpdating;
        private System.Drawing.Point _lastCursorPosition;
        private Color _currentColor;
        private AppSettings _settings;
        private HistoryManager? _historyManager;
        private Border[]? _miniSwatches;
        private Bitmap? _baseHueRing;
        private Bitmap? _svBitmap;
        private Bitmap? _hueBarBmp;
        private Bitmap? _satBarBmp;
        private Bitmap? _valBarBmp;
        private System.Windows.Shapes.Ellipse? _huePointerEllipse;
        private System.Windows.Shapes.Ellipse? _svPointerEllipse;
        private double _lastHue = -1;
        private double _lastSatForBars = -1;
        private double _lastValForBars = -1;
        private int _ringSize = 152;
        private int _svSize = 72; // computed on load

        public Color CurrentColor => _currentColor;

        public event EventHandler? CopyRequested;
        public event EventHandler? CancelRequested;

        public PickerOverlay(AppSettings settings, HistoryManager? historyManager = null)
        {
            _settings = settings;
            _historyManager = historyManager;
            InitializeComponent();
            Loaded += PickerOverlay_Loaded;
            Closed += PickerOverlay_Closed;
        }

        public void UpdateSettings(AppSettings settings)
        {
            _settings = settings;
            LoadFormatOptions();
            ApplyOverlayTheme();
        }

        private void ApplyOverlayTheme()
        {
            var res = System.Windows.Application.Current.Resources;
            if (res["OverlayPanelBrush"] is System.Windows.Media.Brush brush)
                OverlayBorder.Background = brush;
        }

        private void PickerOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            Left = screenBounds.Left;
            Top = screenBounds.Top;
            Width = screenBounds.Width;
            Height = screenBounds.Height;

            CompositionTarget.Rendering += OnRendering;
            LoadFormatOptions();
            Focus();

            // compute sizes based on ring size and desired thickness
            int ringThickness = 18;
            int outerR = _ringSize / 2 - 1;
            int innerR = Math.Max(4, outerR - ringThickness - 2);
            // fit a square inside the inner circle with some padding
            _svSize = (int)(innerR * Math.Sqrt(2) * 0.9);

            // Cache the hue ring once for performance
            _baseHueRing = ColorUtilities.GenerateHueRing(_ringSize, ringThickness, 2);
            HueRingImage.Source = ColorSampler.ToBitmapSource(_baseHueRing);

            // initial empty sv/hue bars (will be updated on first tick)
            _hueBarBmp = ColorUtilities.GenerateHueBar(154, 14);
            HueBar.Source = ColorSampler.ToBitmapSource(_hueBarBmp);

            _svBitmap = null;
            _lastHue = -1;
            _lastSatForBars = -1;
            _lastValForBars = -1;

            WheelPointerOverlay.Children.Clear();
            _huePointerEllipse = CreatePointerEllipse(12);
            _svPointerEllipse = CreatePointerEllipse(10);
            WheelPointerOverlay.Children.Add(_huePointerEllipse);
            WheelPointerOverlay.Children.Add(_svPointerEllipse);

            // Build mini swatch panel (10 swatches: 2 rows x 5)
            _miniSwatches = new Border[10];
            for (int i = 0; i < 10; i++)
            {
                var swatch = new Border
                {
                    Width = 22, Height = 22,
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(2),
                    BorderBrush = System.Windows.Media.Brushes.White,
                    BorderThickness = new Thickness(0.5),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = i
                };
                swatch.MouseLeftButtonDown += MiniSwatch_Click;
                MiniSwatchPanel.Children.Add(swatch);
                _miniSwatches[i] = swatch;
            }
            UpdateMiniSwatches();
            ApplyOverlayTheme();

            if (_historyManager != null)
                _historyManager.RecordAdded += OnHistoryRecordAdded;
        }

        private void LoadFormatOptions()
        {
            FormatChoice.ItemsSource = _settings.EnabledFormats;
            if (_settings.EnabledFormats.Count > 0)
            {
                FormatChoice.SelectedIndex = 0;
            }
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (_isUpdating)
            {
                return;
            }

            _isUpdating = true;
            try
            {
                UpdatePickerState();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdatePickerState();
        }

        private void PickerOverlay_Closed(object? sender, EventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
            if (_historyManager != null)
                _historyManager.RecordAdded -= OnHistoryRecordAdded;
        }

        private void OnHistoryRecordAdded(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(UpdateMiniSwatches);
        }

        private void UpdateMiniSwatches()
        {
            if (_miniSwatches == null || _historyManager == null) return;
            var records = _historyManager.Records;
            for (int i = 0; i < 10; i++)
            {
                if (i < records.Count)
                {
                    _miniSwatches[i].Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(records[i].Hex));
                    _miniSwatches[i].ToolTip = records[i].Hex;
                }
                else
                {
                    _miniSwatches[i].Background = System.Windows.Media.Brushes.Transparent;
                    _miniSwatches[i].ToolTip = null;
                }
            }
        }

        private void MiniSwatch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border swatch && swatch.Tag is int idx && _miniSwatches != null && _historyManager != null)
            {
                var records = _historyManager.Records;
                if (idx < records.Count)
                {
                    System.Windows.Clipboard.SetText(records[idx].Hex);
                }
            }
        }

        private void UpdatePickerState()
        {
            var cursorPosition = System.Windows.Forms.Cursor.Position;
            if (cursorPosition == _lastCursorPosition)
            {
                return;
            }

            _lastCursorPosition = cursorPosition;
            _currentColor = ColorSampler.GetColorAt(cursorPosition.X, cursorPosition.Y);
            if (!_isLocked)
                UpdateOverlayPosition(cursorPosition.X, cursorPosition.Y);
            RefreshColorDisplay(cursorPosition.X, cursorPosition.Y);
        }

        private void UpdateOverlayPosition(int cursorX, int cursorY)
        {
            var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            double targetLeft;
            if (cursorX > screenBounds.Right - OverlayBorder.Width - 40)
            {
                targetLeft = cursorX - OverlayBorder.Width - 40;
            }
            else
            {
                targetLeft = cursorX + 20;
            }

            double targetTop = cursorY + 20;
            if (targetTop + OverlayBorder.Height > screenBounds.Bottom)
            {
                targetTop = cursorY - OverlayBorder.Height - 20;
            }

            Canvas.SetLeft(OverlayBorder, Math.Max(screenBounds.Left + 10, targetLeft));
            Canvas.SetTop(OverlayBorder, Math.Max(screenBounds.Top + 10, targetTop));
        }

        private void RefreshColorDisplay(int x, int y)
        {
            using var magnified = ColorSampler.GetMagnifiedRegion(x, y, 11, 8);
            MagnifierImage.Source = ColorSampler.ToBitmapSource(magnified);
            ColorPreview.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(_currentColor.R, _currentColor.G, _currentColor.B));
            var (h, s, v) = ColorUtilities.ToHsv(_currentColor);

            // Update hue ring / SV square / pointers
            if (_baseHueRing != null)
            {
                // ensure sv bitmap matches current hue (regenerate only when hue changes)
                if (_svBitmap == null || Math.Abs(h - _lastHue) > 0.6)
                {
                    _svBitmap?.Dispose();
                    _svBitmap = ColorUtilities.GenerateSvSquare(_svSize, h);
                    _lastHue = h;

                    // update sat/val bars since hue changed
                    _satBarBmp?.Dispose();
                    _valBarBmp?.Dispose();
                    _satBarBmp = ColorUtilities.GenerateSatBar(154, 12, h, v);
                    _valBarBmp = ColorUtilities.GenerateValBar(154, 12, h, s);
                    SatBar.Source = ColorSampler.ToBitmapSource(_satBarBmp);
                    ValBar.Source = ColorSampler.ToBitmapSource(_valBarBmp);
                    _lastSatForBars = s;
                    _lastValForBars = v;
                }
                else
                {
                    // update sat/val if their values changed noticeably
                    if (Math.Abs(s - _lastSatForBars) > 0.01)
                    {
                        _satBarBmp?.Dispose();
                        _satBarBmp = ColorUtilities.GenerateSatBar(154, 12, h, v);
                        SatBar.Source = ColorSampler.ToBitmapSource(_satBarBmp);
                        _lastSatForBars = s;
                    }
                    if (Math.Abs(v - _lastValForBars) > 0.01)
                    {
                        _valBarBmp?.Dispose();
                        _valBarBmp = ColorUtilities.GenerateValBar(154, 12, h, s);
                        ValBar.Source = ColorSampler.ToBitmapSource(_valBarBmp);
                        _lastValForBars = v;
                    }
                }

                HueValueText.Text = $"{h:F0}°";
                SatValueText.Text = $"{(s * 100):F0}%";
                ValValueText.Text = $"{(v * 100):F0}%";
                UpdateBarOverlay(HueBarOverlay, h / 360.0);
                UpdateBarOverlay(SatBarOverlay, s);
                UpdateBarOverlay(ValBarOverlay, v);

                SvSquareImage.Source = _svBitmap != null ? ColorSampler.ToBitmapSource(_svBitmap) : null;
                SvSquareImage.Width = _svSize;
                SvSquareImage.Height = _svSize;
                int svLeft = (_ringSize - _svSize) / 2;
                int svTop = (_ringSize - _svSize) / 2;
                Canvas.SetLeft(SvSquareImage, svLeft);
                Canvas.SetTop(SvSquareImage, svTop);

                double angleRad = (h - 150) * Math.PI / 180.0;
                double outerR = _ringSize / 2.0 - 1;
                double innerR = outerR - 18;
                double ringR = (outerR + innerR) / 2.0;
                double cx = _ringSize / 2.0;
                double cy = _ringSize / 2.0;
                double hx = cx + Math.Cos(angleRad) * ringR;
                double hy = cy + Math.Sin(angleRad) * ringR;
                if (_huePointerEllipse != null)
                {
                    Canvas.SetLeft(_huePointerEllipse, hx - _huePointerEllipse.Width / 2);
                    Canvas.SetTop(_huePointerEllipse, hy - _huePointerEllipse.Height / 2);
                }

                int svX = svLeft + (int)Math.Round(s * (_svSize - 1));
                int svY = svTop + (int)Math.Round((1.0 - v) * (_svSize - 1));
                if (_svPointerEllipse != null)
                {
                    Canvas.SetLeft(_svPointerEllipse, svX - _svPointerEllipse.Width / 2);
                    Canvas.SetTop(_svPointerEllipse, svY - _svPointerEllipse.Height / 2);
                }
            }

            RgbText.Text = _settings.EnabledFormats.Contains("RGB") ? $"RGB: {_currentColor.R}, {_currentColor.G}, {_currentColor.B}" : string.Empty;
            HexText.Text = _settings.EnabledFormats.Contains("HEX") ? ColorUtilities.ToHex(_currentColor) : string.Empty;

            RgbText.Visibility = string.IsNullOrEmpty(RgbText.Text) ? Visibility.Collapsed : Visibility.Visible;
            HexText.Visibility = string.IsNullOrEmpty(HexText.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Alt 切换锁定/跟随模式
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt || (e.Key == Key.System && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)))
            {
                _isLocked = !_isLocked;
                OverlayBorder.BorderBrush = _isLocked
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0xFF, 0xFF))
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
                LockStatusText.Text = _isLocked ? "按 Alt 恢复跟随（已锁定）" : "按 Alt 锁定窗口（未锁定）";
                LockStatusText.Foreground = _isLocked
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0xFF, 0xCC))
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0xCC, 0xAA));
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _historyManager?.RemoveLast();
            }
            else if (e.Key == Key.C)
            {
                CopyRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (e.Key == Key.Escape)
            {
                CancelRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CopyRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }

        private void Window_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }

        private void UpdateBarOverlay(Canvas overlay, double normalizedPosition)
        {
            overlay.Children.Clear();
            if (normalizedPosition < 0 || normalizedPosition > 1)
            {
                normalizedPosition = Math.Max(0, Math.Min(1, normalizedPosition));
            }

            double width = overlay.ActualWidth;
            double height = overlay.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                width = 220;
                height = 14;
            }

            double x = Math.Max(4, Math.Min(width - 4, normalizedPosition * width));
            var outer = new System.Windows.Shapes.Ellipse
            {
                Width = 10,
                Height = 10,
                Stroke = System.Windows.Media.Brushes.White,
                StrokeThickness = 2,
                Fill = System.Windows.Media.Brushes.Black,
                Opacity = 0.95
            };
            Canvas.SetLeft(outer, x - 4);
            Canvas.SetTop(outer, (height - 8) / 2);
            overlay.Children.Add(outer);

            var inner = new System.Windows.Shapes.Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = System.Windows.Media.Brushes.White
            };
            Canvas.SetLeft(inner, x - 2);
            Canvas.SetTop(inner, (height - 4) / 2);
            overlay.Children.Add(inner);
        }

        private System.Windows.Shapes.Ellipse CreatePointerEllipse(double size)
        {
            return new System.Windows.Shapes.Ellipse
            {
                Width = size,
                Height = size,
                Stroke = System.Windows.Media.Brushes.White,
                StrokeThickness = 2,
                Fill = System.Windows.Media.Brushes.Black,
                Opacity = 0.95
            };
        }

        public string GetCurrentColorValue()
        {
            if (FormatChoice.SelectedItem is string selectedFormat)
            {
                return selectedFormat switch
                {
                    "HSV" => ColorUtilities.ToHsvString(_currentColor),
                    "RGB" => $"RGB: {_currentColor.R}, {_currentColor.G}, {_currentColor.B}",
                    "HEX" => ColorUtilities.ToHex(_currentColor),
                    _ => ColorUtilities.ToHsvString(_currentColor),
                };
            }

            return ColorUtilities.ToHsvString(_currentColor);
        }
    }
}
