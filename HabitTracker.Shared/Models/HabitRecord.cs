namespace HabitTracker.Shared.Models
{
    public class HabitRecord
    {
        public int Id { get; set; }
        public int HabitId { get; set; } // исправлено с HabiId
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public string? Note { get; set; }
        public Habit? Habit { get; set; }
    }
}