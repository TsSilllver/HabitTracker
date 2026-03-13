using HabitTracker.Server.Data;
using HabitTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly ServerDbContext _context;

        public SyncController(ServerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DataExport>> GetAllData()
        {
            var habits = await _context.Habits.ToListAsync();
            var records = await _context.HabitRecords.ToListAsync();
            var schedules = await _context.Schedules.ToListAsync();

            foreach (var r in records) r.Habit = null;
            foreach (var s in schedules) s.Habit = null;
            foreach (var h in habits)
            {
                h.Records = null;
                h.Schedules = null;
            }

            return new DataExport
            {
                Habits = habits,
                HabitRecords = records,
                Schedules = schedules
            };
        }

        [HttpPost]
        public async Task<IActionResult> ReplaceAllData(DataExport data)
        {
            Console.WriteLine($"ReplaceAllData called at {DateTime.Now}");
            try
            {
                Console.WriteLine($"Received data: Habits={data.Habits?.Count ?? 0}, Records={data.HabitRecords?.Count ?? 0}, Schedules={data.Schedules?.Count ?? 0}");

                // Очищаем существующие данные
                _context.HabitRecords.RemoveRange(_context.HabitRecords);
                _context.Schedules.RemoveRange(_context.Schedules);
                _context.Habits.RemoveRange(_context.Habits);
                await _context.SaveChangesAsync();

                if (data.Habits == null || !data.Habits.Any())
                {
                    return Ok();
                }

                // Сохраняем привычки и получаем новые ID
                var habits = data.Habits.ToList();
                foreach (var habit in habits)
                {
                    habit.Id = 0;
                    _context.Habits.Add(habit);
                }
                await _context.SaveChangesAsync();

                // Создаём маппинг старых ID на новые (по индексу, т.к. порядок сохраняется)
                var idMap = new Dictionary<int, int>();
                for (int i = 0; i < data.Habits.Count; i++)
                {
                    idMap[data.Habits.ElementAt(i).Id] = habits[i].Id;
                }

                // Добавляем записи с обновлёнными HabitId
                if (data.HabitRecords != null)
                {
                    foreach (var record in data.HabitRecords)
                    {
                        record.Id = 0;
                        if (idMap.TryGetValue(record.HabitId, out int newHabitId))
                            record.HabitId = newHabitId;
                        else
                            continue;
                        _context.HabitRecords.Add(record);
                    }
                }

                // Добавляем расписания с обновлёнными HabitId
                if (data.Schedules != null)
                {
                    foreach (var schedule in data.Schedules)
                    {
                        schedule.Id = 0;
                        if (idMap.TryGetValue(schedule.HabitId, out int newHabitId))
                            schedule.HabitId = newHabitId;
                        else
                            continue;
                        _context.Schedules.Add(schedule);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReplaceAllData: {ex}");
                // Возвращаем детали ошибки клиенту
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }
    }
}