using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("sqlConnection");


// Configuring Database connection
builder.Services.AddDbContextPool<IdentityContext>(opt =>
{
    opt.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 4, 28)));
    // geliştirmede bu olabilir
    opt.EnableSensitiveDataLogging(true);
});

// Configuring Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        
        // ASP net core Identity incele dökümantasyon 5 fail hakkı
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        
        // denedim ...
        // options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789çÇİİöÖşŞüÜ";

    })
    .AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();    // generate tokenları kullanmak için gerekli

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;   // expiration var demek
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>(i => new SmtpEmailSender(
    builder.Configuration["EmailSender:Host"],
    builder.Configuration.GetValue<int>("EmailSender:Port"),
    builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
    builder.Configuration["EmailSender:Username"],
    builder.Configuration["EmailSender:Password"]
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// HiHiHa
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// IdentitySeedData geliyor -- biz extension olarak yazmıştık
IdentitySeedData.IdentityTestUser(app);

app.Run();