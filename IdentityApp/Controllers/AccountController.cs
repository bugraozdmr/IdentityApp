using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Login()
    {
        return View();
    }
}