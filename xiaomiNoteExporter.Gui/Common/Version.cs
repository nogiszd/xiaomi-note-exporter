using ExecutableVersion = System.Version;
using System.Reflection;

namespace xiaomiNoteExporter.Gui.Common;

public sealed class Version
{
    public readonly ExecutableVersion? Current = Assembly.GetExecutingAssembly().GetName().Version;
}
