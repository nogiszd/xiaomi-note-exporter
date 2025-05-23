﻿using System.Text.RegularExpressions;

namespace xiaomiNoteExporter;

public static class RelativeTimeParser
{
    private static readonly Dictionary<string, Func<int, TimeSpan>> _units = new()
    {
        { "second", v => TimeSpan.FromSeconds(v) },
        { "seconds", v => TimeSpan.FromSeconds(v) },
        { "minute", v => TimeSpan.FromMinutes(v) },
        { "minutes", v => TimeSpan.FromMinutes(v) },
        { "hour", v => TimeSpan.FromHours(v) },
        { "hours", v => TimeSpan.FromHours(v) },
        { "day", v => TimeSpan.FromDays(v) },
        { "days", v => TimeSpan.FromDays(v) },
        { "week", v => TimeSpan.FromDays(v * 7) },
        { "weeks", v => TimeSpan.FromDays(v * 7) },
        { "month", v => TimeSpan.FromDays(v * 30) },
        { "months", v => TimeSpan.FromDays(v * 30) },
        { "year", v => TimeSpan.FromDays(v * 365) },
        { "years", v => TimeSpan.FromDays(v * 365) },
    };

    /// <summary>
    /// Parses a relative time string (e.g., "5 minutes ago") into a DateTime object.
    /// </summary>
    /// <param name="input">Input string that contains relative time info.</param>
    /// <returns><c>DateTime</c> object calculated from relative time description.</returns>
    /// <exception cref="ArgumentException">Thrown when input is empty.</exception>
    /// <exception cref="FormatException">Thrown when RegEx couldn't be matched to input string.</exception>
    /// <exception cref="NotSupportedException">Thrown when found unit of time is not in supported range.</exception>
    public static DateTime Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input date string cannot be null or empty.", nameof(input));
        }

        input = input.Trim().ToLowerInvariant();

        var match = Regex.Match(input, @"(\d+)\s+(\w+)\s+ago");
        
        if (!match.Success)
        {
            throw new FormatException("Could not parse the date string.");
        }

        int value = int.Parse(match.Groups[1].Value);
        string unit = match.Groups[2].Value;

        if (_units.TryGetValue(unit, out var toTimeSpan))
        {
            return DateTime.Now - toTimeSpan(value);
        }

        throw new NotSupportedException($"Unknown time unit: {unit}");
    }
}
