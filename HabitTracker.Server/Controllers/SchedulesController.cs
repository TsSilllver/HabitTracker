using HabitTracker.Server.Data;
using HabitTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SchedulesController : ControllerBase
{
    private readonly ServerDbContext _context;

    public SchedulesController(ServerDbContext context)
    {
        _context = context;
    }

    // GET: api/schedules
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedules()
    {
        return await _context.Schedules
            .Include(s => s.Habit)
            .ToListAsync();
    }

    // GET: api/schedules/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Schedule>> GetSchedule(int id)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Habit)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null)
            return NotFound();

        return schedule;
    }

    // POST: api/schedules
    [HttpPost]
    public async Task<ActionResult<Schedule>> PostSchedule(Schedule schedule)
    {
        // Проверяем существование привычки
        var habit = await _context.Habits.FindAsync(schedule.HabitId);
        if (habit == null)
            return BadRequest($"Habit with id {schedule.HabitId} not found.");

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, schedule);
    }

    // PUT: api/schedules/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSchedule(int id, Schedule schedule)
    {
        if (id != schedule.Id)
            return BadRequest();

        var habit = await _context.Habits.FindAsync(schedule.HabitId);
        if (habit == null)
            return BadRequest($"Habit with id {schedule.HabitId} not found.");

        _context.Entry(schedule).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ScheduleExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/schedules/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
            return NotFound();

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ScheduleExists(int id)
    {
        return _context.Schedules.Any(e => e.Id == id);
    }
}