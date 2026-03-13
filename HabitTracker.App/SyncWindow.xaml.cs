using System.Windows;

namespace HabitTracker.App
{
    public partial class SyncWindow : Window
    {
        public SyncDirection SelectedDirection { get; private set; } = SyncDirection.Cancel;

        public SyncWindow()
        {
            InitializeComponent();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            SelectedDirection = SyncDirection.Download;
            Close();
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            SelectedDirection = SyncDirection.Upload;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedDirection = SyncDirection.Cancel;
            Close();
        }
    }

    public enum SyncDirection
    {
        Cancel,
        Download,
        Upload
    }
}