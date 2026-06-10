using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ColorPickerTray
{
    public class HistoryManager
    {
        private const int MaxRecords = 50;
        private readonly string _historyPath;

        public List<ColorHistoryRecord> Records { get; private set; } = new();
        public event EventHandler? RecordAdded;

        public HistoryManager(string dataDirectory)
        {
            Directory.CreateDirectory(dataDirectory);
            _historyPath = Path.Combine(dataDirectory, "history.json");
            Load();
        }

        private void Load()
        {
            if (!File.Exists(_historyPath))
            {
                Records = new List<ColorHistoryRecord>();
                return;
            }

            try
            {
                var json = File.ReadAllText(_historyPath, System.Text.Encoding.UTF8);
                Records = JsonSerializer.Deserialize<List<ColorHistoryRecord>>(json) ?? new List<ColorHistoryRecord>();
            }
            catch
            {
                Records = new List<ColorHistoryRecord>();
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(Records, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_historyPath, json, System.Text.Encoding.UTF8);
        }

        public void AddRecord(ColorHistoryRecord record)
        {
            Records.Insert(0, record);
            if (Records.Count > MaxRecords)
            {
                Records = Records.Take(MaxRecords).ToList();
            }
            Reindex();
            Save();
            RecordAdded?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveLast()
        {
            if (Records.Count > 0)
            {
                Records.RemoveAt(0);
                Reindex();
                Save();
                RecordAdded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Clear()
        {
            Records.Clear();
            Save();
        }

        private void Reindex()
        {
            for (var i = 0; i < Records.Count; i++)
            {
                Records[i].Index = i + 1;
            }
        }

        public bool ExportCsv(string filePath)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Index,Timestamp,HSV,RGB,HEX");
                foreach (var record in Records)
                {
                    sb.AppendLine($"{record.Index},\"{record.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{record.Hsv}\",\"{record.Rgb}\",\"{record.Hex}\"");
                }

                File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
