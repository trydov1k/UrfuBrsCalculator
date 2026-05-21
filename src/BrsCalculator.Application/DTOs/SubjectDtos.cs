using BrsCalculator.Domain.Enums;

namespace BrsCalculator.Application.DTOs;

public record SubjectListItemDto(
    Guid Id,
    string Name,
    decimal TotalScore,
    string Grade,
    bool IsPassed);

public record SubjectDetailDto(
    Guid Id,
    string Name,
    IReadOnlyList<SubjectNodeDto> Nodes,
    decimal TotalScore,
    string Grade,
    bool IsPassed,
    string? FailureReason);

public record SubjectNodeDto(
    Guid Id,
    Guid? ParentId,
    string Path,
    string Name,
    NodeLevelType LevelType,
    CertificationKind? CertificationKind,
    decimal MaxScore,
    decimal Coefficient,
    bool IsExam,
    decimal? ActualScore,
    decimal ComputedScore,
    bool IsLeaf,
    int SortOrder);

public record CreateSubjectRequest(string Name, IReadOnlyList<string> LessonTypes);
public record UpdateSubjectRequest(string Name);
public record CreateNodeRequest(
    Guid? ParentId,
    string Name,
    NodeLevelType LevelType,
    CertificationKind? CertificationKind,
    decimal MaxScore,
    decimal Coefficient,
    bool IsExam);
public record UpdateNodeRequest(
    string Name,
    decimal MaxScore,
    decimal Coefficient,
    bool IsExam);
public record UpdateScoreRequest(decimal? ActualScore);
public record WhatIfRequest(IReadOnlyDictionary<Guid, decimal?> Overrides, string TargetGrade);
public record WhatIfResultDto(
    decimal CurrentTotal,
    decimal TargetTotal,
    decimal Shortfall,
    IReadOnlyDictionary<Guid, decimal> SuggestedScores,
    string ProjectedGrade);
