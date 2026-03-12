using System.Collections.Generic;

namespace HabitTracker.Shared.Models
{
    public class DataExport
    {
        public List<Habit> Habits { get; set; } = new();
        public List<HabitRecord> HabitRecords { get; set; } = new();
        public List<Schedule> Schedules { get; set; } = new();
    }
}