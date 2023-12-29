using OpenQA.Selenium.Chrome;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using xiaomiNoteExporter.Gui.Services;

namespace xiaomiNoteExporter.Gui.Pages
{
    public partial class ParsePage : Page, INotifyPropertyChanged
    {
        private readonly ScrapeService _scrapeService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private int _current;
        public int Current
        {
            get { return _current; }
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnPropertyChanged(nameof(Current));
                }
            }
        }

        private int _total;
        public int Total
        {
            get { return _total; }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public string ProgressText => $"{Current}/{Total}";

        public ParsePage(ChromeDriver driver)
        {
            InitializeComponent();

            _scrapeService = new(driver);

            DataContext = this;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _scrapeService.Start();

            Current = _scrapeService.CurrentNote;
            Total = _scrapeService.NotesAmount;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_scrapeService.IsRunning)
            {
                await _scrapeService.Stop();
            }
        }
    }
}
