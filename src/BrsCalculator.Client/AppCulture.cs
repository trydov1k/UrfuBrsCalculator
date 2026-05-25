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

    public static string FormatScoreInput(decimal? value) =>
        value.HasValue ? FormatScore(value.Value) : string.Empty;

    /// <summary>
    /// Parses user input; returns false while the text ends with a decimal separator (incomplete fraction).
    /// </summary>
    public static bool TryParseScoreInput(string? text, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
            return true;

        text = text.Trim();
        if (text.EndsWith(',') || text.EndsWith('.'))
            return false;

        var normalized = text.Replace(" ", "").Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            return false;

        value = RoundScore(parsed);
        return true;
    }
}
