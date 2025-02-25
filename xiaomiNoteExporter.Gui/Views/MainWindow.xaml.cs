using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using OpenQA.Selenium.Chrome;

using xiaomiNoteExporter.Gui.Pages;
using xiaomiNoteExporter.Gui.Services;

namespace xiaomiNoteExporter.Gui;

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

        StatusbarService.Initialize();

        driver = _driver.Prepare();

        WindowTitle = ((App)Application.Current).Title;

        Domain = domain;

        worker = new BrowserWorker(driver, this);

        DataContext = this;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CheckIfExportsExist();
        MainFrame.Navigate(new SignInPage(Domain, driver, worker));
    }

    private void CheckIfExportsExist()
    {
        CanContinue = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "notes_export*.xml").Any();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            worker.Stop();
        } 
        catch
        {
            // ignore this exception, because we need to revoke the worker CancellationToken anyways
        }

        try
        {
            driver.Close();
            driver.Quit();
        }
        catch
        {
            // apparently driver is not longer running
            // try to kill chromedriver.exe manually if exist
            Process.GetProcessesByName("chromedriver").ToList().ForEach(p => p.Kill());
        }

        Application.Current.Shutdown();
    }
}
