using HabitTracker.Core.Data;
using HabitTracker.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HabitTracker.App
{
    public partial class StatisticsWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private int? _selectedHabitId;

        public StatisticsWindow(AppDbContext context, int? habitId = null)
        {
            InitializeComponent();
            _dbContext = context;
            _selectedHabitId = habitId;
            Loaded += async (s, e) => await LoadHabitsAsync();
        }

        private async System.Threading.Tasks.Task LoadHabitsAsync()
        {
            var habits = await _dbContext.Habits.ToListAsync();
            HabitsCombo.ItemsSource = habits;
            if (_selectedHabitId.HasValue)
            {
                HabitsCombo.SelectedItem = habits.FirstOrDefault(h => h.Id == _selectedHabitId.Value);
            }
        }

        private async void HabitsCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (HabitsCombo.SelectedItem is Habit selected)
            {
                await LoadStatisticsAsync(selected.Id);
            }
        }

        private async System.Threading.Tasks.Task LoadStatisticsAsync(int habitId)
        {
            var records = await _dbContext.HabitRecords
                .Where(r => r.HabitId == habitId && r.Date >= DateTime.Today.AddDays(-30))
                .OrderBy(r => r.Date)
                .ToListAsync();

            var habit = await _dbContext.Habits.FindAsync(habitId);

            var chartValues = new ChartValues<int>();
            var labels = new List<string>();

            foreach (var day in Enumerable.Range(0, 30).Select(offset => DateTime.Today.AddDays(-offset).Date).Reverse())
            {
                var record = records.FirstOrDefault(r => r.Date.Date == day);
                chartValues.Add(record?.Value ?? 0);
                labels.Add(day.ToString("dd.MM"));
            }

            var series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = habit?.Name ?? "Привычка",
                    Values = chartValues,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 8
                }
            };

            ProgressChart.Series = series;
            ProgressChart.AxisX.Clear();
            ProgressChart.AxisX.Add(new Axis
            {
                Title = "Дни",
                Labels = labels,
                LabelsRotation = 45
            });
            ProgressChart.AxisY.Clear();
            ProgressChart.AxisY.Add(new Axis
            {
                Title = "Значение",
                MinValue = 0
            });
        }
    }
}