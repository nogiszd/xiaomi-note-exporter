using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using xiaomiNoteExporter.Gui.Events;
using xiaomiNoteExporter.Gui.Extensions;
using xiaomiNoteExporter.Gui.Services;

namespace xiaomiNoteExporter.Gui.Pages
{
    public partial class SignInPage : Page
    {
        private readonly ChromeDriver Driver;

        private readonly SignedInEvent signedInEvent = new();

        private readonly BrowserWorker _worker;

        private string Domain { get; set; }

        public SignInPage(string domain, ChromeDriver driver, BrowserWorker worker)
        {
            InitializeComponent();

            Driver = driver;

            Domain = domain;

            _worker = worker;

            signedInEvent.Handler += SignedIn_Handler;
        }

        private void SignedIn_Handler(object sender, EventArgs e)
        {
            NavigationService.Navigate(new ParsePage(Driver));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StatusbarService.SetStatus("Ready and waiting");
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            NavigateToDomain();
            await _worker.Start();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var wait = Driver.GetWait(TimeSpan.FromSeconds(10));

            try
            {
                if (await Task.Run(() => wait.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'ant-tabs')]"))).Count != 0))
                {
                    MessageBox.Show(
                        $"You didn't sign into Mi Cloud or account is invalid.\nPlease try again.", 
                        "Oops", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning
                        );
                    NavigateToDomain();
                } 
                else if (await Task.Run(() => wait.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'identity-verifyPhone-')]"))).Count != 0))
                {
                    MessageBox.Show(
                        $"You need to complete 2FA verification.",
                        "Oops",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                        );
                } 
                else
                {
                    await signedInEvent.Raise();
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
