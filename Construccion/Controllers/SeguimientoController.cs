using Construccion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Construccion.Controllers
{
    [Authorize(Roles = "Admin, Empleado")]
    public class SeguimientoController : Controller
    {
        private readonly ConstruccionContext _context;

        public SeguimientoController(ConstruccionContext context) //Este controlador gestiona acciones relacionado con Seguimiento
        {
            _context = context;
        }

        public async Task<IActionResult> SeleccionarObra(string filtro, int pageNumber = 1) //Este es un metodo en el que se muestra la vista para seleccionar las obras
        {
            int pageSize = 5;
            var obras = _context.Obras.AsQueryable(); // Se realiza una consulta para obtener las obras de la base de datos

            if (!string.IsNullOrEmpty(filtro))
            {
                obras = obras.Where(o => o.NombreObra!.Contains(filtro) || o.Cliente!.Contains(filtro));
            }

            var totalObras = await obras.CountAsync();

           
            ViewData["TotalPaginas"] = (int)Math.Ceiling(totalObras / (double)pageSize);
            ViewData["PaginaActual"] = pageNumber;

            var obrasPaginated = await obras.OrderBy(o => o.NombreObra)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Filtro"] = filtro;

            return View(obrasPaginated);
        }


        public IActionResult Hitos(int idObra) // Este es un metodo en el que se muestra la vista de los hitos que se han ingresado
        {
            var obra = _context.Obras
                               .Include(o => o.Partidas)  
                               .FirstOrDefault(o => o.IdObra == idObra); //Aqui se realiza la consult de obras en la que se incluye las partida, teniendo como default el Id de la obra

            if (obra == null)
            {
                return NotFound();
            }

            
            var hitos = _context.Hitos.Where(h => h.IdObra == idObra).ToList(); // Aqui se realiza la consulta a hitos para buscar la obra

            
            ViewBag.Partidas = new SelectList(obra.Partidas, "IdPartida", "Nombre"); // Aqui se cargan las partidas que se encuentran ingresada en la obra
            // Aqui se los hitos de obra y se crea una lista desplegable de las partidas
            ViewBag.IdObra = idObra;
            ViewBag.NombreObra = obra.NombreObra;
            ViewBag.Cliente = obra.Cliente;

            
            var porcentajeAvance = CalcularPorcentajeAvance(idObra);
            ViewBag.PorcentajeAvance = porcentajeAvance; // Aqui muestra la información del porcentaje a la vista

            return View(hitos); // Aqui pasa la información a la vista 
        }
        public IActionResult CrearHito(int idObra) // Aqui se muestra la vista con el formulario para crear el hito
        {
            
            var obra = _context.Obras
                               .Include(o => o.Partidas)
                               .FirstOrDefault(o => o.IdObra == idObra); // Aqui realiza la consulta en obras en la que se incluye las partidas y que es por default el id de la obra correspondiente

            if (obra == null)
            {
                return NotFound();
            }

            
            var partidas = obra.Partidas;

            
            if (partidas == null || !partidas.Any()) // Este es una sentencia en el que se mostrará las partidas que se encuentran en la obra
            {
                ViewBag.Partidas = new SelectList(Enumerable.Empty<SelectListItem>());
            }
            else
            {
                ViewBag.Partidas = new SelectList(partidas, "IdPartida", "Nombre");
            }

            ViewBag.IdObra = idObra;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearHito(Hito hito)
        {
            if (!ModelState.IsValid) // Aqui se actualiza la vista Hito en el caso de que el modelo sea válido. De esta forma se valida que el Id del hito en la URL sea igual con el del modelo
            {
                ViewBag.IdObra = hito.IdObra;
                
                var partidas = _context.Partidas.Where(p => p.IdObra == hito.IdObra).ToList();
                ViewBag.Partidas = new SelectList(partidas, "IdPartida", "Nombre");
                return View(hito);
            }

            _context.Hitos.Add(hito); // Se agrega al contexto de hitos en la base de datos
            _context.SaveChanges();

            TempData["Mensaje"] = "Hito creado exitosamente."; // se genera el mensaje que el hito ha sido creado
            return RedirectToAction("Hitos", new { idObra = hito.IdObra }); 
        }

        [HttpPost]
        public IActionResult GuardarAvance(int idObra, List<int> hitosFinalizados) // Aqui al seleccionar el hito en el checkbox aumenta el porcentaje de la obra
        {
            
            var hitos = _context.Hitos.Where(h => h.IdObra == idObra).ToList(); // Aqui se realiza la consulta a hitos en el id de la obra que se quiere realizar en el id de la obra

            if (hitosFinalizados == null || !hitosFinalizados.Any()) // Se hace una sentencia si es que el hito se encuentra finalizado
            {
                foreach (var hito in hitos) 
                {
                    hito.Finalizado = false;
                }
            }
            else
            {
                foreach (var hito in hitos) // Aqui recibe el id del hito que se encuentra finalizado, y se actualizado
                {
                    hito.Finalizado = hitosFinalizados.Contains(hito.IdHito);
                }
            }

            _context.SaveChanges(); 

            TempData["Mensaje"] = "El avance ha sido guardado exitosamente.";
            return RedirectToAction("Hitos", new { idObra });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SeleccionarHito(int idObra) // Aqui muestra la vista para seleccionar el hitop que se eliminará
        {
            var obra = _context.Obras.FirstOrDefault(o => o.IdObra == idObra);
            if (obra == null)
            {
                return NotFound();
            }

            var hitos = _context.Hitos
                                .Where(h => h.IdObra == idObra)
                                .Select(h => new { h.IdHito, h.NombreHito })
                                .ToList();

            ViewBag.IdObra = idObra;
            ViewBag.Hitos = new SelectList(hitos, "IdHito", "NombreHito");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarEliminarHito(int idHito, int idObra) // Aqui se muestra la vista para la confirmar la eliminación del hito
        {
            var hito = _context.Hitos.FirstOrDefault(h => h.IdHito == idHito);
            if (hito == null)
            {
                return NotFound();
            }

            ViewBag.IdObra = idObra;
            ViewBag.NombreHito = hito.NombreHito;

            return View(hito);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarHito(int idHito, int idObra)
        {
            var hito = _context.Hitos.FirstOrDefault(h => h.IdHito == idHito);
            if (hito != null)
            {
                _context.Hitos.Remove(hito);
                _context.SaveChanges();
                TempData["Mensaje"] = "Hito eliminado exitosamente.";
            }

            return RedirectToAction("Hitos", new { idObra });
        }

        private decimal CalcularPorcentajeAvance(int idObra) // Aqui se realiza el calculo del porcentaje de avance de acuerdo a al cantidad de hitos que hay
        {
            var hitos = _context.Hitos.Where(h => h.IdObra == idObra).ToList();
            if (hitos.Count == 0)
            {
                return 0; 
            }

            int totalHitos = hitos.Count;
            int hitosFinalizados = hitos.Count(h => h.Finalizado);

            return Math.Round((hitosFinalizados / (decimal)totalHitos) * 100,2); // Aqui se realiza el calculo del porcentaje como el número de hitos finalizados dividido entre el total, multiplicando por 100, en el que devuelve el porcentaje redondea en dos decimales
        }
    }
}


