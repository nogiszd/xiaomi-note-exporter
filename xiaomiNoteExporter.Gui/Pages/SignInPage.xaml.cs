using System;
using System.Windows;
using System.Windows.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using xiaomiNoteExporter.Gui.Extensions;

namespace xiaomiNoteExporter.Gui.Pages
{
    public partial class SignInPage : Page
    {
        private readonly ChromeDriver Driver;

        private string Domain { get; set; }

        public SignInPage(string domain, ChromeDriver driver)
        {
            InitializeComponent();

            Driver = driver;

            Domain = domain;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            NavigateToDomain();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var wait = Driver.GetWait(TimeSpan.FromSeconds(10));

            try
            {
                if (wait.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'ant-tabs')]"))).Count != 0)
                {
                    MessageBox.Show(
                        $"You didn't sign into Mi Cloud or account is invalid.\nPlease try again.", 
                        "Oops", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning
                        );
                    NavigateToDomain();
                }
            } 
            catch (Exception ex) 
            {
                MessageBox.Show(
                    $"An error occurred. More details below:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                    );
                Window.GetWindow(this).Close();
            }
        }

        private void NavigateToDomain()
        {
            Driver.Navigate().GoToUrl($"https://{Domain}/note/h5#");
        }
    }
}
