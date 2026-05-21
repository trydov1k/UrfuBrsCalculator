namespace BrsCalculator.Domain.Entities;

public class Subject
{
    public Guid Id { get; set; }
    public Guid SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Semester Semester { get; set; } = null!;
    public ICollection<SubjectNode> Nodes { get; set; } = new List<SubjectNode>();
}
