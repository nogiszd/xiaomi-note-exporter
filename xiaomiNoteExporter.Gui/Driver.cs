using OpenQA.Selenium.Chrome;
using System;

namespace xiaomiNoteExporter.Gui
{
    public class Driver
    {
        private static string[] _args = Array.Empty<string>();

        public Driver(string[] args)
        {
            _args = args;
        }

        public static ChromeDriver Prepare()
        {
            ChromeOptions options = new();
            options.AddArguments(_args);

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            return new(service, options);
        }
    }
}
