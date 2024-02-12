using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels;

public class LoginViewModel
{
    [DataType(DataType.Text)] 
    public string? Username { get; set; } = null;

    [DataType(DataType.Password)] 
    public string? Password { get; set; } = null;

    public bool RememberMe { get; set; } = true;
}