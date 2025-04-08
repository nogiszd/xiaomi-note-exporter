using System.Reflection;
using System.Text.RegularExpressions;

namespace xiaomiNoteExporter.Extensions;

public static class VersionExtensions
{
    /// <summary>
    /// Get the version string in the format <c>Major.Minor.Build</c> or <c>Major.Minor.Build-Suffix</c>
    /// </summary>
    /// <param name="version">`Version` object to be parsed.</param>
    /// <returns>Formatted version string.</returns>
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
