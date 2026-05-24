using System.Security.Claims;
using BrsCalculator.Application.DTOs;
using BrsCalculator.Application.Mapping;
using BrsCalculator.Domain.Brs;
using BrsCalculator.Domain.Entities;
using BrsCalculator.Domain.Enums;
using BrsCalculator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrsCalculator.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/semesters/{semesterId:guid}/subjects/{subjectId:guid}/[controller]")]
public class NodesController : ControllerBase
{
    private readonly AppDbContext _db;

    public NodesController(AppDbContext db) => _db = db;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private async Task<Subject?> GetOwnedSubject(Guid semesterId, Guid subjectId) =>
        await _db.Subjects
            .Include(s => s.Semester)
            .Include(s => s.Nodes)
            .FirstOrDefaultAsync(s =>
                s.Id == subjectId &&
                s.SemesterId == semesterId &&
                s.Semester.UserId == UserId);

    [HttpPost]
    public async Task<ActionResult<SubjectDetailDto>> Create(
        Guid semesterId, Guid subjectId, CreateNodeRequest request)
    {
        var subject = await GetOwnedSubject(semesterId, subjectId);
        if (subject is null) return NotFound();

        if (request.IsExam && subject.Nodes.Any(n => n.IsExam))
            return BadRequest("В дисциплине может быть только один экзамен.");

        var parent = request.ParentId.HasValue
            ? subject.Nodes.FirstOrDefault(n => n.Id == request.ParentId)
            : null;

        if (request.ParentId.HasValue && parent is null)
            return BadRequest("Родительский узел не найден.");

        var path = BuildChildPath(subject.Nodes, parent?.Path ?? "0000", request.ParentId);
        var sortOrder = subject.Nodes.Count(n => n.ParentId == request.ParentId);

        var node = new SubjectNode
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            ParentId = request.ParentId,
            Path = path,
            Name = request.Name,
            LevelType = request.LevelType,
            CertificationKind = request.CertificationKind,
            MaxScore = request.IsExam ? 100 : ScorePrecision.Round(request.MaxScore),
            Coefficient = request.Coefficient,
            IsExam = request.IsExam,
            SortOrder = sortOrder
        };

        _db.SubjectNodes.Add(node);
        await _db.SaveChangesAsync();

        subject = await GetOwnedSubject(semesterId, subjectId);
        return Ok(SubjectMapper.ToDetail(subject!, subject!.Nodes));
    }

    [HttpPut("{nodeId:guid}")]
    public async Task<ActionResult<SubjectDetailDto>> Update(
        Guid semesterId, Guid subjectId, Guid nodeId, UpdateNodeRequest request)
    {
        var subject = await GetOwnedSubject(semesterId, subjectId);
        if (subject is null) return NotFound();

        var node = subject.Nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node is null) return NotFound();

        if (SubjectNodeRules.IsCoefficientOnlyEdit(node.LevelType))
        {
            node.Coefficient = request.Coefficient;
        }
        else
        {
            if (request.IsExam && !node.IsExam && subject.Nodes.Any(n => n.IsExam && n.Id != nodeId))
                return BadRequest("В дисциплине может быть только один экзамен.");

            node.Name = request.Name;
            node.MaxScore = node.IsExam || request.IsExam ? 100 : ScorePrecision.Round(request.MaxScore);
            node.Coefficient = request.Coefficient;
            node.IsExam = request.IsExam;
        }

        await _db.SaveChangesAsync();

        return Ok(SubjectMapper.ToDetail(subject, subject.Nodes));
    }

    [HttpDelete("{nodeId:guid}")]
    public async Task<ActionResult<SubjectDetailDto>> Delete(Guid semesterId, Guid subjectId, Guid nodeId)
    {
        var subject = await GetOwnedSubject(semesterId, subjectId);
        if (subject is null) return NotFound();

        var node = await _db.SubjectNodes.FirstOrDefaultAsync(n => n.Id == nodeId && n.SubjectId == subjectId);
        if (node is null) return NotFound();

        if (node.LevelType == NodeLevelType.Subject)
            return BadRequest("Корневой узел дисциплины удалить нельзя.");

        if (!SubjectNodeRules.CanDelete(node.LevelType))
            return BadRequest("Удалять можно только компоненты.");

        var ids = CollectSubtreeIds(subject.Nodes, nodeId)
            .OrderByDescending(id => subject.Nodes.First(n => n.Id == id).Path.Length)
            .ToList();

        foreach (var id in ids)
        {
            var toRemove = await _db.SubjectNodes.FindAsync(id);
            if (toRemove is not null)
                _db.SubjectNodes.Remove(toRemove);
        }

        await _db.SaveChangesAsync();

        subject = await GetOwnedSubject(semesterId, subjectId);
        return Ok(SubjectMapper.ToDetail(subject!, subject!.Nodes));
    }

    private static IEnumerable<Guid> CollectSubtreeIds(IEnumerable<SubjectNode> nodes, Guid rootId)
    {
        yield return rootId;
        foreach (var child in nodes.Where(n => n.ParentId == rootId))
        {
            foreach (var id in CollectSubtreeIds(nodes, child.Id))
                yield return id;
        }
    }

    [HttpPut("{nodeId:guid}/score")]
    public async Task<ActionResult<SubjectDetailDto>> UpdateScore(
        Guid semesterId, Guid subjectId, Guid nodeId, UpdateScoreRequest request)
    {
        var subject = await GetOwnedSubject(semesterId, subjectId);
        if (subject is null) return NotFound();

        var node = subject.Nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node is null) return NotFound();

        if (request.ActualScore.HasValue)
            node.ActualScore = Math.Clamp(ScorePrecision.Round(request.ActualScore.Value), 0, node.MaxScore);
        else
            node.ActualScore = null;

        await _db.SaveChangesAsync();
        return Ok(SubjectMapper.ToDetail(subject, subject.Nodes));
    }

    private static string BuildChildPath(IEnumerable<SubjectNode> nodes, string parentPath, Guid? parentId)
    {
        var siblings = nodes.Where(n => n.ParentId == parentId).ToList();
        var next = siblings.Count + 1;
        var segment = parentId is null ? "0001" : $"{next:D4}";
        return parentId is null ? segment : $"{parentPath}.{segment}";
    }
}
