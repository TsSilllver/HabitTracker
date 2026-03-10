using HabitTracker.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace HabitTracker.Core.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitRecord> HabitRecords { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HabitTracker",
                "habits.db");
            string? directory = Path.GetDirectoryName(dbPath);
            if (directory != null) Directory.CreateDirectory(directory);
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HabitRecord>()
                .HasOne(hr => hr.Habit)
                .WithMany(h => h.Records)
                .HasForeignKey(hr => hr.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Habit)
                .WithMany(h => h.Schedules)
                .HasForeignKey(s => s.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}