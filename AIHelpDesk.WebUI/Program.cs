using AIHelpDesk.Application;
using AIHelpDesk.Application.AI;
using AIHelpDesk.Infrastructure;
using AIHelpDesk.Infrastructure.Data;
using AIHelpDesk.Infrastructure.Model;
using AIHelpDesk.WebUI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.SemanticKernel;
using MudBlazor.Services;
using OpenAI.Chat;
using System.Globalization;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurazioni di base
ConfigureDatabase(builder);
ConfigureIdentity(builder);
ConfigureLocalization(builder);

// 2. Registrazione servizi HTTP, UI e terze parti
ConfigureHttpClient(builder);
builder.Services.AddControllers();
ConfigureBlazorComponents(builder);
builder.Services.AddMudServices();

// 3. Registrazione servizi applicativi
ConfigureApplicationServices(builder);

var app = builder.Build();

// 4. Migrazioni e seeding
ApplyMigrations(app);
await SeedRolesAndAdmin(app);

// 5. Pipeline HTTP
ConfigureRequestPipeline(app);

app.Run();


// ---------------------
// Metodi di estensione
// ---------------------

static void ConfigureDatabase(WebApplicationBuilder builder)
{
    var conn = builder.Configuration.GetConnectionString("sql")
               ?? throw new InvalidOperationException("Connection string 'sql' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(opts =>
        opts.UseSqlServer(conn));
}

static void ConfigureIdentity(WebApplicationBuilder builder)
{
    builder.Services
        .AddDefaultIdentity<IdentityUser>(opts => opts.SignIn.RequireConfirmedAccount = true)
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();
}

static void ConfigureLocalization(WebApplicationBuilder builder)
{
    builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");
}

static void ConfigureHttpClient(WebApplicationBuilder builder)
{
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped(sp =>
    {
        var nav = sp.GetRequiredService<NavigationManager>();
        var httpCtx = sp.GetRequiredService<IHttpContextAccessor>().HttpContext!;
        var cookies = httpCtx.Request.Headers["Cookie"].ToString();

        var handler = new HttpClientHandler { UseCookies = false };
        var client = new HttpClient(handler) { BaseAddress = new Uri(nav.BaseUri) };
        if (!string.IsNullOrEmpty(cookies))
            client.DefaultRequestHeaders.Add("Cookie", cookies);
        return client;
    });
}

static void ConfigureBlazorComponents(WebApplicationBuilder builder)
{
    builder.Services
        .AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddCircuitOptions(opts => opts.DetailedErrors = true);
}

static void ConfigureApplicationServices(WebApplicationBuilder builder)
{
    // Parser, chunker e embedding
    builder.Services.AddScoped<IFileParserService, FileParserService>();
    builder.Services.AddSingleton<ITextChunkerService, TextChunkerService>();
    builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI"));
    builder.Services.AddScoped<IEmbeddingService, OpenAiEmbeddingService>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();

    // Vettoriale
    builder.AddQdrantClient("qdrant");
    builder.Services.AddScoped<IVectorStoreService, QdrantVectorStoreService>();

    // OpenAI ChatClient singleton
    builder.Services.AddSingleton<ChatClient>(sp =>
    {
        var cfg = builder.Configuration;
        var apiKey = cfg["OpenAI:ApiKey"]!;
        var model = cfg.GetValue<string>("OpenAI:Model", "gpt-4");
        return new ChatClient(model: model, apiKey: apiKey);
    });

    // Servizi di dominio
    builder.Services.AddScoped<DocumentIndexService>();
    builder.Services.AddScoped<IChatService, ChatService>();
    builder.Services.AddScoped<IChatLogService, ChatLogService>();
    builder.Services.AddScoped<IPromptSettingsService, PromptSettingsService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();

    // Tenant (anche se non usi il tenant, puoi tenere la config o rimuoverla)
    builder.Services.Configure<TenantInfoOptions>(
        builder.Configuration.GetSection("TenantInfo"));

    // Default di terze parti
    builder.AddServiceDefaults();
}

static void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

static async Task SeedRolesAndAdmin(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAndAdminAsync(services);
}

static void ConfigureRequestPipeline(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Localization
    var cultures = new[] { "en", "it" };
    var locOpts = new RequestLocalizationOptions()
        .SetDefaultCulture("it")
        .AddSupportedCultures(cultures)
        .AddSupportedUICultures(cultures);
    app.UseRequestLocalization(locOpts);

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapRazorPages();

    app.UseAntiforgery();

    app.MapRazorComponents<App>()
       .AddInteractiveServerRenderMode();

    // Logout endpoint
    app.MapPost("/auth/logout", async (
        SignInManager<IdentityUser> signInManager,
        [FromQuery] string? returnUrl) =>
    {
        await signInManager.SignOutAsync();
        if (!string.IsNullOrEmpty(returnUrl) &&
            Uri.TryCreate(returnUrl, UriKind.Relative, out var uri) &&
            !returnUrl.StartsWith("//"))
            return Results.Ok(new { redirect = returnUrl });

        return Results.Ok(new { redirect = "/" });
    })
    .RequireAuthorization();
}
