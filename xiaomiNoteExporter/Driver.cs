using OpenQA.Selenium.Chrome;

namespace xiaomiNoteExporter;

public class Driver
{
    private static string[] _args = Array.Empty<string>();

    public Driver(string[] args)
    {
        _args = args;
    }

    public ChromeDriver Prepare(bool useStaticDriver = false) 
    {
        ChromeDriverService service;

        ChromeOptions options = new();
        options.AddArguments(_args);

        if (useStaticDriver)
            service = ChromeDriverService.CreateDefaultService(AppContext.BaseDirectory);
        else
            service = ChromeDriverService.CreateDefaultService();

        service.HideCommandPromptWindow = true;

        return new ChromeDriver(service, options);
    }
}
