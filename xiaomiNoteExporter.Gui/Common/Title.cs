using System;
using System.Reflection;

namespace xiaomiNoteExporter.Gui.Common
{
    public sealed class Title
    {
        private static readonly Version? CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;

        private static readonly string VersionString = $"{CurrentVersion?.Major}.{CurrentVersion?.Minor}.{CurrentVersion?.Build}";

        public string Value => $"Xiaomi Note Exporter {VersionString}";
    }
}
