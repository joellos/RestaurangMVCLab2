
using RestaurangMVCLab2.Services;


var builder = WebApplication.CreateBuilder(args);

// Lägg till MVC services
builder.Services.AddControllersWithViews();

// Registrera HttpClient och MenuService
// Program.cs
builder.Services.AddHttpClient<MenuService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135/api/");
});

builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135/api/");
});

builder.Services.AddHttpClient<BookingService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135/api/");
});

builder.Services.AddHttpClient<TableService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135/api/");
});


// Lägg till session support (för att hålla JWT token)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Lägg till session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();