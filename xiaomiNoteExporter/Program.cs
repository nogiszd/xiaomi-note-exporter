using System.Drawing;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using System.Reflection;
using Pastel;

namespace xiaomiNoteExporter
{
    internal static class Program
    {
        public static Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
        static ChromeDriver PrepareDriver()
        {
            ChromeOptions options = new ChromeOptions();
            string[] args = {"--headless"}; // here put optional chrome args (eg. "--headless" - makes chrome working in background without showing window)
            options.AddArguments(args);
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeDriver driver = new ChromeDriver(service, options);
            return driver;
        }

        static void Main()
        {
            Console.Title = $"Xiaomi Note Exporter {appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";
            Console.WriteLine($"{"Xiaomi Note Exporter".Pastel(Color.FromArgb(252, 106, 0))} - Export your notes to {"Markdown".Pastel(Color.SkyBlue)}!\n");
            string? token = "";
            string? userId = "";
            int notesAmount;

            Console.WriteLine("Input session token: ");
            while (true)
            {
                token = Console.ReadLine();
                if (token == "")
                {
                    Console.Clear();
                    Console.WriteLine($"Input {"valid".Pastel(Color.Red)} session token: ");
                } else
                {
                    break;
                }
            }

            Console.WriteLine("Input userId: ");
            while (true)
            {
                userId = Console.ReadLine();
                if (userId == "")
                {
                    Console.Clear();
                    Console.WriteLine($"Input {"valid".Pastel(Color.Red)} userId: ");
                } else
                {
                    break;
                }
            }

            ChromeDriver driver = PrepareDriver();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.Navigate().GoToUrl("https://us.i.mi.com/note/h5#");
            driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie("serviceToken", token));
            driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie("userId", userId));
            driver.Navigate().GoToUrl("https://us.i.mi.com/note/h5#");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            // if token is invalid then show error
            if (driver.FindElements(By.XPath(@"//div[contains(@class, 'ant-tabs')]")).Count > 0)
            {
                driver.Close();
                driver.Quit();
                Console.WriteLine($"\n{"Provided session token was invalid or no longer active and couldn't access Mi Cloud.".Pastel(Color.Red)}");
                Console.WriteLine("Application will exit now...".Pastel(Color.Gray));
                Console.ReadKey();
                Environment.Exit(0);
            }

            wait.Until(e => e.FindElement(By.XPath(@"//button[contains(@class, 'btn-create')]")).Displayed);
            wait.Until(e => e.FindElement(By.XPath(@"//body/div[contains(@class, 'spinner')]")).GetAttribute("style").Contains("display: none"));
            Console.Clear();

            try {
                string notesAmountEl = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'note-count-select')]"))).Text;
                notesAmount = int.Parse(Regex.Replace(notesAmountEl, @"[^\d]", ""));
                IWebElement noteList = wait.Until(e => e.FindElement(By.XPath("//div[contains(@class, 'note-list-items')]")));
                string fName = $"exported_notes_{DateTime.Now.ToString("dd-MM-yy_HH-mm-ss")}.md";
                
                int control = 0;
                bool isFirst = true; // check is needed because it usually opens first note automatically
                while (true)
                {
                    Console.Title = $"Parsed {control} notes out of {notesAmount}...";
                    Console.Write($"\rParsed {control} notes out of {notesAmount}... ({(int)(0.5f + ((100f * control) / notesAmount))}%)");
                    if (control == notesAmount)
                    {
                        driver.Close();
                        driver.Quit();
                        Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory + "");
                        break;
                    } else
                    {
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
                        Thread.Sleep(200);
                        wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'ql-editor')]")).Text.Length > 0); // additional check for loading note timespan

                        string title = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'title-bar')]/input"))).GetAttribute("value");
                        string value = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'ql-editor')]"))).Text;

                        using (StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + $"{fName}"))
                        {
                            sw.WriteLine("****");
                            if (title != "") { sw.WriteLine($"**{title}**"); }
                            sw.WriteLine(value);
                        }

                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollBy(0, arguments[1]);", noteList, el.Size.Height);
                        control++;
                    }
                }


                Console.Clear();
                Console.WriteLine($"Successfully exported notes to {fName.Pastel(Color.WhiteSmoke)}\n".Pastel(Color.LimeGreen));
                Console.WriteLine("Press any key to close application...".Pastel(Color.Gray));
                Console.ReadKey();
            } catch (Exception ex) {
                driver.Close();
                driver.Quit();
                Console.WriteLine($"\n{ex.ToString().Pastel(Color.Red)}");
                Console.ReadKey();
            }
        }
    }
}