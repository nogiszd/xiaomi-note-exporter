namespace xiaomiNoteExporter.Extensions;

public static class StringExtensions
{
    public static bool Includes(this string str, params string[] values)
    {
        return values.Any(str.Contains);
    }

    public static bool IncludesIgnoreCase(this string str, params string[] values)
    {
        return values.Any(v => str.Contains(v, StringComparison.InvariantCultureIgnoreCase));
    }
}
