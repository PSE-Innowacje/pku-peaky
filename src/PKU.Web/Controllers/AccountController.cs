using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PKU.Application.Interfaces;

namespace PKU.Web.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
    {
        var user = await _authService.AuthenticateAsync(email, password);
        if (user is null)
            return Redirect("/login?error=1");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("ContractorType", string.Join(",", user.ContractorTypes))
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        return Redirect("/");
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}
