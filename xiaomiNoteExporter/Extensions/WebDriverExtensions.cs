using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace xiaomiNoteExporter.Extensions;

public static class WebDriverExtensions
{
    /// <summary>
    /// Creates a new instance of the WebDriverWait class using the specified WebDriver and timeout interval.
    /// </summary>
    /// <param name="driver">The WebDriver instance to use for waiting operations. Cannot be null.</param>
    /// <param name="timeSpan">The maximum amount of time to wait for a condition to be met before timing out.</param>
    /// <returns>A <c>WebDriverWait</c> object configured with the specified driver and timeout interval.</returns>
    public static WebDriverWait GetWait(this WebDriver driver, TimeSpan timeSpan) => new(driver, timeSpan);
}
