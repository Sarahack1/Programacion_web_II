using Microsoft.AspNetCore.Authentication.Cookies;
using Practica02FincaMVC.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 1. Inyectamos nuestro repositorio para que el sistema pueda consultar tu base de datos
builder.Services.AddScoped<UserRepository>();

// 2. Configuramos el núcleo de seguridad y la Cookie de sesión
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        
        // La autenticación vence después de 2 minutos sin una solicitud que renueve la cookie.
        options.ExpireTimeSpan = TimeSpan.FromMinutes(2);
        options.SlidingExpiration = true;
        
        options.Cookie.Name = "Sigila.Auth"; // ¡Adaptado a tu proyecto!
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. Middlewares de seguridad (El orden es obligatorio para que no colapse)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();