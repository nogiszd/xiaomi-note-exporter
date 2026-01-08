using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace xiaomiNoteExporter.Extensions;

public static class WebDriverExtensions
{
    public static WebDriverWait GetWait(this WebDriver driver, TimeSpan timeSpan) => new(driver, timeSpan);
}
