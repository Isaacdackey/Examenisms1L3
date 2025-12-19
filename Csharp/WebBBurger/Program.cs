using Microsoft.EntityFrameworkCore;
using WebBBurger.Data;
using WebBBurger.Services;
using WebBBurger.Services.Impl;
using WebBBurger.Repositories;
using WebBBurger.Repositories.Impl;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IComplementRepository, ComplementRepository>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseStaticFiles();

app.Use(async (context, next) =>
{
    var startTime = DateTime.Now;
    Console.WriteLine($"=== NOUVELLE REQUÊTE ===");
    Console.WriteLine($"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
    Console.WriteLine($"User: {context.User?.Identity?.Name ?? "Anonymous"}");
    Console.WriteLine($"Start: {startTime:HH:mm:ss.fff}");
    
    await next();
    
    var endTime = DateTime.Now;
    var duration = endTime - startTime;
    Console.WriteLine($"Response: {context.Response.StatusCode}");
    Console.WriteLine($"Duration: {duration.TotalMilliseconds}ms");
    Console.WriteLine($"End: {endTime:HH:mm:ss.fff}");
    Console.WriteLine($"=== FIN REQUÊTE ===\n");
});
app.UseRouting();


app.UseSession();




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            Console.WriteLine("===Connexion à la base de données réussie===");
        }
        else
        {
            Console.WriteLine("Impossible de se connecter à la base de données");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erreur lors du test de connexion: {ex.Message}");
}

Console.WriteLine($"Application démarrée sur: http://localhost:5170");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");

app.Run();