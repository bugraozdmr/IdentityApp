using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels;

public class CreateViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";
    
    
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password),ErrorMessage = "Parolalar eşleşmiyor")]
    public string ConfirmPassword { get; set; }
    
    
}