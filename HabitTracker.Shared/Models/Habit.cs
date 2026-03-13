using System.Collections.Generic;

namespace HabitTracker.Shared.Models
{
    public class Habit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TargetValue { get; set; }
        public string Unit { get; set; } = "раз";
        public bool IsActive { get; set; } = true;
        public string ColorHex { get; set; } = "#1CA9C9"; // цвет по умолчанию (голубой)

        // Навигационные свойства
        public ICollection<HabitRecord>? Records { get; set; }
        public ICollection<Schedule>? Schedules { get; set; }
    }
}