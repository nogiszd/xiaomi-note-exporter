using System.Globalization;
using System.Text.RegularExpressions;

namespace xiaomiNoteExporter;

public static class SimplifiedDateParser
{
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
