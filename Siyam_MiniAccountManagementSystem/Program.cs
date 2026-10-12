using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Siyam_MiniAccountManagementSystem.Data;
using Siyam_MiniAccountManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Render-এর জন্য InMemory Database কনফিগারেশন
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("MiniAccountInMemoryDb"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

builder.Services.AddScoped<ChartOfAccountsService>();

builder.Services.AddScoped<VoucherService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// InMemory Database-এর জন্য টেবিল এবং ডিফল্ট রোল/ইউজার তৈরি নিশ্চিত করা
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        string[] roleNames = { "Admin", "Accountant", "Viewer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminUser = await userManager.FindByEmailAsync("siyam@gmail.com");
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = "siyam@gmail.com",
                Email = "siyam@gmail.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Siyam@123"); 
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.MapRazorPages();

app.Run();
