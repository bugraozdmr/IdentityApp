using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers;

public class UsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsersController(UserManager<IdentityUser> manager)
    {
        _userManager = manager;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Users";
        return View(_userManager.Users);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Create User";
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser() { UserName = model.Username, Email = model.Email };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("",err.Description);
            }
            
            return View(model);
        }
        
        return View(model);
    }
}