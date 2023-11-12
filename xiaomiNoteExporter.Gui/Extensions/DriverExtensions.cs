using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace xiaomiNoteExporter.Gui.Extensions
{
    public static class DriverExtensions
    {
        public static bool IsClosed(this WebDriver driver)
        {
            bool flag = false;

            try
            {
                if (driver.Title != null)
                {
                    flag = false;
                }
            } catch (Exception)
            {
                flag = true;
            }

            return flag;
        }

        public static WebDriverWait GetWait(this WebDriver driver, TimeSpan timeSpan) => new(driver, timeSpan);
    }
}
