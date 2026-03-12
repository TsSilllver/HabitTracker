using HabitTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Server.Data
{
    public class ServerDbContext : DbContext
    {
        // Конструктор, который принимает параметры конфигурации
        public ServerDbContext(DbContextOptions<ServerDbContext> options)
            : base(options)
        {
        }

        // DbSet для каждой модели (таблицы)
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitRecord> HabitRecords { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        // Настройка связей между таблицами
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Связь Habit -> HabitRecord (один ко многим)
            modelBuilder.Entity<HabitRecord>()
                .HasOne(hr => hr.Habit)                // у записи есть одна привычка
                .WithMany(h => h.Records)              // у привычки много записей
                .HasForeignKey(hr => hr.HabitId)       // внешний ключ HabitId
                .OnDelete(DeleteBehavior.Cascade);     // при удалении привычки удалять и записи

            // Связь Habit -> Schedule (один ко многим)
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Habit)
                .WithMany(h => h.Schedules)
                .HasForeignKey(s => s.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}