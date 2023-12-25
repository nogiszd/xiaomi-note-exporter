using OpenQA.Selenium.Chrome;
using System.Windows;
using System.Windows.Controls;

namespace xiaomiNoteExporter.Gui.Pages
{
    public partial class ParsePage : Page
    {
        private readonly ChromeDriver Driver;

        public ParsePage(ChromeDriver driver)
        {
            InitializeComponent();

            Driver = driver;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
