namespace xiaomiNoteExporter.Gui.Common
{
    public sealed class Title
    {
        private static readonly Version CurrentVersion = new();

        private static readonly string VersionString = $"{CurrentVersion.Current?.Major}.{CurrentVersion.Current?.Minor}.{CurrentVersion.Current?.Build}";

        public string Value => $"Xiaomi Note Exporter {VersionString}";
    }
}
