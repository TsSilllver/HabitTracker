using HabitTracker.Core.Data;
using HabitTracker.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace HabitTracker.App
{
    public partial class AddHabitWindow : Window
    {
        private readonly AppDbContext _dbContext;

        public AddHabitWindow(AppDbContext context)
        {
            InitializeComponent();
            _dbContext = context;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Простейшая валидация
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите название");
                return;
            }
            if (!int.TryParse(TargetTextBox.Text, out int target) || target <= 0)
            {
                MessageBox.Show("Введите корректное целевое значение");
                return;
            }

            var habit = new Habit
            {
                Name = NameTextBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text.Trim(),
                TargetValue = target,
                Unit = UnitTextBox.Text.Trim(),
                IsActive = true
            };

            await _dbContext.Habits.AddAsync(habit);
            await _dbContext.SaveChangesAsync(); // сохраняем, чтобы получить Id

            // Добавляем расписание
            var dayChecks = new[] { (MonCheck, DayOfWeek.Monday), (TueCheck, DayOfWeek.Tuesday), (WedCheck, DayOfWeek.Wednesday), (ThuCheck, DayOfWeek.Thursday), (FriCheck, DayOfWeek.Friday), (SatCheck, DayOfWeek.Saturday), (SunCheck, DayOfWeek.Sunday) };
            foreach (var (check, day) in dayChecks)
            {
                if (check.IsChecked == true)
                {
                    _dbContext.Schedules.Add(new Schedule { HabitId = habit.Id, DayOfWeek = day });
                }
            }

            await _dbContext.SaveChangesAsync();
            DialogResult = true; // закрываем окно с успехом
            Close();
        }
    }
}