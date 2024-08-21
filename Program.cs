using Microsoft.EntityFrameworkCore;
using BeanScene.Data;
using Microsoft.AspNetCore.Identity;
using BeanScene.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<BSDBContext>();

builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<EmailOuterService>();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BSDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RetailDBConnection") ?? throw new InvalidOperationException("Connection string 'BeanSceneContext' not found.")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seeding manager user 
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roles = new[] { "Member", "Staff", "Manager" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        //this is where you can add your own 'master' account which can edit or control other users.
        //this is the company's master account, it's always there
        string email = "beanscene@management.com";
        string password = "Management1234!";
        string firstName = "BeanScene";
        string lastName = "Management";
        string phoneNumber = "0400000000";

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            //incredibly important for the User: ApplicationUser and Identity link, never, ever, EVER remove.
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Manager");
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("role", "Manager"));
            }
        }
    }
    catch (Exception ex) //if this happens, you did something wrong.
    {
        logger.LogError(ex, "An error occurred during initialization.");
    }
}

app.Run();
