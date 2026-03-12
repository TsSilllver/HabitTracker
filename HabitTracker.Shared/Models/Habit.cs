namespace HabitTracker.Shared.Models;

public class Habit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TargetValue { get; set; } // целевое значение (например, 30 подтягиваний)
    public string Unit { get; set; } = "раз"; // единицы измерения
    public bool IsActive { get; set; } = true; // можно отключать привычку без удаления

    // Связи
    public ICollection<HabitRecord>? Records { get; set; }
    public ICollection<Schedule>? Schedules { get; set; }
}