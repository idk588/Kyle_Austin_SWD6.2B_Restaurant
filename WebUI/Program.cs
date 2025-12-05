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

// ----------------- DB CONTEXTS -----------------
// Identity DB (for login / register)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// App DB (for restaurants + menu items)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ----------------- IDENTITY -----------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ----------------- MVC -----------------
builder.Services.AddControllersWithViews();

// ----------------- CACHING -----------------
builder.Services.AddMemoryCache();

// ----------------- OUR REPOSITORIES -----------------
// In-memory repo (AA2.3 temporary storage)
builder.Services.AddKeyedScoped<ItemsRepository, ItemsInMemoryRepository>("memory");

// DB repo (final commit to database)
builder.Services.AddKeyedScoped<ItemsRepository, ItemsDbRepository>("db");

// Concrete ItemsDbRepository (ItemsController asks for this directly)
builder.Services.AddScoped<ItemsDbRepository>();

// ----------------- OUR FACTORIES -----------------
builder.Services.AddScoped<ImportItemFactory>();

// ----------------- BUILD APP -----------------
var app = builder.Build();

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
