using System.Drawing;
using Pastel;

using xiaomiNoteExporter.Extensions;

namespace xiaomiNoteExporter;

internal class ConsoleHelp(Version? version)
{
    private readonly List<string> messages = new ()
    {
        $"{"Xiaomi Note Exporter".Pastel(Color.FromArgb(252, 106, 0))} {version.GetVersionString()}\n",
        $"Usage: xiaomiNoteExporter.exe {"[options]".Pastel(Color.DimGray)}\n",
        "Options:",
        "  -h, --help\t\tShow this help message and exit\n",
        $"  -d, --domain <domain> {"(default: us.i.mi.com)".Pastel(Color.DimGray)}\n\tMi Notes domain that you were redirected to.\n",
        $"  -s, --split <timestamp> {"(default: dd-MM-yyyy_HH-mm-ss)".Pastel(Color.DimGray)}\n\tSplit notes into separate files with provided timestamp format. Must be compatible with:\n\thttps://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings"
    };

    public void Print()
    {
        foreach (var item in messages)
        {
            Console.WriteLine(item);
        }
    }
}
