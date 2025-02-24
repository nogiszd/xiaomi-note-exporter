using System;
using System.Linq;

namespace xiaomiNoteExporter.Gui.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Check if passed <c>string</c> is present in the array of string values.
    /// </summary>
    /// <param name="source">Value to be searched.</param>
    /// <param name="values">Collection of values to search within.</param>
    /// <returns>A <c>bool</c> representing truthfulness of presence.</returns>
    public static bool Includes(this string? source, params string[] values)
    {
        return values.Contains(source);
    }
}
