using Microsoft.Playwright;

namespace PKU.E2ETests;

[Collection("Playwright")]
public class DashboardTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_AsAdmin_ShouldDisplayAllCards()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/rozliczenia/dashboard");

        await Assertions.Expect(page.Locator("h3")).ToContainTextAsync("Dashboard oswiadczen");
        await Assertions.Expect(page.Locator(".card.bg-primary")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".card.bg-secondary")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".card.bg-success")).ToBeVisibleAsync();

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task Dashboard_AsKontrahent_ShouldShowContractorInfo()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync("osdp@pku.pl", "admin123");

        await page.GotoAsync($"{_fixture.BaseUrl}/rozliczenia/dashboard");

        await Assertions.Expect(page.Locator("text=Kontrahent:")).ToBeVisibleAsync();

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task Dashboard_ShouldDisplayCurrentMonth()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/rozliczenia/dashboard");

        var year = DateTime.Now.Year.ToString();
        await Assertions.Expect(page.Locator($"text={year}")).ToBeVisibleAsync();

        await page.Context.DisposeAsync();
    }
}
