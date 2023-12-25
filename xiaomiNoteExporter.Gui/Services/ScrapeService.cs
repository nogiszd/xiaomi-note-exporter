using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using xiaomiNoteExporter.Gui.Extensions;

namespace xiaomiNoteExporter.Gui.Services
{
    public sealed class ScrapeService
    {
        private readonly ChromeDriver _driver;

        public ScrapeService(ChromeDriver driver)
        {
            _driver = driver;
        }

        public void Scrape()
        {
            Stopwatch watch = new();
            var span = _driver.GetWait(TimeSpan.FromMilliseconds(10));
            watch.Start();
        }
    }
}
