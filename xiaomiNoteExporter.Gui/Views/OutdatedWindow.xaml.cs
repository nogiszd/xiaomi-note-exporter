using System.Windows;
using System.Diagnostics;

namespace xiaomiNoteExporter.Gui.Views
{
    public partial class OutdatedWindow : Window
    {
        public string WindowTitle { get; private set; }

        public string NewerVersion { get; set; }

        public OutdatedWindow(string newerVersion)
        {
            InitializeComponent();

            WindowTitle = ((App)Application.Current).Title;

            NewerVersion = newerVersion;

            DataContext = this;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(Properties.Settings.Default.repo) { UseShellExecute = true });
            Application.Current.Shutdown();
        }
    }
}
