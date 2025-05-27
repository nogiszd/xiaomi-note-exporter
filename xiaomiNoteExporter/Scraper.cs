using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Pastel;

using xiaomiNoteExporter.Extensions;

namespace xiaomiNoteExporter;

public partial class Scraper(ChromeDriver driver, Action shutdownHandler)
{
    private readonly ChromeDriver _driver = driver;

    private WebDriverWait? _wait;

    private readonly Action _shutdownHandler = shutdownHandler;

    private int totalNotes = 0;

    private int currentNote = 0;

    /// <summary>
    /// Method that starts the scraping process.
    /// </summary>
    /// <param name="domain">Domain address to be visited by <c>ChromeDriver</c>.</param>
    /// <param name="timeStampFormat">Format of the timestamp for file (or directory) name.</param>
    /// <param name="split">If <c>true</c> then notes will be split as separate files.</param>
    public void Start(string domain, string timeStampFormat, bool split = false)
    {
        _wait = _driver.GetWait(TimeSpan.FromSeconds(10));

        _driver.Navigate().GoToUrl($"https://{domain}/note/h5/?locale=en-US");
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        new Prompt($"\n{"[IMPORTANT]".Pastel(Color.Red)} Please sign-in to your account. Press any key after you succeed...").Ask(true);

        try
        {
            if (_wait.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'ant-tabs')]"))).Count != 0)
            {
                Console.WriteLine($"\n{"User didn't sign into Mi Cloud or account is invalid.".Pastel(Color.Red)}");
                Console.WriteLine("Application will exit now...".Pastel(Color.Gray));
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            _shutdownHandler();
            Console.WriteLine($"\n{ex.Message.ToString().Pastel(Color.Red)}");
            Console.ReadKey();
        }

        Scrape(timeStampFormat, split);
    }

    private void Scrape(string timeStampFormat, bool split)
    {
        if (_wait is null)
        {
            return;
        }

        _wait.Until(e =>
        {
            var style = e.FindElement(By.XPath(@"//body/div[contains(@class, 'spinner')]")).GetAttribute("style");
            return style != null && style.Contains("display: none");
        });
        _wait.Until(e => e.FindElement(By.XPath(@"//button[contains(@class, 'btn-create')]")).Displayed);
        Console.Clear();

        try
        {
            Stopwatch watch = new();
            var innerWait = _driver.GetWait(TimeSpan.FromMilliseconds(10));
            watch.Start();

            string notesElements = _wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'note-count-select')]"))).Text;
            totalNotes = int.Parse(DigitRegex().Replace(notesElements, ""));

            IWebElement notesList = _wait.Until(e => e.FindElement(By.XPath("//div[contains(@class, 'note-list-items')]")));

            string currentExportDate = DateTime.Now.ToString(timeStampFormat);
            string exportName = $"exported_notes_{currentExportDate}";
            string fileName = $"{exportName}.md"; // file name for accumulated notes (without split)

            if (split)
            {
                // if split is enabled, create directory in which notes will be saved
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exportName));
            }

            bool isFirst = true; // this check is needed, because it usually opens first note automatically

            while (true)
            {
                Console.Title = $"Parsed {currentNote} notes out of {totalNotes}...";
                Console.Write($"\rParsed {currentNote} notes out of {totalNotes}... ({currentNote.GetPercentage(totalNotes)}%)");

                if (currentNote == totalNotes)
                {
                    watch.Stop();
                    _shutdownHandler();
                    Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory + "");
                    break;
                }
                else
                {
                    IWebElement element;
                    if (!isFirst)
                    {
                        // if element is not the first one, use following element
                        element = _wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'open')]/following::div")));
                    } 
                    else
                    {
                        element = _wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'open')]")));
                        isFirst = false;
                    }

                    element.Click(); // open the note
                    Thread.Sleep(200); // timeout for optimization

                    // creation date text (retrieved from UI)
                    string createdString = element.FindElement(By.XPath(@".//div[2]/div[1]")).Text;

                    // creation date (calculated from retrieved text)
                    DateTime createdDate;

                    if (createdString.ToLower().Contains("yesterday"))
                    {
                        createdDate = DateTime.Now.AddDays(-1).Date; // get yesterday's date
                    } 
                    else if (createdString.EndsWith("ago"))
                    {
                        createdDate = RelativeTimeParser.Parse(createdString);
                    } 
                    else if (SimplifiedDateParser.TryParseMdHm(createdString, out DateTime parsedSimple))
                    {
                        createdDate = parsedSimple;
                    } 
                    else
                    {
                        createdDate = DateTime.Parse(createdString, new CultureInfo("en-US"));
                    }

                    try
                    {
                        innerWait.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'open')]/div[2][not(./i)]")).Count == 1);
                    } 
                    catch
                    {
                        // found note that is not supported, log this fact and continue
                        SaveToFile(
                            !split ? fileName : $"{exportName}\\{$"note_{createdDate.ToString(timeStampFormat)}"}.md", 
                            $"** Unsupported note type (Mind-map or Sound note) (Created at: {createdDate:dd/MM/yyyy HH:mm})**"
                            );
                        ExecuteScroll(notesList, element);
                        currentNote++;
                        continue;
                    }

                    _wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'origin-title')]/div")).Displayed);

                    string title = _wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'origin-title')]/div"))).Text;
                    string value = _wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'pm-container')]"))).Text;

                    SaveToFile(
                        !split ? fileName : $"{exportName}\\{$"note_{createdDate.ToString(timeStampFormat)}"}.md", 
                        value, 
                        title
                        );
                    ExecuteScroll(notesList, element);
                    currentNote++;
                }
            }

            Console.Clear();
            Console.Title = string.Format("Completed! (took {0:00}:{1:00}:{2:00})",
                watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);

            if (split)
            {
                Console.WriteLine($"Successfully exported notes to {exportName.Pastel(Color.WhiteSmoke)} directory\n".Pastel(Color.LimeGreen));
            } 
            else
            {
                Console.WriteLine($"Successfully exported notes to {fileName.Pastel(Color.WhiteSmoke)}\n".Pastel(Color.LimeGreen));
            }

            Console.WriteLine("Press any key to close application...".Pastel(Color.Gray));
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            _shutdownHandler();
            Console.WriteLine($"\nPlease report this error on GitHub".Pastel(Color.Gray));
            Console.WriteLine($"\n{ex.ToString().Pastel(Color.Red)}");
            Console.ReadKey();
        }
    }

    private static void SaveToFile(string fileName, string content, string? title = null)
    {
        using StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + fileName);

        sw.WriteLine("****");

        if (!string.IsNullOrEmpty(title))
        {
            sw.WriteLine($"**{title}**");
        }

        sw.WriteLine(content);
    }

    private void ExecuteScroll(IWebElement notesList, IWebElement currentElement)
    {
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollBy(0, arguments[1]);", notesList, currentElement.Size.Height);
    }

    [GeneratedRegex("[^\\d]")]
    private static partial Regex DigitRegex();
}
