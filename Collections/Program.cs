using Collections.Constants;
using Collections.Data;
using Collections.Extensions;
using Microsoft.AspNetCore.Localization;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppServices(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);


var app = builder.Build();

await app.MigrateDatabaseAsync<ApplicationDbContext>();

if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT");
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
        pattern: "{controller=Home}/{action=Index}/{slug?}");
});

app.Run();
