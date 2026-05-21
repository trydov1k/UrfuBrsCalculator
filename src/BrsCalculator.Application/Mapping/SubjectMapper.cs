using BrsCalculator.Application.DTOs;
using BrsCalculator.Domain.Brs;
using BrsCalculator.Domain.Entities;

namespace BrsCalculator.Application.Mapping;

public static class SubjectMapper
{
    public static SubjectListItemDto ToListItem(Subject subject, IEnumerable<SubjectNode> nodes)
    {
        var nodeList = nodes as IReadOnlyList<SubjectNode> ?? nodes.ToList();
        var scores = BrsCalculationEngine.ComputeScores(nodeList);
        var grade = BrsCalculationEngine.EvaluateGrade(nodeList, scores);
        return new SubjectListItemDto(subject.Id, subject.Name, grade.TotalScore, grade.GradeLabel, grade.IsPassed);
    }

    public static SubjectDetailDto ToDetail(Subject subject, IEnumerable<SubjectNode> nodes)
    {
        var nodeList = nodes as IReadOnlyList<SubjectNode> ?? nodes.ToList();
        var scores = BrsCalculationEngine.ComputeScores(nodeList);
        var grade = BrsCalculationEngine.EvaluateGrade(nodeList, scores);
        var childIds = nodeList.Where(n => n.ParentId.HasValue).Select(n => n.ParentId!.Value).ToHashSet();

        var nodeDtos = nodeList
            .OrderBy(n => n.Path)
            .Select(n => new SubjectNodeDto(
                n.Id,
                n.ParentId,
                n.Path,
                n.Name,
                n.LevelType,
                n.CertificationKind,
                n.MaxScore,
                n.Coefficient,
                n.IsExam,
                n.ActualScore,
                scores.GetValueOrDefault(n.Id),
                !childIds.Contains(n.Id) && n.LevelType == Domain.Enums.NodeLevelType.Component,
                n.SortOrder))
            .ToList();

        return new SubjectDetailDto(
            subject.Id,
            subject.Name,
            nodeDtos,
            grade.TotalScore,
            grade.GradeLabel,
            grade.IsPassed,
            grade.FailureReason);
    }

    public static SubjectDetailDto ToDetailWithOverrides(
        Subject subject,
        IEnumerable<SubjectNode> nodes,
        IReadOnlyDictionary<Guid, decimal?>? overrides)
    {
        var nodeList = nodes as IReadOnlyList<SubjectNode> ?? nodes.ToList();
        decimal? Override(Guid id) =>
            overrides is not null && overrides.TryGetValue(id, out var v) ? v : null;

        var scores = BrsCalculationEngine.ComputeScores(nodeList, Override);
        var grade = BrsCalculationEngine.EvaluateGrade(nodeList, scores);
        var childIds = nodeList.Where(n => n.ParentId.HasValue).Select(n => n.ParentId!.Value).ToHashSet();

        var nodeDtos = nodeList
            .OrderBy(n => n.Path)
            .Select(n => new SubjectNodeDto(
                n.Id,
                n.ParentId,
                n.Path,
                n.Name,
                n.LevelType,
                n.CertificationKind,
                n.MaxScore,
                n.Coefficient,
                n.IsExam,
                overrides is not null && overrides.TryGetValue(n.Id, out var o) ? o : n.ActualScore,
                scores.GetValueOrDefault(n.Id),
                !childIds.Contains(n.Id) && n.LevelType == Domain.Enums.NodeLevelType.Component,
                n.SortOrder))
            .ToList();

        return new SubjectDetailDto(
            subject.Id,
            subject.Name,
            nodeDtos,
            grade.TotalScore,
            grade.GradeLabel,
            grade.IsPassed,
            grade.FailureReason);
    }
}
