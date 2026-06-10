using System;

namespace ColorPickerTray
{
    public class ColorHistoryRecord
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string Hsv { get; set; } = string.Empty;
        public string Rgb { get; set; } = string.Empty;
        public string Hex { get; set; } = string.Empty;
    }
}
