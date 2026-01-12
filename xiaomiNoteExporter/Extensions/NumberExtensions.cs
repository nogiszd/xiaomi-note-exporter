namespace xiaomiNoteExporter.Extensions;

public static class NumberExtensions
{
    /// <summary>
    /// Calculates the percentage value of <paramref name="numerator"/> relative to
    /// <paramref name="denominator"/>, rounded to the nearest whole number.
    /// </summary>
    /// <param name="numerator">The part value used as the basis for the percentage calculation.</param>
    /// <param name="denominator">The total value against which the percentage is calculated.</param>
    /// <returns>An integer representing the calculated percentage.</returns>
    public static int GetPercentage(this int numerator, int denominator)
    {
        return (int)(.5f + 100f * numerator / denominator);
    }
}
