using HabitTracker.Core.Data;
using HabitTracker.Shared.Models;
using System;
using System.Windows;

namespace HabitTracker.App
{
    public partial class MarkDoneWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private readonly Habit _habit;

        public MarkDoneWindow(AppDbContext context, Habit habit)
        {
            InitializeComponent();
            _dbContext = context;
            _habit = habit;
            HabitNameText.Text = $"{habit.Name} (цель: {habit.TargetValue} {habit.Unit})";
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ValueTextBox.Text, out int value) || value < 0)
            {
                MessageBox.Show("Введите корректное значение (неотрицательное число).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var record = new HabitRecord
            {
                HabitId = _habit.Id,
                Date = DateTime.Today,
                Value = value,
                Note = string.IsNullOrWhiteSpace(NoteTextBox.Text) ? null : NoteTextBox.Text.Trim()
            };

            await _dbContext.HabitRecords.AddAsync(record);
            await _dbContext.SaveChangesAsync();
            DialogResult = true;
            Close();
        }
    }
}