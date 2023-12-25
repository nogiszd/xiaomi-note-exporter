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
                var title = driver.Title;

                if (title != null) 
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
