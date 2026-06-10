using System.Windows;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace ColorPickerTray
{
    public partial class HistoryWindow : Window
    {
        private readonly HistoryManager _historyManager;

        public HistoryWindow(HistoryManager historyManager)
        {
            InitializeComponent();
            _historyManager = historyManager;
            _historyManager.RecordAdded += OnRecordAdded;
            Closed += (_, _) => _historyManager.RecordAdded -= OnRecordAdded;
            HistoryGrid.ItemsSource = _historyManager.Records;
        }

        private void OnRecordAdded(object? sender, System.EventArgs e)
        {
            HistoryGrid.ItemsSource = null;
            HistoryGrid.ItemsSource = _historyManager.Records;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV 文件 (*.csv)|*.csv",
                FileName = "color_history.csv"
            };
            if (dialog.ShowDialog(this) == true)
            {
                if (_historyManager.ExportCsv(dialog.FileName))
                {
                    MessageBox.Show(this, "导出成功。", "ColorPickerTray", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(this, "导出失败。", "ColorPickerTray", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "是否清空所有历史记录？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _historyManager.Clear();
                HistoryGrid.ItemsSource = null;
                HistoryGrid.ItemsSource = _historyManager.Records;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
