namespace xiaomiNoteExporter.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Determines whether the specified string contains any of the provided values.
    /// </summary>
    /// <param name="str">The string to search within. Cannot be null.</param>
    /// <param name="values">An array of strings to search for within the source string. Cannot be null or contain null elements.</param>
    /// <returns><c>true</c> if the source string contains at least one of the specified values; otherwise, <c>false</c>.</returns>
    public static bool Includes(this string str, params string[] values)
    {
        return values.Any(str.Contains);
    }

    /// <summary>
    /// Determines whether the specified string exactly matches any of the provided values.
    /// </summary>
    /// <param name="str">The string to search within. Cannot be null.</param>
    /// <param name="values">An array of strings to search for within the source string. Cannot be null or contain null elements.</param>
    /// <returns><c>true</c> if the source string contains at least one of the specified values; otherwise, <c>false</c>.</returns>
    public static bool IncludesExact(this string str, params string[] values)
    {
        return values.Any(v => str.Equals(v, StringComparison.InvariantCulture));
    }

    /// <summary>
    /// Determines whether the specified string contains any of the provided values, using a case-insensitive comparison
    /// based on the invariant culture.
    /// </summary>
    /// <param name="str">The string to search within.</param>
    /// <param name="values">An array of strings to search for within the source string. Each value is compared using a case-insensitive,
    /// culture-invariant comparison.</param>
    /// <returns><c>true</c> if the source string contains at least one of the specified values; otherwise, <c>false</c>.</returns>
    public static bool IncludesIgnoreCase(this string str, params string[] values)
    {
        return values.Any(v => str.Contains(v, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Replaces all newline characters in the string with Markdown-compatible line breaks.
    /// </summary>
    /// <param name="str">The input string in which newline characters will be escaped. Can be null or empty.</param>
    /// <returns>A string where each newline character is replaced with two spaces followed by a newline, compatible for Markdown line breaks.</returns>
    public static string EscapeNewLine(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        str = str.Replace("\r\n", "\n");

        return str.Replace("\n", "  \n");
    }
}
