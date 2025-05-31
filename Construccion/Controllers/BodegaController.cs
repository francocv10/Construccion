using Construccion.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using ClosedXML.Excel; 
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Layout.Properties;


namespace Construccion.Controllers
{
    [Authorize(Roles = "Admin")] //Atributo que solamente permite ingresar al usuario que tenga el rol admin
    public class BodegaController : Controller //Este controlador gestiona acciones relacionado con Bodega
    {
        private readonly ConstruccionContext _context; // Este es una instancia que permite interactuar con la base de datos 

        public BodegaController(ConstruccionContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string Filtro = "", int page = 1, int pageSize = 5)// Aqui se encuentran los atributos de paginacion, y la filtracion del nombre
        {
            //Este es una variable en el que se realiza una consulta para obtener las bodegas con los insumos asociados
            var bodegasQuery = _context.Bodegas.Include(b => b.Insumos).AsQueryable(); 

            
            if (!string.IsNullOrEmpty(Filtro)) // Aqui se realizan las consultas por el nombre de la bodega
            {
                bodegasQuery = bodegasQuery.Where(b => b.NombreBodega.Contains(Filtro));
                ViewData["Filtro"] = Filtro; // Aqui se guarda el filtro en el ViewData para usarlo en la vista
            }

           // Aqui se realiza el calculo del total de bodegas registrados y el número de páginas que habrá
            var totalBodegas = await bodegasQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBodegas / (double)pageSize);

            
            var bodegas = await bodegasQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // En esta variable con los take y skip, se utiliza para obtener solamente los registros de las bodegas que se encuentran actualmente

            ViewData["PaginaActual"] = page;
            ViewData["TotalPaginas"] = totalPages; // En los ViewsData se Envía información sobre la paginación de la vista
             
            return View(bodegas); // retorna a la vista de la lista de bodegas, para que pueda ser renderizada
        }

