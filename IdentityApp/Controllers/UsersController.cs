using System.Web;
using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Controllers;

// buraya koymadım bilerek Create sorunlu
public class UsersController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IEmailSender _emailSender;

    public UsersController(UserManager<AppUser> manager, 
        RoleManager<AppRole> roleManager,
        IEmailSender emailSender)
    {
        _userManager = manager;
        _roleManager = roleManager;
        _emailSender = emailSender;
    }

    [Authorize(Roles = "Admin")]
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
    
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateViewModel model)
    {
        // bunu taşı bence
        if (ModelState.IsValid)
        {
            var user = new AppUser() { UserName = model.Username, Email = model.Email , FullName = model.FullName};

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // bu işlemleri direkt account controllera yazabilirdik dağınık olmazdı ...
                string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                // token encode edilmezse çalışmaz
                var encoded_token = HttpUtility.UrlEncode(token);
                var url = Url.Action("ConfirmEmail", "Account",new {id=user.Id,token=encoded_token});
                
                // email

                await _emailSender.SendEmailAsync(user.Email, "Hesap onayı",
                    $"Lütfen email hesabınızı onaylamak için maile " +
                    $"<a href='https://localhost:7284{url}'>tıklayın</a>");
                
                
                TempData["message"] = "email hesabınızdaki onay mailine tıklayın.";
                return RedirectToAction("Login","Account");
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("",err.Description);
            }
            
            return View(model);
        }
        
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit([FromRoute]string id)
    {
        if (id is null)
        {
            return RedirectToAction("Index");
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user is not null)
        {
            ViewBag.Roles = await _roleManager.Roles.Select(i => i.Name).ToListAsync();
            
            return View(new EditViewModel()
            {
                Username = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Id = user.Id,
                SelectedRoles = await _userManager.GetRolesAsync(user)
            });
        }

        return RedirectToAction("Index");
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute]string id,[FromForm] EditViewModel model)
    {
        if (!model.Id.Equals(id))
        {
            ModelState.AddModelError("","Id'ler eşleşmiyor");
            return View(model);
        }
        
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is not null)
            {
                user.Email = model.Email;
                user.FullName = model.FullName;
                user.UserName = model.Username;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded && !string.IsNullOrEmpty(model.Password))
                {
                    // eğer parola değişmek istemişse
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, model.Password);
                }
                
                
                if (result.Succeeded)
                {
                    await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                    if (model.SelectedRoles != null)
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);   
                    }
                    return RedirectToAction("Index");
                }

                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("",err.Description);
                }

                return View(model);
            }
        }

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromForm] string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        
        if (id is null || user is null)
        {
            return RedirectToAction("Index","Home");
        }

        await _userManager.DeleteAsync(user);
        
        return RedirectToAction("Index");
    }
}