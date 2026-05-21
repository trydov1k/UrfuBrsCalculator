using BrsCalculator.Domain.Entities;
using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Application.Services;

public class SubjectTemplateService
{
    public IReadOnlyList<SubjectNode> BuildTemplate(Guid subjectId, string subjectName, IReadOnlyList<string> lessonTypes)
    {
        var nodes = new List<SubjectNode>();
        var root = CreateNode(subjectId, null, "0001", subjectName, NodeLevelType.Subject, null, 100, 1, false, 0);
        nodes.Add(root);

        if (lessonTypes.Count == 0)
            lessonTypes = ["Лекции"];

        var weight = 1m / lessonTypes.Count;
        for (var i = 0; i < lessonTypes.Count; i++)
        {
            var ltPath = $"0001.{(i + 1):D4}";
            var lessonType = CreateNode(subjectId, root.Id, ltPath, lessonTypes[i], NodeLevelType.LessonType, null, 100, weight, false, i);
            nodes.Add(lessonType);

            var current = CreateNode(subjectId, lessonType.Id, $"{ltPath}.0001", "Текущая аттестация",
                NodeLevelType.Certification, CertificationKind.Current, 100, 0.5m, false, 0);
            nodes.Add(current);

            var intermediate = CreateNode(subjectId, lessonType.Id, $"{ltPath}.0002", "Промежуточная аттестация",
                NodeLevelType.Certification, CertificationKind.Intermediate, 100, 0.5m, false, 1);
            nodes.Add(intermediate);
        }

        return nodes;
    }

    private static SubjectNode CreateNode(
        Guid subjectId, Guid? parentId, string path, string name,
        NodeLevelType level, CertificationKind? certKind,
        decimal maxScore, decimal coefficient, bool isExam, int sortOrder) => new()
    {
        Id = Guid.NewGuid(),
        SubjectId = subjectId,
        ParentId = parentId,
        Path = path,
        Name = name,
        LevelType = level,
        CertificationKind = certKind,
        MaxScore = maxScore,
        Coefficient = coefficient,
        IsExam = isExam,
        SortOrder = sortOrder
    };
}
