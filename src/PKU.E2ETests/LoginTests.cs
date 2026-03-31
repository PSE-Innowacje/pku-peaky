using Microsoft.Playwright;

namespace PKU.E2ETests;

[Collection("Playwright")]
public class LoginTests
{
    private readonly PlaywrightFixture _fixture;

    public LoginTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task LoginPage_ShouldDisplay_LoginForm()
    {
        var context = await _fixture.CreateContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/login");

        await Assertions.Expect(page.Locator("h3")).ToContainTextAsync("PKU - Logowanie");
        await Assertions.Expect(page.Locator("#email")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#password")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("button[type='submit']")).ToBeVisibleAsync();

        await context.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldRedirectToHome()
    {
        var context = await _fixture.CreateContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.FillAsync("#email", "admin@pku.pl");
        await page.FillAsync("#password", "admin123");
        await page.ClickAsync("button[type='submit']");

        await page.WaitForURLAsync($"{_fixture.BaseUrl}/");
        await Assertions.Expect(page.Locator("h1")).ToContainTextAsync("PKU - Platforma skladania oswiadczen");

        await context.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldShowError()
    {
        var context = await _fixture.CreateContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/login");
        await page.FillAsync("#email", "wrong@pku.pl");
        await page.FillAsync("#password", "wrongpassword");
        await page.ClickAsync("button[type='submit']");

        await page.WaitForURLAsync("**/login?error=1");
        await Assertions.Expect(page.Locator(".alert-danger")).ToContainTextAsync("Nieprawidlowy email lub haslo");

        await context.DisposeAsync();
    }

    [Fact]
    public async Task UnauthenticatedUser_ShouldBeRedirectedToLogin()
    {
        var context = await _fixture.CreateContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/");

        await Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("login"));

        await context.DisposeAsync();
    }

    [Fact]
    public async Task Logout_ShouldRedirectToLogin()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/account/logout");

        await Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("login"));

        await page.Context.DisposeAsync();
    }
}
