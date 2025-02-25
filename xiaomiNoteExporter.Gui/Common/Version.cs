using System.Reflection;
using ExecutableVersion = System.Version;

namespace xiaomiNoteExporter.Gui.Common;

public sealed class Version
{
    public readonly ExecutableVersion? Current = Assembly.GetExecutingAssembly().GetName().Version;
}
