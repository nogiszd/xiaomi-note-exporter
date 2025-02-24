using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

using xiaomiNoteExporter.Gui.Entities;
using xiaomiNoteExporter.Gui.Extensions;

namespace xiaomiNoteExporter.Gui.Services;

public partial class ScrapeService : INotifyPropertyChanged
{
    private readonly ChromeDriver _driver;
    public readonly Stopwatch _watch;

    private int _notesAmount;
    public int NotesAmount
    {
        get => _notesAmount;
        private set
        {
            if (_notesAmount != value)
            {
                _notesAmount = value;
                OnPropertyChanged(nameof(NotesAmount));
            }
        }
    }

    private int _currentNote;
    public int CurrentNote
    {
        get => _currentNote;
        private set
        {
            if (_currentNote != value)
            {
                _currentNote = value;
                OnPropertyChanged(nameof(CurrentNote));
            }
        }
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ScrapeService(ChromeDriver driver)
    {
        _driver = driver;
        _watch = new();
    }

    public async Task Start()
    {
        await Task.Run(Scrape);   
    }

    private void Scrape()
    {
        var span = _driver.GetWait(TimeSpan.FromMilliseconds(10));
        var wait = _driver.GetWait(TimeSpan.FromSeconds(10));

        InitializeTimer();

        string notesAmountEl = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'note-count-select')]"))).Text;
        NotesAmount = int.Parse(DigitRegex().Replace(notesAmountEl, ""));

        IWebElement noteList = wait.Until(e => e.FindElement(By.XPath("//div[contains(@class, 'note-list-items')]")));

        string fileName = $"{DateTime.Now:dd-MM-yy_HH-mm-ss}";
        XmlDocument doc = XmlExtensions.Initialize("notes");

        CurrentNote = 0;
        bool isFirst = true; // check is needed because it usually opens first note automatically
        IsRunning = true;

        while (IsRunning)
        {
            StatusbarService.SetStatus(
                $"Parsing: {CurrentNote}/{NotesAmount} ({CurrentNote.GetPercentage(NotesAmount)}%)"
                );

            if (CurrentNote == NotesAmount)
            {
                _watch.Stop();
                break;
            }
            else
            {
                IWebElement el;
                if (!isFirst)
                {
                    el = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'open')]/following::div")));
                }
                else
                {
                    el = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'open')]")));
                    isFirst = false;
                }

                el.Click();
                Thread.Sleep(200); // timeout for fetching optimization

                string createdAt = el.FindElement(By.XPath(@".//div[2]/div[1]")).Text;

                try
                {
                    span.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'open')]/div[2][not(./i)]")).Count == 1);
                }
                catch
                {
                    Insert(doc, Note.Create(null, null, createdAt, NoteType.Unsupported));

                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollBy(0, arguments[1]);", noteList, el.Size.Height);
                    UpdateCurrentNote();
                    continue;
                }

                wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'origin-title')]/div")).Displayed);

                string title = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'origin-title')]/div"))).Text;
                string value = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'pm-container')]"))).Text;

                Insert(doc, Note.Create(title, value, createdAt, NoteType.Normal));

                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollBy(0, arguments[1]);", noteList, el.Size.Height);
                UpdateCurrentNote();
            }
        }

        doc.Save($"{fileName}.xml");

        StatusbarService.SetStatus(
            string.Format(
                "Done in {0:00}:{0:00}:{0:00}",
                _watch.Elapsed.Hours, _watch.Elapsed.Minutes, _watch.Elapsed.Seconds
                )
            );

        IsRunning = false;
    }

    public async Task Stop()
    {
        await Task.Run(() => IsRunning = false);
    }

    private void Insert(XmlDocument doc, Note note)
    {
        string title = note.Type == NoteType.Normal ? note.Title! : string.Empty;
        string value = note.Type == NoteType.Normal ? note.Content! : "Unsupported note type (Mind-map or Sound note)";

        doc.AppendNote(
            title,
            value, 
            note.CreatedAt, 
            note.Type
            );
    }

    private void InitializeTimer()
    {
        _watch.Reset();
        _watch.Start();
    }

    private void UpdateCurrentNote()
    {
        Application.Current.Dispatcher.Invoke(() => CurrentNote++);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

    [GeneratedRegex("[^\\d]")]
    private static partial Regex DigitRegex();
}
