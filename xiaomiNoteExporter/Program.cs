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
    class Program
    {
        public static Version? appVersion = Assembly.GetExecutingAssembly().GetName().Version;
        readonly static Driver _driver = new(new string[] {"--headless"});

        static void Main()
        {
            Console.Title = $"Xiaomi Note Exporter {appVersion?.Major}.{appVersion?.Minor}.{appVersion?.Build}";
            Console.WriteLine($"{"Xiaomi Note Exporter".Pastel(Color.FromArgb(252, 106, 0))} - Export your notes to {"Markdown".Pastel(Color.SkyBlue)}!\n");
            string? token = new Prompt("Input session token").Ask();
            string? userId = new Prompt("Input userId").Ask();
            int notesAmount;

            ChromeDriver driver = _driver.Prepare();
            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(10));

            driver.Navigate().GoToUrl("https://us.i.mi.com/note/h5#");
            driver.Manage().Cookies.AddCookie(new Cookie("serviceToken", token));
            driver.Manage().Cookies.AddCookie(new Cookie("userId", userId));
            driver.Navigate().GoToUrl("https://us.i.mi.com/note/h5#");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            // if token is invalid then show error
            try
            {
                if (wait.Until(e => e.FindElements(By.XPath(@"//div[contains(@class, 'ant-tabs')]"))).Count != 0)
                {
                    driver.Close();
                    driver.Quit();
                    Console.WriteLine($"\n{"Provided session token was invalid or no longer active and couldn't access Mi Cloud.".Pastel(Color.Red)}");
                    Console.WriteLine("Application will exit now...".Pastel(Color.Gray));
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            } 
            catch (Exception ex)
            {
                driver.Close();
                driver.Quit();
                Console.WriteLine($"\n{ex.Message.ToString().Pastel(Color.Red)}");
                Console.ReadKey();
            }


            wait.Until(e => e.FindElement(By.XPath(@"//body/div[contains(@class, 'spinner')]")).GetAttribute("style").Contains("display: none"));
            wait.Until(e => e.FindElement(By.XPath(@"//button[contains(@class, 'btn-create')]")).Displayed);
            Console.Clear();

            try {
                Stopwatch watch = new();
                watch.Start();
                string notesAmountEl = wait.Until(e => e.FindElement(By.XPath(@"//div[contains(@class, 'note-count-select')]"))).Text;
                notesAmount = int.Parse(Regex.Replace(notesAmountEl, @"[^\d]", ""));
                IWebElement noteList = wait.Until(e => e.FindElement(By.XPath("//div[contains(@class, 'note-list-items')]")));
                string fName = $"exported_notes_{DateTime.Now:dd-MM-yy_HH-mm-ss}.md";
                
                int control = 0;
                bool isFirst = true; // check is needed because it usually opens first note automatically
                while (true)
                {
                    Console.Title = $"Parsed {control} notes out of {notesAmount}...";
                    Console.Write($"\rParsed {control} notes out of {notesAmount}... ({(int)(0.5f + ((100f * control) / notesAmount))}%)");
                    if (control == notesAmount)
                    {
                        watch.Stop();
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
                        Thread.Sleep(200); // fallback for fetching optimization
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
                Console.Title = string.Format("Completed! (took {0:00}:{1:00}:{2:00})", 
                    watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
                Console.WriteLine($"Successfully exported notes to {fName.Pastel(Color.WhiteSmoke)}\n".Pastel(Color.LimeGreen));
                Console.WriteLine("Press any key to close application...".Pastel(Color.Gray));
                Console.ReadKey();
            } catch (Exception ex) {
                driver.Close();
                driver.Quit();
                Console.WriteLine($"\nPlease report this error on GitHub".Pastel(Color.Gray));
                Console.WriteLine($"\n{ex.ToString().Pastel(Color.Red)}");
                Console.ReadKey();
            }
        }
    }
}