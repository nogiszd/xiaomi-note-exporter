namespace xiaomiNoteExporter.Extensions;

public static class NumberExtensions
{
    public static int GetPercentage(this int numerator, int denominator)
    {
        return (int)(.5f + 100f * numerator / denominator);
    }
}
