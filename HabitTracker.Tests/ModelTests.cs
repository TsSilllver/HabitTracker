using HabitTracker.Shared.Models;
using FluentAssertions;

namespace HabitTracker.Tests;

public class ModelTests
{
    [Fact]
    public void Habit_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var habit = new Habit();

        // Assert
        habit.Id.Should().Be(0);
        habit.Name.Should().BeEmpty();
        habit.TargetValue.Should().Be(0);
        habit.Unit.Should().Be("раз");
        habit.IsActive.Should().BeTrue();
        habit.Records.Should().BeNull();
        habit.Schedules.Should().BeNull();
    }

    [Fact]
    public void HabitRecord_ShouldInitializeCorrectly()
    {
        // Arrange
        var date = DateTime.Today;

        // Act
        var record = new HabitRecord
        {
            HabitId = 1,
            Date = date,
            Value = 10,
            Note = "Good job"
        };

        // Assert
        record.Id.Should().Be(0); // Id по умолчанию 0, пока не сохранён
        record.HabitId.Should().Be(1);
        record.Date.Should().Be(date);
        record.Value.Should().Be(10);
        record.Note.Should().Be("Good job");
        record.Habit.Should().BeNull(); // навигация не загружена
    }

    [Fact]
    public void Schedule_ShouldStoreDayOfWeek()
    {
        // Arrange
        var schedule = new Schedule
        {
            HabitId = 1,
            DayOfWeek = DayOfWeek.Monday
        };

        // Assert
        schedule.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }
}
