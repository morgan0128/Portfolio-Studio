using System.Globalization;
using Microsoft.Playwright;
using Microsoft.Playwright.TestAdapter;
using Microsoft.Playwright.Xunit.v3;

namespace PlaywrightTests;

public abstract class AppPageTest : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions();
        options.BaseURL = ApplicationFixture.FrontendUrl;
        return options;
    }

    public override Task<BrowserTypeLaunchOptions?> LaunchOptionsAsync()
    {
        var options = new BrowserTypeLaunchOptions(PlaywrightSettingsProvider.LaunchOptions);
        var slowMo = Environment.GetEnvironmentVariable("E2E_SLOW_MO");
        if (!string.IsNullOrWhiteSpace(slowMo))
        {
            options.SlowMo = float.Parse(slowMo, CultureInfo.InvariantCulture);
        }

        return Task.FromResult<BrowserTypeLaunchOptions?>(options);
    }
}
