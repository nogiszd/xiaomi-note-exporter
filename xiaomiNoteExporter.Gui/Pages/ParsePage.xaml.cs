using OpenQA.Selenium.Chrome;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using xiaomiNoteExporter.Gui.Extensions;
using xiaomiNoteExporter.Gui.Services;

namespace xiaomiNoteExporter.Gui.Pages;

public partial class ParsePage : Page, INotifyPropertyChanged
{
    private readonly ScrapeService _scrapeService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Current => _scrapeService.CurrentNote;

    public int Total => _scrapeService.NotesAmount;

    public string ProgressText => $"{Current}/{Total}";

    public bool IsRunning => _scrapeService.IsRunning;

    public ParsePage(ChromeDriver driver)
    {
        InitializeComponent();

        _scrapeService = new(driver);

        _scrapeService.PropertyChanged += ScrapeService_PropertyChanged;

        DataContext = this;
    }

    private void ScrapeService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName.Includes(nameof(ScrapeService.CurrentNote), nameof(ScrapeService.NotesAmount), nameof(ScrapeService.IsRunning)))
        {
            OnPropertyChanged(nameof(Current));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await _scrapeService.Start();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        _scrapeService.PropertyChanged -= ScrapeService_PropertyChanged;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (_scrapeService.IsRunning)
        {
            await _scrapeService.Stop();
        }

        StopButton.IsEnabled = false;
    }
}
