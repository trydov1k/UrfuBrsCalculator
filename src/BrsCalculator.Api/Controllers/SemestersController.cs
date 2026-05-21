using System.Security.Claims;
using BrsCalculator.Application.DTOs;
using BrsCalculator.Domain.Entities;
using BrsCalculator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrsCalculator.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SemestersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SemestersController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SemesterDto>>> GetAll()
    {
        var items = await _db.Semesters
            .Where(s => s.UserId == UserId)
            .OrderByDescending(s => s.AcademicYearStart)
            .ThenByDescending(s => s.Season)
            .Select(s => new SemesterDto(
                s.Id,
                s.Name,
                s.AcademicYearStart,
                s.Season,
                s.Subjects.Count))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SemesterDto>> Get(Guid id)
    {
        var semester = await _db.Semesters
            .Where(s => s.Id == id && s.UserId == UserId)
            .Select(s => new SemesterDto(s.Id, s.Name, s.AcademicYearStart, s.Season, s.Subjects.Count))
            .FirstOrDefaultAsync();

        return semester is null ? NotFound() : Ok(semester);
    }

    [HttpPost]
    public async Task<ActionResult<SemesterDto>> Create(CreateSemesterRequest request)
    {
        var semester = new Semester
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Name = request.Name,
            AcademicYearStart = request.AcademicYearStart,
            Season = request.Season
        };

        _db.Semesters.Add(semester);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = semester.Id },
            new SemesterDto(semester.Id, semester.Name, semester.AcademicYearStart, semester.Season, 0));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateSemesterRequest request)
    {
        var semester = await _db.Semesters.FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (semester is null) return NotFound();

        semester.Name = request.Name;
        semester.AcademicYearStart = request.AcademicYearStart;
        semester.Season = request.Season;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var semester = await _db.Semesters.FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (semester is null) return NotFound();

        _db.Semesters.Remove(semester);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
