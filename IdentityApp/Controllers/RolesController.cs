using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers;

public class RolesController : Controller
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var model = _roleManager.Roles;
        
        return View(model);
    }
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] AppRole model)
    {
        if (ModelState.IsValid)
        {
            var result = await _roleManager.CreateAsync(model);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("",err.Description);
            }
        }
        return View(model);
    }

    public async Task<IActionResult> Edit([FromRoute]string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role != null && role.Name != null)
        {
            ViewBag.Users = await _userManager.GetUsersInRoleAsync(role.Name);
            return View(role);
        }

        return RedirectToAction("Index");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AppRole model)
    {
        if (ModelState.IsValid)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);

            if (role != null && role.Name != null)
            {
                role.Name = model.Name;

                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("",err.Description);
                }

                // eğer hata alırsa boş gitmesin userS tekrar giderken
                ViewBag.Users = await _userManager.GetUsersInRoleAsync(role.Name);
            }
        }

        return View(model);
    }
}