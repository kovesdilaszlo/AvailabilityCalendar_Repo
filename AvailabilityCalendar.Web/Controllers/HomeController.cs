using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvailabilityCalendar.Web.Controllers;

/// <summary>
/// Handles basic navigation for authenticated and anonymous users.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Redirects authenticated users to the calendar.
    /// </summary>
    [Authorize]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Calendar");
    }

    /// <summary>
    /// Redirects anonymous users to the login page.
    /// </summary>
    [AllowAnonymous]
    public IActionResult Landing()
    {
        return RedirectToAction("Login", "Account");
    }
}