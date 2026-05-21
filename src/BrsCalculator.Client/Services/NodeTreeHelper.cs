using BrsCalculator.Application.DTOs;
using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Client.Services;

public static class NodeTreeHelper
{
    public static bool CanHaveChildren(SubjectNodeDto node) =>
        node.LevelType is NodeLevelType.Certification or NodeLevelType.Component;

    public static bool CanAddChild(SubjectNodeDto parent) =>
        parent.LevelType is NodeLevelType.Certification or NodeLevelType.Component;

    public static bool IsEditableStructure(SubjectNodeDto node) =>
        node.LevelType != NodeLevelType.Subject;

    public static bool CanMarkExam(SubjectNodeDto parent) =>
        parent.LevelType == NodeLevelType.Certification &&
        parent.CertificationKind == CertificationKind.Intermediate;

    private const decimal WeightTolerance = 0.0001m;

    public static decimal SiblingCoefficientSum(IReadOnlyList<SubjectNodeDto> nodes, Guid? parentId) =>
        nodes.Where(n => n.ParentId == parentId).Sum(n => n.Coefficient);

    public static decimal SiblingMaxScoreSum(IReadOnlyList<SubjectNodeDto> nodes, Guid? parentId) =>
        nodes.Where(n => n.ParentId == parentId).Sum(n => n.MaxScore);

    public static IEnumerable<string> GetSiblingWeightWarnings(
        IReadOnlyList<SubjectNodeDto> nodes, Guid? parentId, NodeLevelType level)
    {
        if (!parentId.HasValue)
            yield break;

        var weightSum = SiblingCoefficientSum(nodes, parentId);
        if (Math.Abs(weightSum - 1m) <= WeightTolerance)
            yield break;

        if (level == NodeLevelType.Component && weightSum > 1m + WeightTolerance)
        {
            var maxSum = SiblingMaxScoreSum(nodes, parentId);
            if (Math.Abs(maxSum - 100m) <= WeightTolerance)
                yield break;

            yield return
                $"Сумма весов компонентов: {weightSum:0.####}; при сумме > 1 сумма макс. баллов должна быть 100 (сейчас {maxSum:0.##})";
            yield break;
        }

        if (level == NodeLevelType.Component)
        {
            yield return
                $"Сумма весов компонентов: {weightSum:0.####} (ожидается 1; если больше 1 — сумма макс. баллов должна быть 100)";
            yield break;
        }

        yield return $"Сумма весов на уровне: {weightSum:0.####} (ожидается 1)";
    }

    public static string LevelLabel(NodeLevelType level, CertificationKind? cert) => level switch
    {
        NodeLevelType.Subject => "Предмет",
        NodeLevelType.LessonType => "Тип занятия",
        NodeLevelType.Certification => cert == CertificationKind.Intermediate
            ? "Промежуточная аттестация" : "Текущая аттестация",
        NodeLevelType.Component => "Компонент",
        _ => level.ToString()
    };

    public static IEnumerable<SubjectNodeDto> ChildrenOf(
        IReadOnlyList<SubjectNodeDto> nodes, Guid? parentId) =>
        nodes.Where(n => n.ParentId == parentId).OrderBy(n => n.SortOrder).ThenBy(n => n.Path);

    public static decimal DefaultCoefficient(IReadOnlyList<SubjectNodeDto> nodes, Guid? parentId)
    {
        var siblings = nodes.Count(n => n.ParentId == parentId);
        return siblings == 0 ? 1m : Math.Round(1m / (siblings + 1), 4);
    }
}
