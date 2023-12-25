using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using xiaomiNoteExporter.Gui.Entities;
using xiaomiNoteExporter.Gui.Extensions;

namespace xiaomiNoteExporter.Gui.Services
{
    public sealed partial class ScrapeService
    {
        private readonly ChromeDriver _driver;

        public readonly Stopwatch watch;

        public ScrapeService(ChromeDriver driver)
        {
            _driver = driver;

            watch = new();
        }

        public void Scrape()
        {
            var span = _driver.GetWait(TimeSpan.FromMilliseconds(10));
            var wait = _driver.GetWait(TimeSpan.FromSeconds(10));

            watch.Start();

            string notesAmountEl = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'note-count-select')]"))).Text;
            int notesAmount = int.Parse(DigitRegex().Replace(notesAmountEl, ""));

            IWebElement noteList = wait.Until(e => e.FindElement(By.XPath("//div[contains(@class, 'note-list-items')]")));

            string fileName = $"{DateTime.Now:dd-MM-yy_HH-mm-ss}";
            XmlDocument doc = XmlExtensions.Initialize("notes");

            int control = 0;
            bool isFirst = true; // check is needed because it usually opens first note automatically

            while (true)
            {
                StatusbarService.SetStatus($"Parsed: {control}/{notesAmount} ({(int)(.5f + 100f * control / notesAmount)}%)");

                if (control == notesAmount)
                {
                    watch.Stop();
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
                        control++;
                        continue;
                    }

                    wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'origin-title')]/div")).Displayed);

                    string title = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'origin-title')]/div"))).Text;
                    string value = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'pm-container')]"))).Text;

                    Insert(doc, Note.Create(title, value, createdAt, NoteType.Normal));

                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollBy(0, arguments[1]);", noteList, el.Size.Height);
                    control++;
                }
            }

            doc.Save($"{fileName}.xml");
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

        [GeneratedRegex("[^\\d]")]
        private static partial Regex DigitRegex();
    }
}
