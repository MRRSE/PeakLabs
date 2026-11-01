using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Data;
using PeakLabs.Models.Identity;
using PeakLabs.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DashboardActivityLogger>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing. Configure it using user secrets or an environment variable.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentity<PeakLabsUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Auth/Login";
    options.AccessDeniedPath = "/Admin/Auth/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(3);
    options.SlidingExpiration = false;
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(5);
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("contact-form", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PeakLabsUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var initialAdmin = builder.Configuration.GetSection("InitialAdmin");
    var adminUserName = initialAdmin["UserName"]?.Trim();
    var adminEmail = initialAdmin["Email"]?.Trim();
    var adminPassword = initialAdmin["Password"];

    if (await userManager.Users.AnyAsync())
    {
        if (!string.IsNullOrEmpty(adminPassword))
        {
            app.Logger.LogWarning("InitialAdmin settings are ignored because the database already contains users. Remove these settings.");
        }
    }
    else if (!string.IsNullOrWhiteSpace(adminUserName)
        && !string.IsNullOrWhiteSpace(adminEmail)
        && !string.IsNullOrWhiteSpace(adminPassword))
    {
        if (!await roleManager.RoleExistsAsync(DashboardRoles.SuperAdmin))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(DashboardRoles.SuperAdmin));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Could not create the initial administrator role.");
            }
        }

        var admin = new PeakLabsUser
        {
            UserName = adminUserName,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "مدیر",
            LastName = "اصلی",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var createResult = await userManager.CreateAsync(admin, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not create the initial administrator: {string.Join("; ", createResult.Errors.Select(error => error.Code))}");
        }

        var addRoleResult = await userManager.AddToRoleAsync(admin, DashboardRoles.SuperAdmin);
        if (!addRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(admin);
            throw new InvalidOperationException("Could not assign the administrator role to the initial account.");
        }

        app.Logger.LogInformation("Initial administrator account was created. Remove InitialAdmin settings from the host.");
    }
    else if (!await userManager.Users.AnyAsync())
    {
        app.Logger.LogWarning("No users exist and InitialAdmin settings are missing. Configure InitialAdmin:UserName, InitialAdmin:Email, and InitialAdmin:Password to create the first administrator.");
    }
}

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
app.UseRateLimiter();
app.UseMiddleware<SiteTrafficMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
