using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using xiaomiNoteExporter.Gui.Extensions;

namespace xiaomiNoteExporter.Gui;

public sealed class BrowserWorker
{
    private readonly CancellationTokenSource cancellationTokenSource = new();

    private readonly ChromeDriver _driver;

    private readonly Window _window;

    public BrowserWorker(ChromeDriver driver, Window window)
    {
        _driver = driver;
        _window = window;
    }

    public async Task Start()
    {
        await Task.Run(async () => await Check_IsBrowserClosed(cancellationTokenSource.Token));
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
    }

    private async Task Check_IsBrowserClosed(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_driver.IsClosed())
            {
                MessageBox.Show(
                    "Browser was closed. Program will terminate now...",
                    "Oops",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                    );
                _window.Dispatcher.Invoke(() => _window.Close());
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}
