using System.Drawing;
using System.Reflection;
using OpenQA.Selenium.Chrome;
using Pastel;

using xiaomiNoteExporter.Extensions;

namespace xiaomiNoteExporter;

class Program
{
    public static Version? appVersion = Assembly.GetExecutingAssembly().GetName().Version;

    static bool _shouldAskForDomain = true;
    public static string defaultDomain = "us.i.mi.com";

    static bool _shouldSplit = false;
    static string _timestampFormat = "dd-MM-yyyy_HH-mm-ss";

    readonly static Driver _driver = new(Array.Empty<string>());
    static ChromeDriver? driver;

    static void ShutdownHandler_Handler()
    {
        if (driver is not null)
        {
            driver.Close();
            driver.Quit();
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            if (args[0].Includes("-h", "--help"))
            {
                ShowHelp();
                return;
            }
            else
            {
                ParseArgs(args);
            }
        }

        driver = _driver.Prepare(); // prepare driver after parsing arguments from command line

        Console.Title = $"Xiaomi Note Exporter {appVersion?.GetVersionString()}";

        Console.WriteLine(
            $"{"Xiaomi Note Exporter".Pastel(Color.FromArgb(252, 106, 0))} - Export your notes to {"Markdown".Pastel(Color.SkyBlue)}!\n"
            );

        string? domain = _shouldAskForDomain 
            ? new Prompt(
                $"{"[OPTIONAL]".Pastel(Color.DimGray)} Input Mi Notes domain that you were redirected to (default \"{defaultDomain}\"):",
                defaultDomain
                ).Ask() 
            : defaultDomain;

        new Scraper(driver, ShutdownHandler_Handler).Start(domain, _timestampFormat, _shouldSplit);
    }

    private static void ShowHelp() => new ConsoleHelp(appVersion).Print();

    private static void ParseArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg.Includes("-d", "--domain"))
            {
                if (i + 1 < args.Length)
                {
                    string domain = args[i + 1];

                    if (!string.IsNullOrEmpty(domain))
                    {
                        defaultDomain = domain; // set global domain to the provided value
                        _shouldAskForDomain = false; // shouldn't ask for domain, since it was provided as argument
                    } 
                    else
                    {
                        Console.WriteLine($"{"[ERROR]".Pastel(Color.Red)} Provided domain is invalid.");
                        Environment.Exit(1);
                    }

                    i++; // skip next argument - this was a value
                } 
                else
                {
                    Console.WriteLine($"{"[ERROR]".Pastel(Color.Red)} Domain address is required with domain flag.");
                    Environment.Exit(1);
                }
            } 
            else if (arg.Includes("-s", "--split"))
            {
                if (i + 1 < args.Length)
                {
                    string timestampFormat = args[i + 1];

                    if (!string.IsNullOrEmpty(timestampFormat))
                    {
                        try
                        {
                            DateTime.Now.ToString(timestampFormat);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine($"{"[ERROR]".Pastel(Color.Red)} Invalid timestamp format.");
                            Environment.Exit(1);
                        }

                        _timestampFormat = timestampFormat;
                    }

                    i++; // skip next argument - this was a value
                }

                _shouldSplit = true; // if flag is present, split is enabled (even if no value is provided)
            }
        }
    }
}