using BrsCalculator.Domain.Entities;
using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Domain.Brs;

public static class BrsCalculationEngine
{
    public static IReadOnlyDictionary<Guid, decimal> ComputeScores(
        IReadOnlyList<SubjectNode> nodes,
        Func<Guid, decimal?>? scoreOverrides = null)
    {
        var byParent = nodes
            .Where(n => n.ParentId.HasValue)
            .GroupBy(n => n.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(n => n.SortOrder).ToList());

        var scores = new Dictionary<Guid, decimal>();
        foreach (var node in nodes.OrderByDescending(n => n.Path.Length))
        {
            var children = byParent.GetValueOrDefault(node.Id);
            if (children is null or { Count: 0 })
            {
                var raw = scoreOverrides?.Invoke(node.Id) ?? node.ActualScore;
                scores[node.Id] = Math.Clamp(raw ?? 0, 0, node.MaxScore);
            }
            else
            {
                scores[node.Id] = children.Sum(c =>
                    scores.GetValueOrDefault(c.Id) * c.Coefficient);
            }
        }

        return scores;
    }

    public static GradeResult EvaluateGrade(
        IReadOnlyList<SubjectNode> nodes,
        IReadOnlyDictionary<Guid, decimal> scores)
    {
        var root = nodes.FirstOrDefault(n => n.LevelType == NodeLevelType.Subject);
        var total = root is null ? 0 : scores.GetValueOrDefault(root.Id);

        var exam = nodes.FirstOrDefault(n => n.IsExam);
        decimal? examScore = null;
        bool? examPassed = null;

        if (exam is not null)
        {
            var intermediate = FindAncestor(nodes, exam, NodeLevelType.Certification);
            if (intermediate?.Coefficient > 0)
            {
                examScore = scores.GetValueOrDefault(exam.Id);
                examPassed = examScore >= 40;
            }
        }

        var totalPassed = total >= 40;
        var passed = totalPassed && (examPassed is null or true);

        if (!passed)
        {
            var reason = !totalPassed ? "Итоговый балл ниже 40." :
                examPassed == false ? "Экзамен/зачёт ниже 40." : null;
            return new GradeResult(total, "2", false, examPassed, examScore, reason);
        }

        var grade = total switch
        {
            >= 80 => "5",
            >= 60 => "4",
            >= 40 => "3",
            _ => "2"
        };

        return new GradeResult(total, grade, true, examPassed, examScore, null);
    }

    public static decimal TargetTotalScore(string targetGrade) => targetGrade switch
    {
        "5" => 80,
        "4" => 60,
        "3" => 40,
        _ => 40
    };

    private static SubjectNode? FindAncestor(
        IReadOnlyList<SubjectNode> nodes,
        SubjectNode start,
        NodeLevelType level)
    {
        var map = nodes.ToDictionary(n => n.Id);
        var current = start;
        while (current.ParentId is { } parentId && map.TryGetValue(parentId, out var parent))
        {
            if (parent.LevelType == level)
                return parent;
            current = parent;
        }

        return null;
    }
}
