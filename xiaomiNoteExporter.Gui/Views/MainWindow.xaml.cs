using System.Windows;

namespace xiaomiNoteExporter.Gui
{
    public partial class MainWindow : Window
    {
        public string WindowTitle { get; private set; }

        private string Domain { get; set; }

        public MainWindow(string domain)
        {
            InitializeComponent();

            WindowTitle = ((App)Application.Current).Title;

            Domain = domain;

            DataContext = this;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
