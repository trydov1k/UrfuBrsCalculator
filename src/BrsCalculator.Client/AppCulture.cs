using System.Globalization;
using BrsCalculator.Domain.Brs;

namespace BrsCalculator.Client;

public static class AppCulture
{
    public static CultureInfo Russian { get; } = CultureInfo.GetCultureInfo("ru-RU");

    public static string FormatScore(decimal value) =>
        value.ToString(ScorePrecision.DisplayFormat, Russian);

    public static decimal RoundScore(decimal value) => ScorePrecision.Round(value);

    public static decimal? RoundScore(decimal? value) => ScorePrecision.Round(value);
}
