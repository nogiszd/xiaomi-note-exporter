using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace xiaomiNoteExporter.Gui.Extensions;

public static class DriverExtensions
{
    /// <summary>
    /// Check if <c>WebDriver</c> is still in opened state.
    /// </summary>
    /// <param name="driver">Instance of <c>WebDriver</c>.</param>
    public static bool IsClosed(this WebDriver driver)
    {
        bool flag = false;

        try
        {
            var title = driver.Title;

            if (title != null) 
            {
                flag = false;
            }
        } 
        catch (Exception)
        {   
            flag = true;
        }

        return flag;
    }

    /// <summary>
    /// Get <c>WebDriverWait</c> instance with specified <c>TimeSpan</c>.
    /// </summary>
    /// <param name="driver">Instance of <c>WebDriver</c>.</param>
    /// <param name="timeSpan">Chosen <c>TimeSpan</c>.</param>
    public static WebDriverWait GetWait(this WebDriver driver, TimeSpan timeSpan) => new(driver, timeSpan);
}
