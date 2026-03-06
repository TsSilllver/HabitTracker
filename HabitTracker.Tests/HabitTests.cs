using HabitTracker.Core.Models;
namespace HabitTracker.Tests;

public class HabitTests
{
    [Fact]
    public void CanCreateHabitWithName()
    {
        // Arrange (Подготовка)
        var habit = new Habit { Name = "Бег", TargetValue = 5, Unit = "км" };

        // Act (Действие) - тут пока ничего не делаем

        // Assert (Проверка)
        Assert.Equal("Бег", habit.Name);
        Assert.Equal(5, habit.TargetValue);
    }
}