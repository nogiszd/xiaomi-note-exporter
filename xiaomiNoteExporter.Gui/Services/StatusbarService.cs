using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace xiaomiNoteExporter.Gui.Services;

public static class StatusbarService
{
    private static StatusBar? statusBar;

    private static readonly string WindowName = "GuiWindow";

    public static void Initialize()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Window? window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.Name == WindowName && w.IsActive);

            if (window is not null)
            {
                statusBar = window.FindName("Statusbar") as StatusBar;
            }
        });
    }

    public static void SetStatus(string status)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (statusBar is null)
            {
                Initialize();
            }

            if (statusBar is not null)
            {
                statusBar.Items.Clear();
                statusBar.Items.Add(new StatusBarItem() { Content = status });
            }
        }, DispatcherPriority.Normal);
    }
}
