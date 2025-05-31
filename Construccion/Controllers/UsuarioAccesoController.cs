using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Construccion.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

public class UsuarioAccesoController : Controller
{
    private readonly ConstruccionContext _context;

    public UsuarioAccesoController(ConstruccionContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var accesos = await _context.UsuarioAccesos // Aqui se almacenan los registros de los usuarios ingresados
            .Include(ua => ua.Usuario) // Aqui se incluye la información que se encuentra relacionado
            .OrderByDescending(ua => ua.FechaHoraAcceso) // Aqui se realizan los registros de la hora y fecha de ingreso de forma descendente
            .ToListAsync(); // Aqui se convierte el listado en una lista y ejecuta la consulta de manera asíncrona

        var totalAccesos = accesos.Count(); // Esta es la cantidad de accesos registrados
        var totalPages = (int)Math.Ceiling((double)totalAccesos / pageSize); // Aqui se realiza el calculo de total de páginas

        var accesosPaged = accesos.Skip((page - 1) * pageSize).Take(pageSize).ToList(); // Aqui se omite y se toma los registro de la pagina actual y anterior

       
        ViewData["PaginaActual"] = page; //Página actual
        ViewData["TotalPaginas"] = totalPages; //Es la cantidad total de páginas

        return View(accesosPaged); //Redirecciona a la vista 
    }

}

