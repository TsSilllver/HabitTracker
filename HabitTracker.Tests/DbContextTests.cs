using HabitTracker.Core.Data;
using HabitTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace HabitTracker.Tests;

public class DbContextTests
{
    [Fact]
    public async Task CanInsertAndRetrieveHabit()
    {
        // Arrange: создаём InMemory контекст
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var habit = new Habit
        {
            Name = "Test",
            TargetValue = 10,
            Unit = "times"
        };

        // Act
        await context.Habits.AddAsync(habit);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Habits.FirstOrDefaultAsync(h => h.Name == "Test");
        retrieved.Should().NotBeNull();
        retrieved!.TargetValue.Should().Be(10);
    }
}