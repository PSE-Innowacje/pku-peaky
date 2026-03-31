using Microsoft.Playwright;

namespace PKU.E2ETests;

[Collection("Playwright")]
public class HomeTests
{
    private readonly PlaywrightFixture _fixture;

    public HomeTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HomePage_ShouldDisplayWelcomeMessage()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("PKU - Platforma skladania oswiadczen");

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task HomePage_ShouldHaveAdminLink()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        var adminLink = page.Locator("a.btn[href='admin/users']");
        await Assertions.Expect(adminLink).ToBeVisibleAsync();
        await Assertions.Expect(adminLink).ToContainTextAsync("Przejdz");

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task HomePage_ShouldHaveDashboardLink()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        var dashboardLink = page.Locator("a.btn[href='rozliczenia/dashboard']");
        await Assertions.Expect(dashboardLink).ToBeVisibleAsync();

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task HomePage_NavigateToAdmin_ShouldShowUsersPage()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        await page.ClickAsync("a.btn[href='admin/users']");
        await page.WaitForURLAsync("**/admin/users");

        await Assertions.Expect(page.Locator("h3")).ToContainTextAsync("Uzytkownicy");

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task HomePage_NavigateToDashboard_ShouldShowDashboard()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        await page.ClickAsync("a.btn[href='rozliczenia/dashboard']");
        await page.WaitForURLAsync("**/rozliczenia/dashboard");

        await Assertions.Expect(page.Locator("h3")).ToContainTextAsync("Dashboard oswiadczen");

        await page.Context.DisposeAsync();
    }
}
