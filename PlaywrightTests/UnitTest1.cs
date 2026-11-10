using Microsoft.Playwright;

namespace PlaywrightTests;

public class UnitTest1 : AppPageTest
{
    [Fact]
    public async Task Test1()
    {
        await Page.GotoAsync("/");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Edit Albums" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Album Manager" })).ToBeVisibleAsync();
    }
}
