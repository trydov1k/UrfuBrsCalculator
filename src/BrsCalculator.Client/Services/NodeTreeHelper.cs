using BrsCalculator.Application.DTOs;
using BrsCalculator.Domain.Brs;
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

    public static bool CanDelete(SubjectNodeDto node) =>
        SubjectNodeRules.CanDelete(node.LevelType);

    public static bool IsCoefficientOnlyEdit(SubjectNodeDto node) =>
        SubjectNodeRules.IsCoefficientOnlyEdit(node.LevelType);

    public static bool CanMarkExam(SubjectNodeDto parent) =>
        parent.LevelType == NodeLevelType.Certification &&
        parent.CertificationKind == CertificationKind.Intermediate;

    private const decimal MaxScoreTarget = 100m;
    private const decimal MaxScoreTolerance = 0.01m;
    private const decimal LeafWeightTarget = 1m;
    private const decimal LeafWeightTolerance = 0.0001m;

    public static IEnumerable<string> GetStructureWarnings(
        IReadOnlyList<SubjectNodeDto> nodes, Guid? parentId)
    {
        if (!parentId.HasValue)
            yield break;

        var siblings = nodes.Where(n => n.ParentId == parentId).ToList();
        if (siblings.Count == 0)
            yield break;

        var weightedSum = siblings.Sum(n => n.MaxScore * n.Coefficient);
        if (Math.Abs(weightedSum - MaxScoreTarget) > MaxScoreTolerance)
            yield return $"Сумма (макс. × вес): {AppCulture.FormatScore(weightedSum)} (ожидается {AppCulture.FormatScore(MaxScoreTarget)})";

        var wrongWeightLeaves = siblings
            .Where(n => n.IsLeaf && Math.Abs(n.Coefficient - LeafWeightTarget) > LeafWeightTolerance)
            .ToList();

        if (wrongWeightLeaves.Count > 0)
        {
            var details = string.Join(", ", wrongWeightLeaves.Select(n =>
                $"«{n.Name}» ({n.Coefficient.ToString("0.####", AppCulture.Russian)})"));
            yield return $"Конечные компоненты должны иметь вес 1: {details}";
        }
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
