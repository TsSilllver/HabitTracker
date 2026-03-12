using HabitTracker.Shared.Models;
using System.Collections.Generic;
using System.Windows;

namespace HabitTracker.App
{
    public partial class SyncWindow : Window
    {
        public SyncWindow(List<Habit> habits)
        {
            InitializeComponent();
            HabitsDataGrid.ItemsSource = habits;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}