using System.Globalization;
using System.Text.RegularExpressions;

namespace xiaomiNoteExporter;

public static class SimplifiedDateParser
{
    /// <summary>
    /// Attempts to parse a string representing a month, day, hour, and minute in the format "M/d HH:mm" into a <see
    /// cref="DateTime"/> value using the current year.
    /// </summary>
    /// <param name="input">The input string to parse, expected in the format "M/d HH:mm" where M is the month (1-12), d is the day (1-31),
    /// HH is the hour (0-23), and mm is the minute (0-59).</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="DateTime"/> value if the parse operation succeeds;
    /// otherwise, contains <see cref="DateTime.MinValue"/>.</param>
    /// <returns><c>true</c> if the input string was successfully parsed; otherwise, <c>false</c>.</returns>
    public static bool TryParseMdHm(string input, out DateTime result)
    {
        result = default;

        var regex = new Regex(@"^(0?[1-9]|1[0-2])/(0?[1-9]|[12][0-9]|3[01]) ([01]?[0-9]|2[0-3]):([0-5][0-9])$");

        if (!regex.IsMatch(input))
        {
            return false;
        }

        var match = regex.Match(input);

        int month = int.Parse(match.Groups[1].Value);
        int day = int.Parse(match.Groups[2].Value);
        string time = $"{match.Groups[3].Value}:{match.Groups[4].Value}";

        int year = DateTime.Now.Year;
        string formatted = $"{year}-{month:D2}-{day:D2} {time}";

        return DateTime.TryParse(formatted, new CultureInfo("en-US"), out result);
    }
}