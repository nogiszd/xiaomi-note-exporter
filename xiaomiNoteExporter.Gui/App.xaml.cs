using System.Windows;
using xiaomiNoteExporter.Gui.Common;

namespace xiaomiNoteExporter.Gui
{
    public partial class App : Application
    {
        public string Title { get; private set; } = string.Empty;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Title = new Title().Value;
        }
    }
}
