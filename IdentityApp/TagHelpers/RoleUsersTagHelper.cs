using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IdentityApp.TagHelpers;

[HtmlTargetElement("td",Attributes = "asp-role-users")]
public class RoleUsersTagHelper : TagHelper
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    [HtmlAttributeName("asp-role-users")]
    public string? RoleId { get; set; } = null!;
    
    public RoleUsersTagHelper(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var userNames = new List<string>();
        
        
        var role = await _roleManager.FindByIdAsync(RoleId);

        if (role is not null && role.Name is not null)
        {
            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    // nullsa bunu "" geç
                    userNames.Add(user.UserName ?? "");
                }
            }
            // text göndericeksen setcontent
            //output.Content.SetContent(userNames.Count == 0 ? "kullanıcı yok" : string.Join(", ", userNames));
            output.Content.SetContent(userNames.Count == 0 ? "kullanıcı yok" : setHtml(userNames));
        }
        output.Content.SetContent("Kullanıcılar");
    }

    private string setHtml(List<string> userNames)
    {
        var html = "<ul>";
        foreach (var user in userNames)
        {
            html += "<li>" + user + "</li>";
        }

        html += "</ul>";

        return html;
    }
}