using Construccion.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Esta linea de codigo es para la autenticacion de las coockies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // Se redirige si no esta autenticado
        options.AccessDeniedPath = "/Login/AccesoDegenado"; // Si no tiene permiso, se redirige a la vista accesodenegado
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin")); // Este es una politica para de usuario para los que tienen el rol admin
});

// Add services to the container.
builder.Services.AddControllersWithViews();
//Se genera una instancia para la cadena de conexion
builder.Services.AddDbContext<ConstruccionContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

var app = builder.Build();



    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
