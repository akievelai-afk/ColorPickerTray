using System.Collections.Generic;

namespace ColorPickerTray
{
    public class AppSettings
    {
        public string HotKey { get; set; } = "Ctrl+F1";
        public string HistoryHotKey { get; set; } = "Ctrl+H";
        public string Theme { get; set; } = "LightBlue";
        public bool CopyOnlyCurrentFormat { get; set; } = true;
        public List<string> EnabledFormats { get; set; } = new() { "HSV" };
    }
}
