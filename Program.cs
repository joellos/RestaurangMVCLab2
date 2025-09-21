using RestaurangMVCLab2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session configuration för JWT tokens
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Session timeout efter 2 timmar
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "RestaurangMVC.Session";
});

// HTTP Client configuration för API-kommunikation
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135/api/"); // Din API bas-URL
    client.DefaultRequestHeaders.Add("User-Agent", "RestaurangMVC/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<MenuService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135/api/");
    client.DefaultRequestHeaders.Add("User-Agent", "RestaurangMVC/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<TableService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135/api/");
    client.DefaultRequestHeaders.Add("User-Agent", "RestaurangMVC/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<BookingService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135/api/");
    client.DefaultRequestHeaders.Add("User-Agent", "RestaurangMVC/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Set log levels
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// IMPORTANT: Session must be before Authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "menu-category",
    pattern: "meny/{category}",
    defaults: new { controller = "Menu", action = "Index" });

app.MapControllerRoute(
    name: "menu-search",
    pattern: "meny/sok/{search}",
    defaults: new { controller = "Menu", action = "Index" });

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Additional routes för admin-funktionalitet
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Index}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "auth",
    pattern: "auth/{action=Login}",
    defaults: new { controller = "Auth" });

// Logging för startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("?? RestaurangMVC Application starting...");
logger.LogInformation("?? API Base URL configured: https://localhost:7135/api/");
logger.LogInformation("?? Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();