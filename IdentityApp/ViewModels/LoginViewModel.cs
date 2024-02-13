using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels;

public class LoginViewModel
{
    [DataType(DataType.Text)] 
    [Required(ErrorMessage = "Kullanıcı adı gerekli")]
    public string? Username { get; set; } = null;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Şifre gerekli")]
    public string? Password { get; set; } = null;

    public bool RememberMe { get; set; } = true;
}