using HabitTracker.Core.Models;

namespace HabitTracker.Core.Models;

public class Schedule
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DayOfWeek DayOfWeek { get; set; } // enum для дня недели

    public Habit? Habit { get; set; }
}