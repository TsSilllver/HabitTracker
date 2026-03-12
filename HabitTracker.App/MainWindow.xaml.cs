using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Shared.Models;
using HabitTracker.Core.Data;
using System.IO;
using Microsoft.Win32;
using HabitTracker.App;

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

        private async void AddHabit_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddHabitWindow(_dbContext);
            if (addWindow.ShowDialog() == true)
            {
                await LoadHabitsAsync(); // обновляем список
            }
        }

        private void MarkDone_Click(object sender, RoutedEventArgs e)
        {
            if (HabitsListBox.SelectedItem is Habit selected)
            {
                var markWindow = new MarkDoneWindow(_dbContext, selected);
                if (markWindow.ShowDialog() == true)
                {
                    // При необходимости можно обновить данные, но пока ничего
                }
            }
            else
            {
                MessageBox.Show("Выберите привычку");
            }
        }

        private void ShowStats_Click(object sender, RoutedEventArgs e)
        {
            var statsWindow = new StatisticsWindow(_dbContext);
            statsWindow.ShowDialog();
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json",
                FileName = $"habits_export_{DateTime.Now:yyyyMMdd}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Загружаем все данные из БД (без Include, чтобы не тащить навигационные свойства)
                    var habits = await _dbContext.Habits.ToListAsync();
                    var records = await _dbContext.HabitRecords.ToListAsync();
                    var schedules = await _dbContext.Schedules.ToListAsync();

                    // Разрываем циклические ссылки: обнуляем навигационные свойства
                    foreach (var r in records)
                        r.Habit = null;
                    foreach (var s in schedules)
                        s.Habit = null;
                    foreach (var h in habits)
                    {
                        h.Records = null!;
                        h.Schedules = null!;
                    }

                    var exportData = new DataExport
                    {
                        Habits = habits,
                        HabitRecords = records,
                        Schedules = schedules
                    };

                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                        // ReferenceHandler не нужен, так как мы убрали циклы
                    };
                    string json = System.Text.Json.JsonSerializer.Serialize(exportData, options);

                    await File.WriteAllTextAsync(saveFileDialog.FileName, json);

                    MessageBox.Show($"Экспорт выполнен успешно! Сохранено {habits.Count} привычек.",
                                    "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Читаем файл асинхронно
                    string json = await File.ReadAllTextAsync(openFileDialog.FileName);
                    var importData = System.Text.Json.JsonSerializer.Deserialize<DataExport>(json);

                    if (importData == null)
                    {
                        MessageBox.Show("Файл не содержит данных или повреждён.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Сбрасываем идентификаторы, чтобы EF добавил их как новые
                    foreach (var habit in importData.Habits)
                    {
                        habit.Id = 0;
                        // Также нужно сбросить навигационные свойства, чтобы они не мешали
                        habit.Records = null!;
                        habit.Schedules = null!;
                    }
                    foreach (var record in importData.HabitRecords)
                    {
                        record.Id = 0;
                        record.Habit = null; // убираем навигацию
                    }
                    foreach (var schedule in importData.Schedules)
                    {
                        schedule.Id = 0;
                        schedule.Habit = null;
                    }

                    // Добавляем всё в контекст
                    await _dbContext.Habits.AddRangeAsync(importData.Habits);
                    await _dbContext.HabitRecords.AddRangeAsync(importData.HabitRecords);
                    await _dbContext.Schedules.AddRangeAsync(importData.Schedules);

                    // Сохраняем изменения
                    await _dbContext.SaveChangesAsync();

                    MessageBox.Show($"Импорт выполнен! Добавлено привычек: {importData.Habits.Count}, записей: {importData.HabitRecords.Count}, расписаний: {importData.Schedules.Count}.", "Импорт", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем список привычек на главном окне
                    await LoadHabitsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void Sync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var api = new ApiService();
                var serverHabits = await api.GetHabitsAsync();

                if (serverHabits != null)
                {
                    var syncWindow = new SyncWindow(serverHabits);
                    syncWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Не удалось получить данные с сервера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обращении к серверу: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}