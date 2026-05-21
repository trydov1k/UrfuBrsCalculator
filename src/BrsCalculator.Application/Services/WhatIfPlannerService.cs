using BrsCalculator.Domain.Brs;
using BrsCalculator.Domain.Entities;
using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Application.Services;

public class WhatIfPlannerService
{
    public const decimal ExamMinimumScore = 40m;

    public static Dictionary<Guid, decimal?> NormalizeOverrides(
        IReadOnlyList<SubjectNode> nodes,
        IReadOnlyDictionary<Guid, decimal?>? overrides)
    {
        var result = overrides is null
            ? new Dictionary<Guid, decimal?>()
            : new Dictionary<Guid, decimal?>(overrides);

        var exam = nodes.FirstOrDefault(n => n.IsExam);
        if (exam is null)
            return result;

        var raw = result.TryGetValue(exam.Id, out var value) ? value : exam.ActualScore;
        result[exam.Id] = Math.Clamp(Math.Max(ExamMinimumScore, raw ?? 0), 0, exam.MaxScore);
        return result;
    }

    public WhatIfPlanResult Plan(
        IReadOnlyList<SubjectNode> nodes,
        IReadOnlyDictionary<Guid, decimal?> overrides,
        string targetGrade)
    {
        var normalized = NormalizeOverrides(nodes, overrides);

        decimal? Override(Guid id) =>
            normalized.TryGetValue(id, out var v) ? v : null;

        var scores = BrsCalculationEngine.ComputeScores(nodes, Override);
        var grade = BrsCalculationEngine.EvaluateGrade(nodes, scores);
        var targetTotal = BrsCalculationEngine.TargetTotalScore(targetGrade);
        var shortfall = Math.Max(0, targetTotal - grade.TotalScore);

        var leaves = GetAdjustableLeaves(nodes, normalized);
        var suggested = DistributeProportionally(leaves, shortfall, scores, Override);

        var projectedScores = BrsCalculationEngine.ComputeScores(nodes, id =>
            suggested.TryGetValue(id, out var s) ? s :
            normalized.TryGetValue(id, out var o) ? o :
            null);

        var projected = BrsCalculationEngine.EvaluateGrade(nodes, projectedScores);

        return new WhatIfPlanResult(
            grade.TotalScore,
            targetTotal,
            shortfall,
            suggested,
            projected.GradeLabel);
    }

    private static List<SubjectNode> GetAdjustableLeaves(
        IReadOnlyList<SubjectNode> nodes,
        IReadOnlyDictionary<Guid, decimal?> overrides)
    {
        var childCounts = nodes
            .Where(n => n.ParentId.HasValue)
            .GroupBy(n => n.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return nodes
            .Where(n => !childCounts.ContainsKey(n.Id))
            .Where(n => n.LevelType == NodeLevelType.Component)
            .Where(n => !n.ActualScore.HasValue || overrides.ContainsKey(n.Id))
            .ToList();
    }

    private static Dictionary<Guid, decimal> DistributeProportionally(
        IReadOnlyList<SubjectNode> leaves,
        decimal shortfall,
        IReadOnlyDictionary<Guid, decimal> currentScores,
        Func<Guid, decimal?>? overrides)
    {
        var result = new Dictionary<Guid, decimal>();
        if (leaves.Count == 0 || shortfall <= 0)
            return result;

        var totalMax = leaves.Sum(l => l.MaxScore);
        if (totalMax <= 0)
            return result;

        foreach (var leaf in leaves)
        {
            var share = shortfall * (leaf.MaxScore / totalMax);
            var current = overrides?.Invoke(leaf.Id) ?? leaf.ActualScore ?? currentScores.GetValueOrDefault(leaf.Id);
            var proposed = Math.Min(leaf.MaxScore, current + share);
            if (leaf.IsExam)
                proposed = Math.Max(ExamMinimumScore, proposed);
            result[leaf.Id] = Math.Round(proposed, 2);
        }

        return result;
    }
}

public record WhatIfPlanResult(
    decimal CurrentTotal,
    decimal TargetTotal,
    decimal Shortfall,
    IReadOnlyDictionary<Guid, decimal> SuggestedScores,
    string ProjectedGrade);
