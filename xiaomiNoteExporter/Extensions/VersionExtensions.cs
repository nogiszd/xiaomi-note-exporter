namespace xiaomiNoteExporter.Extensions;

public static class VersionExtensions
{
    public static string GetString(this Version? version)
    {
        if (version == null)
        {
            return "0.0.0";
        }

        return $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
