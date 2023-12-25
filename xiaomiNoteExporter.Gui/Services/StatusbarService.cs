using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace xiaomiNoteExporter.Gui.Services
{
    public class StatusbarService
    {
        private static StatusBar? statusBar;

        private static string WindowName { get; } = "GuiWindow";

        private static void FindStatusBar(string name)
        {
            Window? window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.Name == name);

            if (window is not null)
            {
                statusBar = window.FindName("Statusbar") as StatusBar;
            }
        }

        public static void SetStatus(string status)
        {
            FindStatusBar(WindowName);

            if (statusBar is not null)
            {
                statusBar.Items.Clear();
                statusBar.Items.Add(new StatusBarItem() { Content = status });
            }
        }
    }
}
