using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace xiaomiNoteExporter;

public static class DriverHelpers
{
    public static IReadOnlyCollection<IWebElement> TryFindImages(IWebElement scope)
    {
        // temporarily set implicit wait to zero
        var driver = ((IWrapsDriver)scope).WrappedDriver;
        var timeouts = driver.Manage().Timeouts();
        var originalImplicitWait = timeouts.ImplicitWait;
        timeouts.ImplicitWait = TimeSpan.Zero;

        try
        {
            var found = scope.FindElements(By.CssSelector(".image-view img"));
            return found.Count > 0 ? found : Array.Empty<IWebElement>();
        }
        finally
        {
            // restore original implicit wait timeout
            timeouts.ImplicitWait = originalImplicitWait;
        }
    }

    public static void WaitUntilImagesAreRealAndLoaded(IWebDriver driver, IReadOnlyCollection<IWebElement> imgs, TimeSpan periodPerItem)
    {
        foreach (var img in imgs)
        {
            var w = new WebDriverWait(driver, periodPerItem);
            w.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(NoSuchElementException));
            w.Until(d => IsRealImageLoaded(d, img));
        }
    }

    public static string GetCurrentSrc(IWebDriver driver, IWebElement img)
    {
        try
        {
            var js = (IJavaScriptExecutor)driver;
            var src = (string?)js.ExecuteScript("return arguments[0].currentSrc || arguments[0].src || '';", img);
            return src ?? string.Empty;
        }
        catch
        {
            return img.GetAttribute("src") ?? string.Empty;
        }
    }

    private static bool IsRealImageLoaded(IWebDriver driver, IWebElement imgEl)
    {
        try
        {
            var js = (IJavaScriptExecutor)driver;

            var src = (string?)js.ExecuteScript("return arguments[0].currentSrc || arguments[0].src || '';", imgEl) ?? "";
            if (string.IsNullOrWhiteSpace(src)) return false;

            if (src.StartsWith("data:image/svg+xml", StringComparison.OrdinalIgnoreCase)) return false;

            var complete = (bool)(js.ExecuteScript("return arguments[0].complete === true;", imgEl) ?? false);
            var hasSize = (bool)(js.ExecuteScript("return (arguments[0].naturalWidth||0) > 0;", imgEl) ?? false);

            return complete && hasSize;
        }
        catch
        {
            return false;
        }
    }
}
