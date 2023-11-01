using System.Linq;
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

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            MainWindow mainWindow = new MainWindow(DomainUrl);

            mainWindow.Show();
        }

        private void Domain_TextChanged(object sender, RoutedEventArgs e)
        {
            var validationResult = Validation.GetErrors(Domain);

            if (validationResult.Any())
            {
                SubmitButton.IsEnabled = false;
                ErrorLabel.Text = validationResult.First().ErrorContent.ToString();
            }
            else
            {
                SubmitButton.IsEnabled = true;
                ErrorLabel.Text = string.Empty;
            }
        }
    }
}
