using System;

namespace HabitTracker.Shared.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        // Навигационное свойство
        public Habit? Habit { get; set; }
    }
}