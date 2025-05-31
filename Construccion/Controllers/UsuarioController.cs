using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Construccion.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;

namespace Construccion.Controllers
{
    [Authorize(Roles = "Admin")] //Se indica quien tiene la autorizacion para ingresar al apartado gestor de usuarios
    public class UsuarioController : Controller
    {
        private readonly ConstruccionContext _context;

        public UsuarioController(ConstruccionContext context)
        {
            _context = context;
        }


        // GET: Usuario
        public async Task<IActionResult> Index(string Filtro, int pageNumber = 1, int pageSize = 5)
        {
            var usuarios = _context.Usuarios.Include(u => u.IdRolNavigation).AsQueryable();

            if (!String.IsNullOrEmpty(Filtro))
            {
                usuarios = usuarios.Where(u => u.NombreCompleto.Contains(Filtro) || u.Identificador.Contains(Filtro));
            }

            ViewData["Filtro"] = Filtro;

            var totalCount = await usuarios.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var currentPage = pageNumber;

            var paginatedUsuarios = await usuarios
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

           
            ViewData["TotalPaginas"] = totalPages;
            ViewData["PaginaActual"] = currentPage;

            return View(paginatedUsuarios);
        }


        // GET: Usuario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuario/Create
        public IActionResult Create()
        {
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol");
            return View();
        }

        // POST: Usuario/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuario,NombreCompleto,Identificador,Clave,IdRol")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        // POST: Usuario/Edit/5
   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,NombreCompleto,Identificador,Clave,IdRol")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