        public IActionResult CrearBodega() // Renderiza la vista con el formulario para crear la bodega
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearBodega(Bodega bodega)
        {
            if (ModelState.IsValid) // En esta sentencia valida el modelo para guardar la bodega y redirija a la vista que se encuentra la lista de bodegas
            {                      // En el caso de que haya errores, retorna a la misma vista
                _context.Bodegas.Add(bodega);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(bodega);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarBodega(int id)
        {
            var bodega = await _context.Bodegas.FindAsync(id);
            if (bodega == null) // Aqui se busca por Id la bodega para pasar a la vista con el formulario, en caso de que no exista retorna not found
            {
                return NotFound();
            }
            return View(bodega);
        }

        [HttpPost]
        public async Task<IActionResult> EditarBodega(int id, Bodega bodega)
        {
            if (id != bodega.IdBodega)
            {
                return NotFound();
            }
            // Aqui se actualiza la bodega en el caso de que el modelo sea válido. De esta forma se valida que el Id de la bodega en el URL sea igual con el del modelo
            if (ModelState.IsValid)
            {
                _context.Update(bodega);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(bodega);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmarEliminarBodega(int id)
        {
            var bodega = await _context.Bodegas.FindAsync(id);

            if (bodega == null)
            {
                return NotFound();
            }

           return View(bodega); // Antes de eliminar una bodega se muestra la vista para confirmar la eliminación de la bodega creada
        }

        [HttpPost]
        public async Task<IActionResult> EliminarBodega(int id)
        {
            var bodega = await _context.Bodegas.FindAsync(id); // Realiza la consulta para de la bodega relacionada a eliminar
            if (bodega == null)
            {
                return NotFound();
            }


            var insumos = await _context.Insumos
                .Where(i => i.IdBodega == id)
                .ToListAsync(); // variable para realizar la consulta de los insumos que se encuentran relacionados con las bodegas

          
            foreach (var insumo in insumos)
            {
                
                var salidas = await _context.SalidaMateriales
                    .Where(sm => sm.IdInsumo == insumo.IdInsumos)
                    .ToListAsync();
                _context.SalidaMateriales.RemoveRange(salidas);

            } // Variable para realizar la consulta de la salida de materiales relacionada con los insumos y bodegas, para que esta sea removida

            
            _context.Insumos.RemoveRange(insumos);
            _context.Bodegas.Remove(bodega);

            await _context.SaveChangesAsync();
            // Se elimina la bodega con los insumos relacionados y cualquier dependencia que tenga con salidas
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListaInsumos(int idBodega, string? filtro = null, int page = 1, int pageSize = 5)
        {   //Este es una variable en el que se realiza consulta de la bodega con los insumos que se encuentran ingresados
            var bodega = await _context.Bodegas.Include(b => b.Insumos).FirstOrDefaultAsync(b => b.IdBodega == idBodega);

            if (bodega == null)
            {
                return NotFound();
            }

            
            if (!string.IsNullOrEmpty(filtro)) // Este es un filtro en el que realiza la consulta del insumo especifico en la bodega correspondiente
            {
                bodega.Insumos = bodega.Insumos!
                    .Where(i => i.Nombre!.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                                i.Tipo!.Contains(filtro, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

           
            var totalInsumos = bodega.Insumos!.Count;
            var totalPages = (int)Math.Ceiling(totalInsumos / (double)pageSize);

            // Paginación de la cantidad de insumos en la lista
            var insumosPaginados = bodega.Insumos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            
            ViewData["PaginaActual"] = page;
            ViewData["TotalPaginas"] = totalPages;
            ViewData["IdBodega"] = idBodega;
            ViewData["Filtro"] = filtro; 

            
            bodega.Insumos = insumosPaginados;

            return View(bodega);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult AgregarInsumo(int idBodega) // este es un metodo para preparar la vista con el formulario para agregar le insumo
        {
            ViewBag.IdBodega = idBodega;
            var insumo = new Insumos { IdBodega = idBodega }; 
            return View(insumo);
        }

        [HttpPost]
        public IActionResult AgregarInsumo(Insumos insumos)
        {
            if (ModelState.IsValid) // este es una sentencia en el caso de que el modelo sea valido para actualizar la bodega con la lista de los insumos ingresados
            {
                
                var bodega = _context.Bodegas.Find(insumos.IdBodega); // Este es una variable para consultar en la bodega que se ingresara medianter Id
                if (bodega == null)
                {
                    ModelState.AddModelError("IdBodega", "La bodega especificada no existe.");
                    return View(insumos);
                }

                
                insumos.IdBodega = bodega.IdBodega; 

               
                _context.Insumos.Add(insumos);
                _context.SaveChanges();

                
                return RedirectToAction("ListaInsumos", new { idBodega = insumos.IdBodega }); // Retorna a la vista de la lista de insumos en la bodega correspondiente mediante Id
            }

            return View(insumos);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditarInsumo(int idBodega, int idInsumo)
        {
            var insumo = await _context.Insumos.FindAsync(idInsumo); // Se realiza la consulta del insumo que se editara mediante Id
            if (insumo == null)
            {
                return NotFound();
            }

            ViewBag.IdBodega = idBodega; 
            return View(insumo);
        }

        [HttpPost]
        public async Task<IActionResult> EditarInsumo(int idBodega, Insumos insumo)
        {
            if (ModelState.IsValid) // En esta sentencia actualiza el insumo en el caso de que el modelo sea valido, en el que se realiza mediante Id
            {
                
                var insumoExistente = await _context.Insumos.FindAsync(insumo.IdInsumos); // Este variable realiza la consulta del insumo a editar mediante Id
                if (insumoExistente != null) // este es una sentencia en el caso de que el insumo que se este editando exista
                {
                    
                    insumoExistente.Nombre = insumo.Nombre;
                    insumoExistente.Tipo = insumo.Tipo;
                    insumoExistente.Cantidad = insumo.Cantidad;

                    await _context.SaveChangesAsync(); 
                    return RedirectToAction("ListaInsumos", new { idBodega }); 
                }
                else
                {
                    
                    return NotFound();
                }
            }

            ViewBag.IdBodega = idBodega; 
            return View(insumo); 
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmarEliminarInsumo(int idBodega, int idInsumo)
        {  // Aqui renderiza la vista para confirmar la eliminación del insumo
            var insumo = await _context.Insumos.FirstOrDefaultAsync(i => i.IdInsumos == idInsumo && i.IdBodega == idBodega);

            if (insumo == null)
            {
                return NotFound();
            }

            ViewBag.IdBodega = idBodega;
            return View(insumo);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarInsumo(int idBodega, int idInsumo)
        {
            //Aqui se realiza la consulta del insumo relacionado con la bodega mediante Id
            var insumo = await _context.Insumos.FirstOrDefaultAsync(i => i.IdInsumos == idInsumo && i.IdBodega == idBodega);

            if (insumo == null)
            {
                return NotFound();
            }

            // Aqui se realiza la consulta del insumo relacionado con la salida de Materiales
            var salidasRelacionadas = _context.SalidaMateriales.Where(s => s.IdInsumo == idInsumo);
            _context.SalidaMateriales.RemoveRange(salidasRelacionadas);

           
            _context.Insumos.Remove(insumo);

        
            await _context.SaveChangesAsync();

            return RedirectToAction("ListaInsumos", new { idBodega });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SalidaMaterial(string filtro = "") // Aqui se renderíza la vista con el filtro, la lista de los insumos que se seleccionará
        {
            
            ViewData["Bodegas"] = _context.Bodegas.ToList(); // Se utiliza el ViewData para seleccionar la bodega existente
            ViewData["Obras"] = _context.Obras.ToList(); // Se utiliza el viewData para seleccionar la obra existente

           
            var insumosQuery = _context.Insumos.AsQueryable(); // Se realiza la consulta de los insumoos que se seleccionará

            if (!string.IsNullOrEmpty(filtro))
            {
                
                insumosQuery = insumosQuery.Where(i => i.Nombre!.Contains(filtro) || i.Tipo!.Contains(filtro));
            }

            var insumos = insumosQuery.ToList(); // Se uso una variable para consultar los insumos que se encuentran ingresados
            ViewData["Insumos"] = insumos; // Se utiliza un ViewData para visualizar la lista de los insumos
            ViewData["Filtro"] = filtro;  

            var salidaMateriales = _context.SalidaMateriales.ToList();
            return View(salidaMateriales);
        }

        [HttpPost]
        public async Task<IActionResult> SalidaMaterial(int idBodega, int[] selectedInsumos, int[] cantidades, int? idObra, int page = 1)
        {
            if (selectedInsumos.Length == 0) //Este es una sentencia en el que arroja un menseja en el caso de que no haya seleccionado un insumo para la salida
            {
                TempData["ErrorMessage"] = "Debe seleccionar al menos un material para realizar la salida";
                return RedirectToAction("SalidaMaterial");
            }

            for (int i = 0; i < selectedInsumos.Length; i++) // Se itera la cantidad de insumos seleccionados con sus cantidades indicadas, en el que se obtiene el id del insumo y la cantidad
            {
                int idInsumo = selectedInsumos[i];
                int cantidad = cantidades[i];
                // Este es una variable en el que se busca el insumo en la bodega que pertenece
                var insumo = await _context.Insumos.FirstOrDefaultAsync(i => i.IdInsumos == idInsumo && i.IdBodega == idBodega);

                if (insumo == null) // Si no se encuentra el insumo, arroja un mensaje de error
                {
                    TempData["ErrorMessage"] = $"El insumo con ID {idInsumo} en la bodega {idBodega} no fue encontrado";
                    continue;
                }

                if (insumo.Cantidad < cantidad) // Si hay suficiente stock del insumo, este arrojara un mensaje
                {
                    TempData["ErrorMessage"] = $"No hay suficiente cantidad de '{insumo.Nombre}' para realizar la salida";
                    continue;
                }

                var salidaMaterial = new SalidaMaterial // Aqui se crea un objeto con los datos proporcionados para guardar la salida del material
                {
                    IdBodega = idBodega,
                    IdInsumo = idInsumo,
                    Cantidad = cantidad,
                    FechaSalida = DateTime.Now,
                    IdObra = idObra  
                };

                _context.SalidaMateriales.Add(salidaMaterial); // Se guarda la salida del material en el contexto de la base de datos

                
                insumo.Cantidad -= cantidad; // Se resta la cantidad del insumo
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Salida de material registrada exitosamente"; // Muestra un mensaje del registro
            return RedirectToAction("SalidaMaterial", new { page });
        }

        public async Task<IActionResult> ListaSalidas(int pageNumber = 1, int pageSize = 5)
        { // Aqui se muestra la vista con la lista de salidas registradas
            var salidas = await _context.SalidaMateriales // este es una variable que permite acceder al conjunto de datos correspondiente a salida material
                .Include(sm => sm.Bodega)   // En el que se incluye los datos relacionados con bodega, insumo, y obra   
                .Include(sm => sm.Insumo)    // y cada include, se encuentra la propiedad de navegación relacionado con salida material, permitiendo asegurar que los datos se encuentren asociados a cada salida   
                .Include(sm => sm.Obra)        
                .ToListAsync();

            var totalSalidas = salidas.Count();
            var totalPages = (int)Math.Ceiling(totalSalidas / (double)pageSize);

            var pagedSalidas = salidas
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["PaginaActual"] = pageNumber;
            ViewData["TotalPaginas"] = totalPages;

            return View(pagedSalidas);
        }

        [HttpGet]
        public IActionResult ExportarSalidaMaterialPdf()
        {
            var salidas = _context.SalidaMateriales // Se reliza la consulta de las salidas de materiales en la base de datos, en la que se incluyen bodega, insumo y obra
                .Include(sm => sm.Bodega)
                .Include(sm => sm.Insumo)
                .Include(sm => sm.Obra) 
                .ToList();

            if (salidas.Count == 0) // Si no hay datos registrados, arroja un error HTTP 404
            {
                return NotFound("No hay salidas de material registradas.");
            }

            using (MemoryStream ms = new MemoryStream()) //el memory stream es para crear el archivo en pdf usando la librería iText7
            {
                PdfWriter writer = new PdfWriter(ms);

                using (PdfDocument pdf = new PdfDocument(writer)) 
                {
                    Document document = new Document(pdf, PageSize.A4);// Aqui se definen los márgenes
                    document.SetMargins(20, 20, 20, 20);

                    
                    var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD); // Se define le tipo de letra
                    var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    
                    document.Add(new Paragraph("Salidas de Material Registradas") // Se establece la fuente en negrita y normal, agregando el título centrado
                        .SetFont(boldFont)
                        .SetFontSize(18)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20));

                    
                    Table table = new Table(new float[] { 2, 2, 1, 2, 2 }).UseAllAvailableWidth(); // Aqui se crea una tabla con 5 columnas y definiendo encabezado
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Bodega").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Insumo").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Cantidad").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha de Salida").SetFont(boldFont)));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Obra").SetFont(boldFont)));

                   
                    foreach (var salida in salidas) // Aqui recorre las salidas del material y llena la tabla con los datos
                    {
                        table.AddCell(new Cell().Add(new Paragraph(salida.Bodega?.NombreBodega ?? "No asignada").SetFont(normalFont)));
                        table.AddCell(new Cell().Add(new Paragraph(salida.Insumo?.Nombre ?? "No especificado").SetFont(normalFont)));
                        table.AddCell(new Cell().Add(new Paragraph(salida.Cantidad.ToString()).SetFont(normalFont)));
                        table.AddCell(new Cell().Add(new Paragraph(salida.FechaSalida.ToShortDateString()).SetFont(normalFont)));
                        table.AddCell(new Cell().Add(new Paragraph(salida.Obra?.NombreObra ?? "No asignada").SetFont(normalFont)));
                    }

                  
                    document.Add(table);

                    
                    document.Close();
                }

                
                return File(ms.ToArray(), "application/pdf", "SalidasMaterial.pdf"); // Aqui se cierra el documento para luego ser descargado
            }
        }

        [HttpGet]
        public IActionResult ExportarSalidaMaterialExcel()
        {
            var salidas = _context.SalidaMateriales // Se reliza la consulta de las salidas de materiales en la base de datos, en la que se incluyen bodega, insumo y obra
                .Include(sm => sm.Bodega)
                .Include(sm => sm.Insumo)
                .Include(sm => sm.Obra) 
                .ToList();
             
            using (var workbook = new XLWorkbook()) // Se usa la librería ClosedXML para convertir a excel
            {
                var worksheet = workbook.Worksheets.Add("Salidas de Material"); 

                // Sea crea una nueva hoja que es llamado salida material, definiendo los encabezados
                worksheet.Cell(1, 1).Value = "Bodega";
                worksheet.Cell(1, 2).Value = "Insumo";
                worksheet.Cell(1, 3).Value = "Cantidad";
                worksheet.Cell(1, 4).Value = "Fecha de Salida";
                worksheet.Cell(1, 5).Value = "Obra"; 

                
                var headerRange = worksheet.Range(1, 1, 1, 5); // Aqui se aplica los encabezados
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                int currentRow = 2;

                foreach (var salida in salidas) // Rellena la hoja con los datos
                {
                    worksheet.Cell(currentRow, 1).Value = salida.Bodega!.NombreBodega;
                    worksheet.Cell(currentRow, 2).Value = salida.Insumo!.Nombre;
                    worksheet.Cell(currentRow, 3).Value = salida.Cantidad;
                    worksheet.Cell(currentRow, 4).Value = salida.FechaSalida.ToShortDateString();
                    worksheet.Cell(currentRow, 5).Value = salida.Obra?.NombreObra ?? "No asignada"; 

                    
                    var rowRange = worksheet.Range(currentRow, 1, currentRow, 5);
                    rowRange.Style.Fill.BackgroundColor = XLColor.WhiteSmoke;

                    currentRow++;
                }

                
                worksheet.Columns().AdjustToContents(); // Se hace un ajuste automático con el ancho de las columnas para que se adapte al contenido

                
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalidasMaterial.xlsx"); // Guarda el archivo en momery stream y se envía al cliente
                }
            }
        }
    }
}