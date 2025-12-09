using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebUI.Data;
using Domain.Interfaces;
using DataAccess.Context;
using DataAccess.Repositories;
using DataAccess.Factories;

var builder = WebApplication.CreateBuilder(args);

// ----------------- CONNECTION STRING -----------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ----------------- MVC -----------------
builder.Services.AddControllersWithViews();

// ----------------- CACHING -----------------
builder.Services.AddMemoryCache();

// ----------------- OUR REPOSITORIES -----------------
// In-memory repo (AA2.3 temporary storage)
builder.Services.AddKeyedScoped<ItemsRepository, ItemsInMemoryRepository>("memory");


// Concrete ItemsDbRepository (ItemsController asks for this directly)
builder.Services.AddScoped<ItemsDbRepository>();

// ----------------- OUR FACTORIES -----------------
builder.Services.AddScoped<ImportItemFactory>();

var app = builder.Build();

// ----------------- SEED HARD-CODED ADMIN USER -----------------
SeedAdminUserAsync(app.Services).GetAwaiter().GetResult();

// ----------------- MIDDLEWARE PIPELINE -----------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

static async Task SeedAdminUserAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();

    // 1) make sure Identity DB schema (AspNetUsers, etc.) exists
    var identityContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await identityContext.Database.MigrateAsync();

    // 2) then we can safely use UserManager
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 🔐 HARD-CODED SITE ADMIN CREDENTIALS
    var adminEmail = "admin@gmail.com";
    var adminPassword = "Admin123!";

    var existing = await userManager.FindByEmailAsync(adminEmail);

    if (existing == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, adminPassword);
    }
}
