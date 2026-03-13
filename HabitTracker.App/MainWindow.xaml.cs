using HabitTracker.App.ViewModels;
using HabitTracker.Core.Data;
using HabitTracker.Shared.Models;
using HabitTracker.App;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HabitTracker.App
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private List<Habit> _allHabits = new();
        private List<HabitRecord> _recentRecords = new();

        public MainWindow()
        {
            InitializeComponent();
            _dbContext = new AppDbContext();
            _dbContext.Database.EnsureCreated();
            Loaded += async (s, e) => await LoadDataAsync();
        }

        // Загрузка всех данных (без фильтрации по вкладкам)
        private async Task LoadDataAsync()
        {
            _allHabits = await _dbContext.Habits.Include(h => h.Schedules).ToListAsync();
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            _recentRecords = await _dbContext.HabitRecords
                .Where(r => r.Date >= thirtyDaysAgo)
                .ToListAsync();

            var viewModels = _allHabits.Select(h => new HabitViewModel(h,
                _recentRecords.Where(r => r.HabitId == h.Id).ToList())).ToList();
            HabitsListBox.ItemsSource = viewModels;
        }

        // ----- Кнопки управления -----
        private async void AddHabit_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddHabitWindow(_dbContext);
            if (addWindow.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }

        private async void MarkDone_Click(object sender, RoutedEventArgs e)
        {
            if (HabitsListBox.SelectedItem is HabitViewModel selected)
            {
                var habit = _allHabits.FirstOrDefault(h => h.Id == selected.Id);
                if (habit != null)
                {
                    var markWindow = new MarkDoneWindow(_dbContext, habit);
                    markWindow.ShowDialog();
                    await LoadDataAsync();
                }
            }
            else
            {
                MessageBox.Show("Выберите привычку из списка.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                    var habits = await _dbContext.Habits.ToListAsync();
                    var records = await _dbContext.HabitRecords.ToListAsync();
                    var schedules = await _dbContext.Schedules.ToListAsync();

                    foreach (var r in records) r.Habit = null;
                    foreach (var s in schedules) s.Habit = null;
                    foreach (var h in habits)
                    {
                        h.Records = null;
                        h.Schedules = null;
                    }

                    var exportData = new DataExport
                    {
                        Habits = habits,
                        HabitRecords = records,
                        Schedules = schedules
                    };

                    var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                    string json = System.Text.Json.JsonSerializer.Serialize(exportData, options);
                    await System.IO.File.WriteAllTextAsync(saveFileDialog.FileName, json);

                    MessageBox.Show($"Экспорт выполнен успешно! Сохранено {habits.Count} привычек.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Импорт с правильной обработкой внешних ключей
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
                    string json = await System.IO.File.ReadAllTextAsync(openFileDialog.FileName);
                    var importData = System.Text.Json.JsonSerializer.Deserialize<DataExport>(json);

                    if (importData == null || importData.Habits == null)
                    {
                        MessageBox.Show("Файл не содержит данных или повреждён.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Очищаем локальную БД
                    _dbContext.HabitRecords.RemoveRange(_dbContext.HabitRecords);
                    _dbContext.Schedules.RemoveRange(_dbContext.Schedules);
                    _dbContext.Habits.RemoveRange(_dbContext.Habits);
                    await _dbContext.SaveChangesAsync();

                    var habits = importData.Habits.ToList();
                    var records = importData.HabitRecords?.ToList() ?? new List<HabitRecord>();
                    var schedules = importData.Schedules?.ToList() ?? new List<Schedule>();

                    // Добавляем привычки и получаем новые ID
                    foreach (var habit in habits)
                    {
                        habit.Id = 0;
                        _dbContext.Habits.Add(habit);
                    }
                    await _dbContext.SaveChangesAsync(); // теперь у habits проставлены реальные ID

                    // Словарь для маппинга старых ID на новые
                    var idMap = importData.Habits.Zip(habits, (old, newH) => new { OldId = old.Id, NewId = newH.Id })
                                                  .ToDictionary(x => x.OldId, x => x.NewId);

                    // Добавляем записи
                    foreach (var record in records)
                    {
                        record.Id = 0;
                        if (idMap.TryGetValue(record.HabitId, out int newHabitId))
                            record.HabitId = newHabitId;
                        else
                            continue; // если нет родителя, пропускаем
                        _dbContext.HabitRecords.Add(record);
                    }

                    // Добавляем расписания
                    foreach (var schedule in schedules)
                    {
                        schedule.Id = 0;
                        if (idMap.TryGetValue(schedule.HabitId, out int newHabitId))
                            schedule.HabitId = newHabitId;
                        else
                            continue;
                        _dbContext.Schedules.Add(schedule);
                    }

                    await _dbContext.SaveChangesAsync();

                    MessageBox.Show($"Импорт выполнен! Добавлено привычек: {habits.Count}, записей: {records.Count}, расписаний: {schedules.Count}.",
                                    "Импорт", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте: {ex.Message}\n\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Синхронизация (с отладочными сообщениями)
        private async void Sync_Click(object sender, RoutedEventArgs e)
        {
            var syncWindow = new SyncWindow();
            syncWindow.ShowDialog();

            if (syncWindow.SelectedDirection == SyncDirection.Cancel)
                return;

            try
            {
                var api = new ApiService();
                MessageBox.Show($"1. Подключаемся к серверу по адресу: {api.GetBaseUrl()}", "Отладка");

                if (syncWindow.SelectedDirection == SyncDirection.Download)
                {
                    MessageBox.Show("2. Запрашиваем данные с сервера...", "Отладка");
                    var serverData = await api.GetAllDataAsync();

                    if (serverData == null)
                    {
                        MessageBox.Show("3. Ответ от сервера: null (возможно, ошибка сериализации или пустой ответ)", "Отладка");
                        return;
                    }

                    MessageBox.Show($"4. Данные получены: привычек = {serverData.Habits.Count}, записей = {serverData.HabitRecords?.Count ?? 0}, расписаний = {serverData.Schedules?.Count ?? 0}", "Отладка");

                    // Очищаем локальную БД
                    _dbContext.HabitRecords.RemoveRange(_dbContext.HabitRecords);
                    _dbContext.Schedules.RemoveRange(_dbContext.Schedules);
                    _dbContext.Habits.RemoveRange(_dbContext.Habits);
                    await _dbContext.SaveChangesAsync();
                    MessageBox.Show("5. Локальная БД очищена", "Отладка");

                    var habits = serverData.Habits.ToList();
                    var records = serverData.HabitRecords?.ToList() ?? new List<HabitRecord>();
                    var schedules = serverData.Schedules?.ToList() ?? new List<Schedule>();

                    // Добавляем привычки и получаем новые ID
                    foreach (var habit in habits)
                    {
                        habit.Id = 0;
                        _dbContext.Habits.Add(habit);
                    }
                    await _dbContext.SaveChangesAsync();
                    MessageBox.Show("6. Привычки добавлены, новые ID получены", "Отладка");

                    // Словарь маппинга старых ID на новые
                    var idMap = serverData.Habits.Zip(habits, (old, newH) => new { OldId = old.Id, NewId = newH.Id })
                                                  .ToDictionary(x => x.OldId, x => x.NewId);

                    // Добавляем записи
                    foreach (var record in records)
                    {
                        record.Id = 0;
                        if (idMap.TryGetValue(record.HabitId, out int newHabitId))
                            record.HabitId = newHabitId;
                        else
                            continue;
                        _dbContext.HabitRecords.Add(record);
                    }

                    // Добавляем расписания
                    foreach (var schedule in schedules)
                    {
                        schedule.Id = 0;
                        if (idMap.TryGetValue(schedule.HabitId, out int newHabitId))
                            schedule.HabitId = newHabitId;
                        else
                            continue;
                        _dbContext.Schedules.Add(schedule);
                    }

                    await _dbContext.SaveChangesAsync();
                    MessageBox.Show("7. Данные сервера сохранены локально", "Отладка");

                    MessageBox.Show("Синхронизация завершена: данные загружены с сервера.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (syncWindow.SelectedDirection == SyncDirection.Upload)
                {
                    // Проверяем соединение с сервером через GET-запрос
                    MessageBox.Show("2. Проверяем соединение с сервером (GET api/sync)...", "Отладка");
                    try
                    {
                        var test = await api.GetAllDataAsync();
                        MessageBox.Show($"GET-запрос выполнен успешно, получено привычек: {test?.Habits?.Count ?? 0}", "Отладка");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при проверке соединения: {ex.Message}", "Ошибка");
                        return;
                    }

                    MessageBox.Show("3. Подготавливаем локальные данные для отправки...", "Отладка");
                    var localHabits = await _dbContext.Habits.ToListAsync();
                    var localRecords = await _dbContext.HabitRecords.ToListAsync();
                    var localSchedules = await _dbContext.Schedules.ToListAsync();

                    MessageBox.Show($"4. Локальные данные: привычек = {localHabits.Count}, записей = {localRecords.Count}, расписаний = {localSchedules.Count}", "Отладка");

                    // Убираем навигационные свойства для сериализации
                    foreach (var r in localRecords) r.Habit = null;
                    foreach (var s in localSchedules) s.Habit = null;
                    foreach (var h in localHabits)
                    {
                        h.Records = null;
                        h.Schedules = null;
                    }

                    var localData = new DataExport
                    {
                        Habits = localHabits,
                        HabitRecords = localRecords,
                        Schedules = localSchedules
                    };

                    MessageBox.Show("5. Отправляем данные на сервер...", "Отладка");
                    var success = await api.SendAllDataAsync(localData);
                    MessageBox.Show($"6. Результат отправки: {(success ? "успешно" : "ошибка")}", "Отладка");

                    if (success)
                    {
                        MessageBox.Show("Данные успешно отправлены на сервер.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при синхронизации:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ----- Обработчики контекстного меню (из карточки) -----
        private async void MarkMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                if (contextMenu.PlacementTarget is Button button && button.Tag is int habitId)
                {
                    var habit = _allHabits.FirstOrDefault(h => h.Id == habitId);
                    if (habit != null)
                    {
                        var markWindow = new MarkDoneWindow(_dbContext, habit);
                        markWindow.ShowDialog();
                        await LoadDataAsync();
                    }
                }
            }
        }

        private async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                if (contextMenu.PlacementTarget is Button button && button.Tag is int habitId)
                {
                    var habit = await _dbContext.Habits.FindAsync(habitId);
                    if (habit != null)
                    {
                        var result = MessageBox.Show($"Удалить привычку \"{habit.Name}\"?",
                            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            _dbContext.Habits.Remove(habit);
                            await _dbContext.SaveChangesAsync();
                            await LoadDataAsync();
                        }
                    }
                }
            }
        }

        private void StatsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
            {
                if (contextMenu.PlacementTarget is Button button && button.Tag is int habitId)
                {
                    var statsWindow = new StatisticsWindow(_dbContext, habitId);
                    statsWindow.ShowDialog();
                }
            }
        }
    }
}