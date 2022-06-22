using Collections.Data;
using Collections.Models;
using Collections.Services;
using Collections.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }); 

builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(Heroku.GetHerokuConnectionString(builder.Configuration))
);

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 4;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredUniqueChars = 0;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddSingleton<IUploadHandler, UploadHandler>();

builder.Services.AddElasticSearch(builder.Configuration);


var app = builder.Build().MigrateDatabase<ApplicationDbContext>();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    string port = Environment.GetEnvironmentVariable("PORT");
    app.Urls.Add($"http://*:{port}");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(Locales.Languages[0]),
    SupportedCultures = Locales.GetCultures,
    SupportedUICultures = Locales.GetCultures
});

app.Use(async (context, next) =>
{
    Thread.CurrentThread.CurrentCulture = new CultureInfo(context.Request.Cookies["Locale"] ?? Locales.Languages[0]);
    Thread.CurrentThread.CurrentUICulture = new CultureInfo(context.Request.Cookies["Locale"] ?? Locales.Languages[0]);

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapAreaControllerRoute(
        name: "Dashboard",
        areaName: "Dashboard",
        pattern: "dashboard/{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();