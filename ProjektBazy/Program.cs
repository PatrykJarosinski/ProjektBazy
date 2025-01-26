using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjektBazy.Data;
using ProjektBazy.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodaj us�ugi dla widok�w Razor
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Dodaj us�ugi Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Dodaj us�ugi autoryzacji
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("admin"));
});

// Dodaj DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodaj HttpClient jako us�ug�
builder.Services.AddHttpClient();

var app = builder.Build();

// U�yj middleware routingu
app.UseRouting();

// U�yj middleware autoryzacji i uwierzytelniania
app.UseAuthentication();
app.UseAuthorization();

// Dodaj obs�ug� strony "Brak uprawnie�"
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 403) // Brak uprawnie�
    {
        context.HttpContext.Response.Redirect("/Shared/AccessDenied");
    }
});

// Dodaj role (admin/user) podczas uruchomienia aplikacji
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "admin", "user" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Dodaj u�ytkownik�w (admin/user) podczas uruchomienia aplikacji
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Utw�rz u�ytkownika admin
    string email = "admin@example.com";
    string password = "Admin123!";

    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Admin",
            LastName = "Admin"
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, "admin");
    }

    // Utw�rz u�ytkownika user
    email = "user@example.com";
    password = "User123!";

    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "User",
            LastName = "User"
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, "user");
    }
}

// Mapowanie tras
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();