namespace BrsCalculator.Domain.Brs;

public sealed record GradeResult(
    decimal TotalScore,
    string GradeLabel,
    bool IsPassed,
    bool? ExamPassed,
    decimal? ExamScore,
    string? FailureReason);
