using AIHelpDesk.WebUI.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;
using AIHelpDesk.WebUI.Data;
using System.Linq;
using MudBlazor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("sql") ?? throw new InvalidOperationException("Connection string 'sql' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services
    .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp => {
    var nav = sp.GetRequiredService<NavigationManager>();
    var httpCtx = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var cookies = httpCtx.Request.Headers["Cookie"].ToString();
    var handler = new HttpClientHandler
    {
        UseCookies = false   // perch� aggiungiamo manualmente l�header
    };
    var client = new HttpClient(handler) { BaseAddress = new Uri(nav.BaseUri) };
    if (!string.IsNullOrEmpty(cookies))
        client.DefaultRequestHeaders.Add("Cookie", cookies);
    return client;
});

builder.Services.AddMudServices();

builder.Services.AddScoped<IFileParserService, FileParserService>();

builder.AddServiceDefaults();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
  db.Database.Migrate();
}

app.MapDefaultEndpoints();

// Seed Identity roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAndAdminAsync(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("it") };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("it")
    .AddSupportedCultures(supportedCultures.Select(c => c.Name).ToArray())
    .AddSupportedUICultures(supportedCultures.Select(c => c.Name).ToArray());
app.UseRequestLocalization(localizationOptions);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Endpoint for user logout
app.MapPost("/auth/logout", async (
    SignInManager<IdentityUser> signInManager,
    [FromQuery] string? returnUrl) =>
{
    await signInManager.SignOutAsync();

    if (!string.IsNullOrEmpty(returnUrl) &&
        Uri.TryCreate(returnUrl, UriKind.Relative, out var uri) &&
        !returnUrl.StartsWith("//"))
    {
        return Results.Ok(new { redirect = returnUrl });
    }

    return Results.Ok(new { redirect = "/" });
})
.RequireAuthorization();

app.Run();
