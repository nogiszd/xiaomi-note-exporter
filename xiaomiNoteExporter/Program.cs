using System.Drawing;
using System.Reflection;

using OpenQA.Selenium.Chrome;
using Pastel;

namespace xiaomiNoteExporter;

class Program
{
    public static Version? appVersion = Assembly.GetExecutingAssembly().GetName().Version;
    public static string defaultDomain = "us.i.mi.com";

    readonly static Driver _driver = new(Array.Empty<string>());
    static readonly ChromeDriver driver = _driver.Prepare();

    static void ShutdownHandler_Handler()
    {
        driver.Close();
        driver.Quit();
    }

    public static void Main()
    {
        Console.Title = $"Xiaomi Note Exporter {appVersion?.Major}.{appVersion?.Minor}.{appVersion?.Build}";
        Console.WriteLine($"{"Xiaomi Note Exporter".Pastel(Color.FromArgb(252, 106, 0))} - Export your notes to {"Markdown".Pastel(Color.SkyBlue)}!\n");
        string? domain = new Prompt(
            $"{"[OPTIONAL]".Pastel(Color.DimGray)} Input Mi Notes domain that you were redirected to (default \"{defaultDomain}\"):",
            defaultDomain
            ).Ask();

        new Scraper(driver, ShutdownHandler_Handler).Start(domain);
    }
}