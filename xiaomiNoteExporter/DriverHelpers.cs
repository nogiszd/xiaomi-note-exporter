using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace xiaomiNoteExporter;

public static class DriverHelpers
{
    /// <summary>
    /// Attempts to locate all image elements within the specified scope that match the <c>".image-view img"</c> CSS selector.
    /// </summary>
    /// <remarks>This method temporarily sets the driver's implicit wait to zero to ensure immediate search
    /// results, then restores the original timeout. Only elements matching the <c>".image-view img"</c> selector within the
    /// given scope are returned.</remarks>
    /// <param name="scope">The web element that defines the search context for locating image elements.</param>
    /// <returns>A read-only collection of <c>IWebElement</c> objects representing the found images. Returns an empty collection if no
    /// matching images are found.</returns>
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
        catch
        {
            return Array.Empty<IWebElement>();
        }
        finally
        {
            // restore original implicit wait timeout
            timeouts.ImplicitWait = originalImplicitWait;
        }
    }

    /// <summary>
    /// Waits until each image element in the specified collection is fully loaded and represents a real image in the
    /// browser.
    /// </summary>
    /// <remarks>This method waits for each image in the collection individually, using the specified timeout
    /// for each item. If an image does not load within the given period, the method returns false.
    /// The method ignores stale or missing elements during the wait, retrying as necessary until the timeout
    /// expires.</remarks>
    /// <param name="driver">The WebDriver instance used to interact with the browser.</param>
    /// <param name="imgs">A collection of image elements to check for being real and fully loaded.</param>
    /// <param name="periodPerItem">The maximum amount of time to wait for each image to load before timing out.</param>
    /// <returns>True if all images are loaded successfully; otherwise, false.</returns>
    public static bool WaitUntilImagesAreRealAndLoaded(IWebDriver driver, IReadOnlyCollection<IWebElement> imgs, TimeSpan periodPerItem)
    {
        try
        {
            foreach (var img in imgs)
            {
                var w = new WebDriverWait(driver, periodPerItem);
                w.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(NoSuchElementException));
                w.Until(d => IsRealImageLoaded(d, img));
            }

            return true;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the resolved image source URL from the specified <see cref="IWebElement"/> representing an image element.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance used to execute JavaScript in the browser context. Cannot be null.</param>
    /// <param name="img">The <see cref="IWebElement"/> representing the image element whose source URL is to be retrieved. Cannot be
    /// null.</param>
    /// <returns>A string containing the resolved image source URL. Returns an empty string if the source cannot be determined.</returns>
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

    /// <summary>
    /// Determines whether the specified image element has successfully loaded a non-SVG image in the browser.
    /// </summary>
    /// <remarks>This method uses JavaScript execution to verify the image's load status and excludes inline
    /// SVG images from being considered as loaded. If an error occurs during script execution, the method returns
    /// false.</remarks>
    /// <param name="driver">The web driver instance used to execute JavaScript in the browser context. Cannot be null.</param>
    /// <param name="imgEl">The image element to check for load status. Cannot be null.</param>
    /// <returns>true if the image element has a non-empty source, is not an inline SVG, and is fully loaded with a natural width
    /// greater than zero; otherwise, false.</returns>
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
