using HabitTracker.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace HabitTracker.Core.Data;

public class AppDbContext : DbContext
{
    public DbSet<Habit> Habits { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HabitTracker",
            "habits.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
