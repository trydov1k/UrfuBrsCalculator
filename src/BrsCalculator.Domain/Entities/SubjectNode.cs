using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Domain.Entities;

public class SubjectNode
{
    public Guid Id { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? ParentId { get; set; }
    /// <summary>Материализованный путь для сортировки и иерархии (например "0001.0002").</summary>
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public NodeLevelType LevelType { get; set; }
    public CertificationKind? CertificationKind { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Coefficient { get; set; }
    public bool IsExam { get; set; }
    public decimal? ActualScore { get; set; }
    public int SortOrder { get; set; }

    public Subject Subject { get; set; } = null!;
    public SubjectNode? Parent { get; set; }
    public ICollection<SubjectNode> Children { get; set; } = new List<SubjectNode>();
}
