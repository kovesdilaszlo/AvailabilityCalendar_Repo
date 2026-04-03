using AvailabilityCalendar.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityCalendar.Web.Controllers;

/// <summary>
/// Handles authentication workflows for the web UI.
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// Creates a new account controller instance.
    /// </summary>
    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    /// <summary>
    /// Displays the login page.
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    /// <summary>
    /// Processes a login attempt using email and password.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Email és jelszó megadása kötelező.");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(
            email.Trim(),
            password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Hibás bejelentkezési adatok.");
            return View();
        }

        return RedirectToAction("Index", "Calendar");
    }

    /// <summary>
    /// Signs out the currently authenticated user.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}