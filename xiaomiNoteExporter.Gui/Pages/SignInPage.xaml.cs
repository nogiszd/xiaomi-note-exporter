using OpenQA.Selenium.Chrome;
using System;
using System.Windows.Controls;

namespace xiaomiNoteExporter.Gui.Pages
{
    public partial class SignInPage : Page
    {
        private readonly ChromeDriver _driver;

        private string _domain { get; set; }

        public SignInPage(string domain, ChromeDriver driver)
        {
            InitializeComponent();

            _driver = driver;

            _domain = domain;
        }

        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _driver.Navigate().GoToUrl($"https://{_domain}/note/h5#");
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }
    }
}
