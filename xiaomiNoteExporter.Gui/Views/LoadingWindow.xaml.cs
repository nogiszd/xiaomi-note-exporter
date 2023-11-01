using System.Windows;

namespace xiaomiNoteExporter.Gui.Views
{
    public partial class LoadingWindow : Window
    {
        public string WindowTitle { get; private set; }

        public LoadingWindow()
        {
            InitializeComponent();

            WindowTitle = ((App)Application.Current).Title;

            DataContext = this;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Window_Initialized(object sender, System.EventArgs e)
        {
            UpdateCheck updateCheck = new();
            var (version, isNewer) = await updateCheck.Check();

            Hide();

            if (isNewer)
            {
                OutdatedWindow outdatedWindow = new(version);
                outdatedWindow.Show();

            } else
            {
                InitializeWindow initializeWindow = new();
                initializeWindow.Show();
            }
        }
    }
}
