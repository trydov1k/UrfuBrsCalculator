using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Domain.Entities;

public class Semester
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AcademicYearStart { get; set; }
    public SemesterSeason Season { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
