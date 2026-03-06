using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Core.Models;
using HabitTracker.Core.Data;

namespace HabitTracker.App
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _dbContext;

        public MainWindow()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _ = LoadHabitsAsync(); // Запускаем асинхронную загрузку без ожидания
        }

        // Единственный метод загрузки, возвращающий Task
        private async System.Threading.Tasks.Task LoadHabitsAsync()
        {
            var habits = await _dbContext.Habits.ToListAsync();
            HabitsListBox.ItemsSource = habits;
            HabitsListBox.DisplayMemberPath = "Name";
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var newHabit = new Habit
            {
                Name = "Встать в 6 утра",
                TargetValue = 1,
                Unit = "раз"
            };

            await _dbContext.Habits.AddAsync(newHabit);
            await _dbContext.SaveChangesAsync();

            // Теперь можно ожидать завершения загрузки
            await LoadHabitsAsync();
        }
    }
}