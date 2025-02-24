namespace xiaomiNoteExporter.Gui.Extensions;

public static class NumberExtensions
{
    /// <summary>
    /// Get percentage value of two numbers.
    /// </summary>
    /// <param name="number">Numerator in percentage equasion.</param>
    /// <param name="divisor">Denominator in percentage equasion.</param>
    /// <returns>Percentage value calculated from between two numbers.</returns>
    public static int GetPercentage(this int num, int den)
    {
        return (int)(.5f + 100f * num / den);
    }
}
