using HabitTracker.Core.Models;

namespace HabitTracker.Core.Models
{
    public class HabitRecord
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public string? Note { get; set; }
        public Habit? Habit { get; set; }
    }
}