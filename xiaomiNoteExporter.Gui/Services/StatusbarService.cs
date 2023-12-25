using System.Windows;
using System.Windows.Controls.Primitives;

namespace xiaomiNoteExporter.Gui.Services
{
    public static class StatusbarService
    {
        private readonly static StatusBar? statusBar = ((App)Application.Current).MainWindow.FindName("Statusbar") as StatusBar;

        public static void SetStatus(string status)
        {
            if (statusBar is not null)
            {
                statusBar.Items.Clear();
                statusBar.Items.Add(new StatusBarItem() { Content = status });
            }
        }
    }
}
