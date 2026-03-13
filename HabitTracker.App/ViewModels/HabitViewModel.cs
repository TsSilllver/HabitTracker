using HabitTracker.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HabitTracker.App.ViewModels
{
    public class HabitViewModel
    {
        private readonly Habit _habit;
        private readonly List<HabitRecord> _recentRecords;

        public HabitViewModel(Habit habit, List<HabitRecord> recentRecords)
        {
            _habit = habit ?? throw new ArgumentNullException(nameof(habit));
            _recentRecords = recentRecords ?? new List<HabitRecord>();
        }

        public int Id => _habit.Id;
        public string Name => _habit.Name;
        public string? Description => _habit.Description;
        public int TargetValue => _habit.TargetValue;
        public string Unit => _habit.Unit;
        public string ColorHex => _habit.ColorHex;

        public string TargetSummary => $"Цель: {TargetValue} {Unit}";

        public List<DayCompletion> LastSevenDaysCompletion
        {
            get
            {
                var result = new List<DayCompletion>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    var record = _recentRecords.FirstOrDefault(r => r.Date.Date == date);
                    bool completed = record != null && record.Value >= TargetValue;
                    result.Add(new DayCompletion
                    {
                        Date = date,
                        Completed = completed,
                        ColorHex = _habit.ColorHex
                    });
                }
                return result;
            }
        }
    }

    public class DayCompletion
    {
        public DateTime Date { get; set; }
        public bool Completed { get; set; }
        public string ColorHex { get; set; } = "#1CA9C9";
    }
}