namespace BrsCalculator.Domain.Brs;

public static class ScorePrecision
{
    public const int DecimalPlaces = 2;
    public const string DisplayFormat = "0.00";

    public static decimal Round(decimal value) =>
        Math.Round(value, DecimalPlaces, MidpointRounding.AwayFromZero);

    public static decimal? Round(decimal? value) =>
        value is { } v ? Round(v) : null;
}
