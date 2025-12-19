using Microsoft.EntityFrameworkCore;
using WebBBurger.Data;
using WebBBurger.Services;
using WebBBurger.Services.Impl;
using WebBBurger.Repositories;
using WebBBurger.Repositories.Impl;

var builder = WebApplication.CreateBuilder(args);


var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");


builder.Services.AddControllersWithViews();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("ConnectionStrings:DefaultConnection est vide ou non configurée");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "BrasilBurger.Session";
});

builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICommandeRepository, CommandeRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IZoneLivraisonRepository, ZoneLivraisonRepository>();
builder.Services.AddScoped<IComplementRepository, ComplementRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapGet("/health", () => Results.Ok("OK"));


using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var canConnect = await dbContext.Database.CanConnectAsync();

        if (canConnect)
        {
            Console.WriteLine("Connexion à la base de données réussie");
        }
        else
        {
            Console.WriteLine("Impossible de se connecter à la base de données");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERREUR BD AU DÉMARRAGE: {ex.Message}");
        throw;
    }
}


Console.WriteLine("=======================================");
Console.WriteLine("   BRASIL BURGER - APPLICATION START   ");
Console.WriteLine($"   Environment : {app.Environment.EnvironmentName}");
Console.WriteLine($"   Port        : {port}");
Console.WriteLine($"   Démarré le  : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine("=======================================");

app.Run();
