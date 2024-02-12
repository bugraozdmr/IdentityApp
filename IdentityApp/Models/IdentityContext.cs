using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Models;

// string olarak id alsın
public class IdentityContext : IdentityDbContext<AppUser,AppRole,string>
{
    public IdentityContext(DbContextOptions options) : base(options)
    {

    }
    
    
}