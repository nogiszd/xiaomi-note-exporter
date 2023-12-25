using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using OpenQA.Selenium.Chrome;
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

        private readonly BrowserWorker worker;

        public MainWindow(string domain)
        {
            InitializeComponent();

            driver = _driver.Prepare();

            WindowTitle = ((App)Application.Current).Title;

            Domain = domain;

            worker = new BrowserWorker(driver, this);

            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SignInPage(Domain, driver, worker));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                worker.Stop();
            } 
            catch (Exception)
            {
                // ignore this exception
            }

            try
            {
                driver.Close();
                driver.Quit();
            }
            catch (Exception)
            {
                // apparently driver is not longer running
                // try to kill chromedriver.exe manually if exist
                Process.GetProcessesByName("chromedriver").ToList().ForEach(p => p.Kill());
            }

            Application.Current.Shutdown();
        }
    }
}
