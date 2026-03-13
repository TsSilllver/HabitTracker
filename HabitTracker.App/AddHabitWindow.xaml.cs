
using HabitTracker.Core.Data;
using HabitTracker.Shared.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HabitTracker.App
{
    public partial class AddHabitWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private bool _isLoaded = false;

        public AddHabitWindow(AppDbContext context)
        {
            InitializeComponent();
            _dbContext = context;

            // Откладываем установку дней до полной загрузки окна
            this.Loaded += (s, e) =>
            {
                _isLoaded = true;
                if (DailyRadio.IsChecked == true)
                    SetAllDays(true);
            };
        }

        private void DailyRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
                SetAllDays(true);
        }

        private void WeeklyRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
                SetAllDays(false);
        }

        private void SetAllDays(bool check)
        {
            // Защита от null (если окно ещё не загружено)
            if (MonCheck == null) return;

            MonCheck.IsChecked = check;
            TueCheck.IsChecked = check;
            WedCheck.IsChecked = check;
            ThuCheck.IsChecked = check;
            FriCheck.IsChecked = check;
            SatCheck.IsChecked = check;
            SunCheck.IsChecked = check;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация и сохранение (без изменений)
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название привычки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(TargetTextBox.Text, out int target) || target <= 0)
            {
                MessageBox.Show("Введите корректное целевое значение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var habit = new Habit
            {
                Name = NameTextBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text.Trim(),
                TargetValue = target,
                Unit = UnitTextBox.Text.Trim(),
                IsActive = true,
                ColorHex = (ColorCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "#1CA9C9"
            };

            _dbContext.Habits.Add(habit);
            await _dbContext.SaveChangesAsync();

            // Сохраняем расписание
            var dayChecks = new[]
            {
                (MonCheck, DayOfWeek.Monday),
                (TueCheck, DayOfWeek.Tuesday),
                (WedCheck, DayOfWeek.Wednesday),
                (ThuCheck, DayOfWeek.Thursday),
                (FriCheck, DayOfWeek.Friday),
                (SatCheck, DayOfWeek.Saturday),
                (SunCheck, DayOfWeek.Sunday)
            };

            foreach (var (check, day) in dayChecks)
            {
                if (check.IsChecked == true)
                {
                    _dbContext.Schedules.Add(new Schedule { HabitId = habit.Id, DayOfWeek = day });
                }
            }

            await _dbContext.SaveChangesAsync();

            DialogResult = true;
            Close();
        }
    }
}