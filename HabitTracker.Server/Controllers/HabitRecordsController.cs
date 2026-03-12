using HabitTracker.Server.Data;
using HabitTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HabitRecordsController : ControllerBase
{
    private readonly ServerDbContext _context;

    public HabitRecordsController(ServerDbContext context)
    {
        _context = context;
    }

    // GET: api/habitrecords
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HabitRecord>>> GetHabitRecords()
    {
        return await _context.HabitRecords
            .Include(r => r.Habit) // опционально, если нужны данные о привычке
            .ToListAsync();
    }

    // GET: api/habitrecords/5
    [HttpGet("{id}")]
    public async Task<ActionResult<HabitRecord>> GetHabitRecord(int id)
    {
        var habitRecord = await _context.HabitRecords
            .Include(r => r.Habit)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (habitRecord == null)
            return NotFound();

        return habitRecord;
    }

    // POST: api/habitrecords
    [HttpPost]
    public async Task<ActionResult<HabitRecord>> PostHabitRecord(HabitRecord habitRecord)
    {
        // Проверяем, существует ли привычка с указанным HabitId
        var habit = await _context.Habits.FindAsync(habitRecord.HabitId);
        if (habit == null)
            return BadRequest($"Habit with id {habitRecord.HabitId} not found.");

        _context.HabitRecords.Add(habitRecord);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHabitRecord), new { id = habitRecord.Id }, habitRecord);
    }

    // PUT: api/habitrecords/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHabitRecord(int id, HabitRecord habitRecord)
    {
        if (id != habitRecord.Id)
            return BadRequest();

        // Проверяем существование привычки
        var habit = await _context.Habits.FindAsync(habitRecord.HabitId);
        if (habit == null)
            return BadRequest($"Habit with id {habitRecord.HabitId} not found.");

        _context.Entry(habitRecord).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!HabitRecordExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/habitrecords/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHabitRecord(int id)
    {
        var habitRecord = await _context.HabitRecords.FindAsync(id);
        if (habitRecord == null)
            return NotFound();

        _context.HabitRecords.Remove(habitRecord);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool HabitRecordExists(int id)
    {
        return _context.HabitRecords.Any(e => e.Id == id);
    }
}