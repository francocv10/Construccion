using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Construccion.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

public class LoginController : Controller // Aqui se manejará la lógica de inicio sesión
{
    private readonly ConstruccionContext _context; // Este es el contexto de la base de datos que se utilizará para que pueda interactuar las tablas

    public LoginController(ConstruccionContext context) 
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index() // Aqui retorna la vista del inicio sesión
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string identificador, string clave) 
    {
        var usuario = await _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.Identificador == identificador && u.Clave == clave); // En esta variable realiza la consulta a la base de datos, para que el usuario y la contraseña coincidan, en la que se incluye el rol

        if (usuario != null)
        {
            var claims = new List<Claim> // Se crean los claim, que son datos de seguridad para identificar el nombre de usuario y la contraseña
            {
                new Claim(ClaimTypes.Name, usuario.Identificador),
                new Claim(ClaimTypes.Role, usuario.IdRolNavigation!.NombreRol),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // Aqui representa la identidad del usuario autenticado basado en los claims
            var authProperties = new AuthenticationProperties { IsPersistent = true }; // Aqui se configura la propiedad de autenticacion, en este caso que la sesión sea persistente

           
            var acceso = new UsuarioAcceso // Se registra en la base de datos la hora, fecha y la direcciónIP del usuario
            {
                IdUsuario = usuario.IdUsuario,
                FechaHoraAcceso = DateTime.Now,
                DireccionIP = HttpContext.Connection.RemoteIpAddress?.ToString() 
            };
            _context.UsuarioAccesos.Add(acceso);
            await _context.SaveChangesAsync();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties); // Aqui se inicia sesión utilizando las cookies

            return RedirectToAction("Index", "Home"); // Aqui redirecciona a la vista Index
        }

        ViewBag.ErrorMessage = "El usuario o la contraseña no son correctas, por favor ingresar nuevamente los datos"; // En el caso que se equivoque de ingresar los datos, arroja un mensaje de error
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Salir() 
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // aqui se cierra la sesión eliminando la cookie de autenticación
        return RedirectToAction("Index", "Login"); // redirecciona a la página de inicio sesión
    }

    public IActionResult AccesoDegenado()
    {
        return View();
    }
}
