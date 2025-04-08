using System.Reflection;
using System.Text.RegularExpressions;

namespace xiaomiNoteExporter.Extensions;

public static class VersionExtensions
{
    public static string GetVersionString(this Version? version)
    {
        if (version is null)
        {
            return "0.0.0";
        }

        var suffix = GetVersionSuffix();

        if (string.IsNullOrWhiteSpace(suffix))
        {
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        return $"{version.Major}.{version.Minor}.{version.Build}-{suffix}";
    }

    private static string GetVersionSuffix()
    {
        var infoVersion = Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";

        var match = Regex.Match(infoVersion, @"-(?<tag>[a-zA-Z]+)");

        return match.Success ? match.Groups["tag"].Value : string.Empty;
    }
}
