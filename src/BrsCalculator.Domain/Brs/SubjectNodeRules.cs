using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Domain.Brs;

public static class SubjectNodeRules
{
    public static bool CanDelete(NodeLevelType level) =>
        level == NodeLevelType.Component;

    public static bool IsCoefficientOnlyEdit(NodeLevelType level) =>
        level is NodeLevelType.LessonType or NodeLevelType.Certification;
}
