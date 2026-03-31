using Microsoft.Playwright;

namespace PKU.E2ETests;

[Collection("Playwright")]
public class AdminTests
{
    private readonly PlaywrightFixture _fixture;

    public AdminTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UsersPage_ShouldDisplayUsersTable()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/admin/users");

        await Assertions.Expect(page.Locator("h3")).ToContainTextAsync("Uzytkownicy");
        await Assertions.Expect(page.Locator("table.table")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("th").First).ToContainTextAsync("Imie i nazwisko");

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task UsersPage_ShouldHaveAddUserButton()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/admin/users");

        var addButton = page.Locator("button:has-text('Dodaj uzytkownika')");
        await Assertions.Expect(addButton).ToBeVisibleAsync();

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task UsersPage_NonAdmin_ShouldBeDenied()
    {
        var page = await _fixture.CreateAuthenticatedPageAsync("osdp@pku.pl", "admin123");

        await page.GotoAsync($"{_fixture.BaseUrl}/admin/users");

        // Non-admin should not see the users table
        await Assertions.Expect(page).Not.ToHaveURLAsync(new System.Text.RegularExpressions.Regex("admin/users$"));

        await page.Context.DisposeAsync();
    }
}
