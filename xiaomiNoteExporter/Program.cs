using System.Drawing;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using System.Reflection;
using Pastel;
using System.Runtime.InteropServices;
using System.Globalization;

namespace xiaomiNoteExporter
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        public delegate bool HandlerRoutine(CtrlTypes ctrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6,
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                default:
                    HandleShutdown();
                    Environment.Exit(0);
                    break;
            }

            return true;
        }

        public static Version? appVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static string defaultDomain = "us.i.mi.com";
        public static string defaultTimestampOutputFormat = "yyyy_MMMM_dd_[HH-mm]";
        readonly static Driver _driver = new(Array.Empty<string>());
        static readonly ChromeDriver driver = _driver.Prepare();
        public delegate void ShutdownHandler();

        static void HandleShutdown()
        {
            driver.Close();
            driver.Quit();
        }

        static void Main()
        {
            ShutdownHandler shutdownHandler = new(HandleShutdown);
            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

            Console.Title = $"Xiaomi Note Exporter {appVersion?.Major}.{appVersion?.Minor}.{appVersion?.Build}";
            Console.WriteLine($"{"Xiaomi Note Exporter".Pastel(Color.FromArgb(252, 106, 0))} - Export your notes to {"Markdown".Pastel(Color.SkyBlue)}!\n");
            string? domain = new Prompt(
                $"{"[OPTIONAL]".Pastel(Color.DimGray)} Input Mi Notes domain that you were redirected to (default \"{defaultDomain}\"):",
                defaultDomain
                ).Ask();

            int notesAmount;

            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(10));

            driver.Navigate().GoToUrl($"https://{domain}/note/h5#");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            new Prompt($"\n{"[IMPORTANT]".Pastel(Color.Red)} Please sign-in to your account. Press any key after you succeed...").Ask(true);

            Console.Clear();

            // if account didnt sign in then show error
            try
            {
                if (wait.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'ant-tabs')]"))).Count != 0)
                {
                    //shutdownHandler();
                    Console.WriteLine($"\n{"User didn't sign into Mi Cloud or account is invalid.".Pastel(Color.Red)}");
                    Console.WriteLine("Application will exit now...".Pastel(Color.Gray));
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                shutdownHandler();
                Console.WriteLine($"\n{ex.Message.ToString().Pastel(Color.Red)}");
                Console.ReadKey();
            }

            wait.Until(e => e.FindElement(By.XPath(@"//body/div[contains(@class, 'spinner')]")).GetAttribute("style").Contains("display: none"));
            wait.Until(e => e.FindElement(By.XPath(@"//button[contains(@class, 'btn-create')]")).Displayed);

            Console.Clear();

            Console.WriteLine("Input date formats as in xiaomi cloud service." +
                "Check this link for more info about date formats: \"https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings\"\n");
            string? parse_string_full = new Prompt(
                $"{"[EXPLICIT]".Pastel(Color.GreenYellow)} Input full-length date format (day, month and year). (examples: \"M/d/yyyy HH:mm\", \"d.M.yyyy HH:mm\"):"
                ).Ask();
            string? parse_string_short = new Prompt(
                $"{"[EXPLICIT]".Pastel(Color.GreenYellow)} Input short date format (day and month). (examples: \"M/d HH:mm\", \"d.M HH:mm\"):"
                ).Ask();
            string? parse_output_string = new Prompt(
                $"{"[OPTIONAL]".Pastel(Color.DimGray)} Input output timestamp format. (default is: \"{defaultTimestampOutputFormat}\"):",
                defaultTimestampOutputFormat
                ).Ask();

            try
            {
                Stopwatch watch = new();
                WebDriverWait innerWait = new(driver, TimeSpan.FromMilliseconds(50));
                watch.Start();
                string notesAmountEl = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'note-count-select')]"))).Text;
                notesAmount = int.Parse(Regex.Replace(notesAmountEl, @"[^\d]", ""));
                IWebElement noteList = wait.Until(e => e.FindElement(By.XPath("//div[contains(@class, 'note-list-items')]")));

                int control = 0;
                bool isFirst = true; // check is needed because it usually opens first note automatically
                System.IO.Directory.CreateDirectory("exported_notes");

                while (true)
                {
                    Console.Title = $"Parsed {control} notes out of {notesAmount}...";
                    Console.Write($"\rParsed {control} notes out of {notesAmount}... ({(int)(0.5f + ((100f * control) / notesAmount))}%)");
                    if (control == notesAmount)
                    {
                        watch.Stop();
                        shutdownHandler();
                        Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory + "exported_notes");
                        break;
                    } else
                    {
                        DateTime currentTime = DateTime.Now;
                        IWebElement el;
                        if ( !isFirst )
                        {
                            el = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'open')]/following::div")));
                        } else
                        {
                            el = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'open')]")));
                            isFirst = false;
                        }

                        el.Click();
                        Thread.Sleep(200);  // timeout for fetching optimization
                        wait.Until(e => e.FindElement(By.XPath(@"//body/div[contains(@class, 'spinner')]")).GetAttribute("style").Contains("display: none")); // make sure it's loaded

                        string lastModifiedTimeRaw = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'open')]/div[2]/div/span"))).Text; // get last modification time string

                        //// parse last modification time string
                        string lastModifiedTime;
                        try
                        {
                            lastModifiedTime = DateTime.ParseExact(lastModifiedTimeRaw, parse_string_full, CultureInfo.InvariantCulture).ToString(parse_output_string);
                        } catch
                        {
                            try
                            {
                                DateTime tempDate = DateTime.ParseExact(lastModifiedTimeRaw, parse_string_short, CultureInfo.InvariantCulture);
                                tempDate.AddYears(currentTime.Year - tempDate.Year); // set current year
                                lastModifiedTime = tempDate.ToString(parse_output_string);
                            }
                            catch
                            {
                                lastModifiedTime = currentTime.ToString(parse_output_string) + "_" + lastModifiedTimeRaw.Replace(' ', '_').Replace('\\', '_').Replace('.', '_');
                            }
                        }

                        string fName = "exported_notes/" + $"{lastModifiedTime}.md";

                        try
                        {
                            innerWait.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'open')]/div[2][not(./i[contains(@class, 'mind')])]")).Count == 1); // mind maps are unsopported (TODO: add check for pictures and audio samples)
                        } catch
                        {
                            string createdAt = el.FindElement(By.XPath(@".//div[2]/div[1]")).Text;
                            using StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + $"{fName}");
                            sw.WriteLine($"**Unsupported note type (Mind-map or Sound note) (Created at: {createdAt})**");

                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollBy(0, arguments[1]);", noteList, el.Size.Height);
                            control++;
                            continue;
                        }

                        wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'origin-title')]/div")).Displayed);

                        string title = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'origin-title')]/div"))).Text;
                        string value = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'pm-container')]"))).Text;

                        using (StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + $"{fName}"))
                        {
                            if (title != "") { sw.WriteLine($"***{title}***"); }
                            sw.WriteLine(value);
                        }

                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollBy(0, arguments[1]);", noteList, el.Size.Height); // scroll down one note
                        control++;
                    }
                }

                Console.Title = string.Format("Completed! (took {0:00}:{1:00}:{2:00})", 
                    watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
                // Console.WriteLine($"Successfully exported notes to {fName.Pastel(Color.WhiteSmoke)}\n".Pastel(Color.LimeGreen));
                Console.WriteLine("\n\nPress any key to close application...".Pastel(Color.Gray));
                Console.ReadKey();
            } catch (Exception ex) {
                shutdownHandler();
                Console.WriteLine($"\nPlease report this error on GitHub".Pastel(Color.Gray));
                Console.WriteLine($"\n{ex.ToString().Pastel(Color.Red)}");
                Console.ReadKey();
            }
        }
    }
}