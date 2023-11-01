using System.Windows;
using System.Windows.Controls;

namespace xiaomiNoteExporter.Gui.Views
{
    public partial class InitializeWindow : Window
    {
        public string WindowTitle { get; private set; }

        public string DomainUrl { get; set; } = "us.i.mi.com";

        public InitializeWindow()
        {
            InitializeComponent();

            WindowTitle = ((App)Application.Current).Title;

            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            return;
        }
    }
}
