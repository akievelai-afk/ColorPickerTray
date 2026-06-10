using System;
using System.IO;
using System.Text.Json;

namespace ColorPickerTray
{
    public class SettingsManager
    {
        private readonly string _settingsPath;

        public AppSettings Current { get; private set; } = new();

        public SettingsManager(string dataDirectory)
        {
            Directory.CreateDirectory(dataDirectory);
            _settingsPath = Path.Combine(dataDirectory, "settings.json");
            Load();
        }

        private void Load()
        {
            if (!File.Exists(_settingsPath))
            {
                Current = new AppSettings();
                Save();
                return;
            }

            try
            {
                var json = File.ReadAllText(_settingsPath);
                Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                Current = new AppSettings();
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
    }
}
