
using RestaurangMVCLab2.Services;


var builder = WebApplication.CreateBuilder(args);

// L�gg till MVC services
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


// L�gg till session support (f�r att h�lla JWT token)
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

// L�gg till session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();