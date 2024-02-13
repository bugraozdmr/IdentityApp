
using System.Web;
using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace IdentityApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public AccountController(UserManager<AppUser> manager
        , RoleManager<AppRole> roleManager
        , SignInManager<AppUser> signInManager, 
        IEmailSender emailSender)
    {
        _userManager = manager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }
    
    
    public IActionResult Login()
    {
        // burda e mail yada username ile giriş yapmaya çalışanları anlaması sağlanıp öyle bir giriş sağlanabilir -- @ içeriyorsa email gibi
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user is not null)
            {
                await _signInManager.SignOutAsync();
                
                
                // 3. eleman : hatırlasın mı - 4. eleman : Lockout parametresi
                var result = await _signInManager.PasswordSignInAsync(user, model.Password,model.RememberMe,true);

                if (result.Succeeded)
                {
                    // sıfırlamalar -- lockout ile hatalı girişler
                    await _userManager.ResetAccessFailedCountAsync(user);
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                    var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                    
                    ModelState.AddModelError("",$"Hesabınız kilitlendi, Lütfen {timeLeft.Minutes} dk sonra bekleyiniz.");
                }
                else
                {
                    // result.NotAuthorized dönüyor galiba ondan else'de
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("","Hesabınızı onaylayınız.");
                        return View(model);
                    }
                    
                    // kullanıcı var bulundu ancak parolası yanlış -- ben olsam bu bilgiyi vermem
                    ModelState.AddModelError("",$"Parolanız hatalı.");    
                }
            }
            else
            {
                ModelState.AddModelError("",$"Kullanıcı adı ya da şifre hatalı.");
            }
        }
        
        return View(model);
    }
    
    // Create işleminde mail gönderme yapılıyor -- usersController

    public async Task<IActionResult> ConfirmEmail(string id, string token)
    {
        if (id == null || token == null)
        {
            TempData["message"] = "Geçersiz Token";
            return View();
        }
        
        var user = await _userManager.FindByIdAsync(id);

        if (user is not null)
        {
            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user,decodedToken);
            
            if (result.Succeeded)
            {
                TempData["message"] = "Hesabınız onaylandı";
                return View();
            }

            TempData["message"] = "Bir şeyler ters gitti.";
            return View();
        }
        
        
        TempData["message"] = "Kullanıcı bulunamadı";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index","Home");
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword([FromForm]string Email)
    {
        if (string.IsNullOrEmpty(Email))
        {
            //ModelState.AddModelError("","Mail adresi boş olamaz."); -- kendi kontrolu var zaten -- o belki disable edilebilir -- ya da yenisini kendin yazarsın
            return View();
        }

        var user = await _userManager.FindByEmailAsync(Email);

        if (user is null)
        {
            ModelState.AddModelError("","Bu mail sistemde bulunmuyor.");
            return View();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        var encoded_token = HttpUtility.UrlEncode(token);
        
        var url = Url.Action("ResetPassword", "Account",new {id=user.Id,token=encoded_token});
                
        // email

        await _emailSender.SendEmailAsync(user.Email, "Parola Sıfırlama",
            $"Şifrenizi değiştirmek için " +
            $"<strong><a href='https://localhost:7284{url}'>tıklayın</a></strong>");

        TempData["message"] = "eposta adresinize gönderilen link ile şifrenizi sıfırlayabilirsiniz.";
        return View();
    }
    
    
    
    public IActionResult ResetPassword(string id,string token)
    {
        if (id == null || token == null)
        {
            TempData["message"] = "değerler boş";
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordModel() { Token = token ,Id = id};
        
        return View(model);
        
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordModel model)
    {
        if (ModelState.IsValid)
        {
            // açık olmasın diye ben verdim
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
            {
                TempData["message"] = "Bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
            
            // Token url decode edilmezse patlar
            var decodedToken = HttpUtility.UrlDecode(model.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken,model.Password);

            if (result.Succeeded)
            {
                TempData["message"] = "Şifre değiştirildi.";
                return RedirectToAction("Login");
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("",err.Description);
            }
        }
        
        return View(model);
        
    }
    
    public async Task<IActionResult> AccessDenied()
    {
        return View();
    }
}