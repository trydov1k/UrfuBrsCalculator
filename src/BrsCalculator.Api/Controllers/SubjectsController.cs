using System.Security.Claims;
using BrsCalculator.Application.DTOs;
using BrsCalculator.Application.Mapping;
using BrsCalculator.Application.Services;
using BrsCalculator.Domain.Entities;
using BrsCalculator.Domain.Enums;
using BrsCalculator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrsCalculator.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/semesters/{semesterId:guid}/[controller]")]
public class SubjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SubjectTemplateService _templateService;
    private readonly WhatIfPlannerService _whatIfPlanner;

    public SubjectsController(
        AppDbContext db,
        SubjectTemplateService templateService,
        WhatIfPlannerService whatIfPlanner)
    {
        _db = db;
        _templateService = templateService;
        _whatIfPlanner = whatIfPlanner;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private async Task<Semester?> GetOwnedSemester(Guid semesterId) =>
        await _db.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId && s.UserId == UserId);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SubjectListItemDto>>> List(Guid semesterId)
    {
        if (await GetOwnedSemester(semesterId) is null) return NotFound();

        var subjects = await _db.Subjects
            .Where(s => s.SemesterId == semesterId)
            .Include(s => s.Nodes)
            .ToListAsync();

        return Ok(subjects.Select(s => SubjectMapper.ToListItem(s, s.Nodes)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubjectDetailDto>> Get(Guid semesterId, Guid id)
    {
        if (await GetOwnedSemester(semesterId) is null) return NotFound();

        var subject = await _db.Subjects
            .Include(s => s.Nodes)
            .FirstOrDefaultAsync(s => s.Id == id && s.SemesterId == semesterId);

        return subject is null ? NotFound() : Ok(SubjectMapper.ToDetail(subject, subject.Nodes));
    }

    [HttpPost]
    public async Task<ActionResult<SubjectDetailDto>> Create(Guid semesterId, CreateSubjectRequest request)
    {
        if (await GetOwnedSemester(semesterId) is null) return NotFound();

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            SemesterId = semesterId,
            Name = request.Name
        };

        var nodes = _templateService.BuildTemplate(subject.Id, request.Name, request.LessonTypes);
        subject.Nodes = nodes.ToList();

        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { semesterId, id = subject.Id },
            SubjectMapper.ToDetail(subject, subject.Nodes));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid semesterId, Guid id, UpdateSubjectRequest request)
    {
        if (await GetOwnedSemester(semesterId) is null) return NotFound();

        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id && s.SemesterId == semesterId);
        if (subject is null) return NotFound();

        subject.Name = request.Name;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid semesterId, Guid id)
    {
        if (await GetOwnedSemester(semesterId) is null) return NotFound();

        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == id && s.SemesterId == semesterId);
        if (subject is null) return NotFound();

        _db.Subjects.Remove(subject);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/preview")]
    public async Task<ActionResult<SubjectDetailDto>> Preview(Guid semesterId, Guid id, WhatIfRequest request)
    {
        if (await GetOwnedSemester(semesterId) is null) return NotFound();

        var subject = await _db.Subjects.Include(s => s.Nodes)
            .FirstOrDefaultAsync(s => s.Id == id && s.SemesterId == semesterId);
        if (subject is null) return NotFound();

        var normalized = WhatIfPlannerService.NormalizeOverrides(
            subject.Nodes.ToList(), request.Overrides);
        return Ok(SubjectMapper.ToDetailWithOverrides(subject, subject.Nodes, normalized));
    }

    [HttpPost("{id:guid}/what-if")]
    public async Task<ActionResult<WhatIfResultDto>> WhatIf(Guid semesterId, Guid id, WhatIfRequest request)
    {
        if (await GetOwnedSemester(semesterId) is null) return NotFound();

        var subject = await _db.Subjects.Include(s => s.Nodes)
            .FirstOrDefaultAsync(s => s.Id == id && s.SemesterId == semesterId);
        if (subject is null) return NotFound();

        var plan = _whatIfPlanner.Plan(subject.Nodes.ToList(), request.Overrides, request.TargetGrade);
        return Ok(new WhatIfResultDto(
            plan.CurrentTotal,
            plan.TargetTotal,
            plan.Shortfall,
            plan.SuggestedScores,
            plan.ProjectedGrade));
    }
}
