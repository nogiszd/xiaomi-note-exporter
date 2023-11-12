using System;
using System.Windows;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using xiaomiNoteExporter.Gui.Pages;

namespace xiaomiNoteExporter.Gui
{
    public partial class MainWindow : Window
    {
        public string WindowTitle { get; private set; }

        public bool CanContinue { get; private set; } = false;

        private string Domain { get; set; }

        static readonly Driver _driver = new(Array.Empty<string>());

        public readonly ChromeDriver driver;

        public MainWindow(string domain)
        {
            InitializeComponent();

            driver = _driver.Prepare();

            WindowTitle = ((App)Application.Current).Title;

            Domain = domain;

            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SignInPage(Domain, driver));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                driver.Close();
                driver.Quit();
            } 
            catch (Exception)
            {
                // apparently driver is not longer running - ignore it
            }

            Application.Current.Shutdown();
        }
    }
}
