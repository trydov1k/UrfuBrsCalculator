using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Application.DTOs;

public record SemesterDto(Guid Id, string Name, int AcademicYearStart, SemesterSeason Season, int SubjectCount);
public record CreateSemesterRequest(string Name, int AcademicYearStart, SemesterSeason Season);
public record UpdateSemesterRequest(string Name, int AcademicYearStart, SemesterSeason Season);
