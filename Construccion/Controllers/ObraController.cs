using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using Construccion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClosedXML.Excel;

[Authorize(Roles = "Admin, Empleado")] // Solamente permite el acceso a ciertos roles de usuarios
public class ObraController : Controller
{
    private readonly ConstruccionContext _context; // 

    public ObraController(ConstruccionContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string Filtro = "", int page = 1, int pageSize = 5)
    {

        var obrasQuery = _context.Obras.AsQueryable(); // Aqui se realiza la consulta en la base de datos de obras


        if (!string.IsNullOrEmpty(Filtro)) // Aqui se realiza una consulta al filtro, en el caso de que coincidan las datos ingresados
        {
            obrasQuery = obrasQuery.Where(o => o.NombreObra!.Contains(Filtro) || o.Cliente!.Contains(Filtro));
            ViewData["Filtro"] = Filtro;
        }


        var totalObras = await obrasQuery.CountAsync(); // Aqui calcula la cantidad de obras que hay en la lista
        var totalPages = (int)Math.Ceiling(totalObras / (double)pageSize); // Aqui calcula la cantidad de páginas dependendiendo de la cantidad de obras

        // En el siguiente código correponde a la obtención de datos correspondientes a la página actual y son enviados a la vista
        var obras = await obrasQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["PaginaActual"] = page;
        ViewData["TotalPaginas"] = totalPages;

        return View(obras);
    }




    public IActionResult Crear()//Retorna la vista con los campos para crear la obra
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(Obra obra)// Valida los datos ingresados para la obra
    { 
        if (ModelState.IsValid) // Se realiza una sentencia si los datos ingresados del modelo son válidos 
        {
            _context.Obras.Add(obra); // Se agrega al contexto de la obra
            _context.SaveChanges(); // Se guarda los cambios en el contexto de la base de datos
            TempData["Mensaje"] = "Obra creada exitosamente."; // Arroja un mensaje cuando la obra ha sido creada
            return RedirectToAction("Index"); // redirecciona a la vista Index, en el que se encuentran 
        }
        return View(obra); // En el caso de que haya errores, regresa a la vista
    }

    public IActionResult Detalles(int id, string Filtro, int pagina = 1, int pageSize = 5)
    {
        var obra = _context.Obras
            .Include(o => o.Partidas)
            .ThenInclude(p => p.Materiales)
            .FirstOrDefault(o => o.IdObra == id); // Se crea una variable, en que se realiza la consulta al contexto de la base de datos
                                                  // En el que se incluye las patidas, los materiales y como por default la obra
        if (obra == null) // Si la obra no existe, retorna el error NotFound
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(Filtro))
        {
            obra.Partidas = obra.Partidas
                .Where(p => p.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var totalCount = obra.Partidas.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        obra.Partidas = obra.Partidas.Skip((pagina - 1) * pageSize).Take(pageSize).ToList();

        
        ViewData["Filtro"] = Filtro;
        ViewData["PaginaActual"] = pagina;
        ViewData["TotalPaginas"] = totalPages;

        return View(obra);
    }


    [Authorize(Roles = "Admin")]
    public IActionResult ConfirmarEliminarObra(int id)//Este es un metodo para mostrar la vista para la confirmacion de eliminacion de la obra
    {
        var obra = _context.Obras.FirstOrDefault(o => o.IdObra == id);
        if (obra == null)
        {
            return NotFound();
        }

        return View(obra); 
    }

    [Authorize(Roles = "Admin")]
    public IActionResult EliminarObra(int id)
    {
        
        var obra = _context.Obras
                           .Include(o => o.Partidas) 
                           .ThenInclude(p => p.Materiales) 
                           .FirstOrDefault(o => o.IdObra == id);

        if (obra != null) // Se crea una sentencia si la obra no es nulo
        {
            
            var hitos = _context.Hitos.Where(h => h.IdObra == id).ToList();
            _context.Hitos.RemoveRange(hitos); // Se crea una variable de hitos, en el que se busca el id de la obra en hitos, para que sea removida

            
            var seguimientos = _context.SeguimientoObras.Where(s => s.IdObra == id).ToList();
            _context.SeguimientoObras.RemoveRange(seguimientos); // Se crea  una variable de seguimiento en el que se busca el contexto en la base de datos
            // para consultar el id de la obra en el listado, para que este sea removido 
            
            foreach (var partida in obra.Partidas) // Son parámetros para consultar las partidas que se encuentran relacionadas con las obras, para que puedan ser removidas en conjunto con los materiales
            {
                _context.Materiales.RemoveRange(partida.Materiales);
            }

            
            _context.Partidas.RemoveRange(obra.Partidas); // se remueve las partidas que se encuentran relacionadas con las obras

            
            _context.Obras.Remove(obra); // Se remueve la obra en el contexto de la base de datos

            
            _context.SaveChanges(); // Se gaurdan los cambios en la base de datos 
        }

        return RedirectToAction("Index"); // Redirecciona a la vista Index
    }

    public IActionResult AgregarPartida(int id) // Se crea una metodo para agregar la partida en la obra mostrando la vista con el formulario para su ingreso
    {
        var obra = _context.Obras.Include(o => o.Partidas).FirstOrDefault(o => o.IdObra == id);
        return View(new Partida { IdObra = id }); // retorna la vista para crear una nueva partida en la obra que corresponde
    }


    [HttpPost]
    public IActionResult AgregarPartida(int obraId, Partida partida)
    {
        try // En este caso se utiliza le try para manejar las excepciones que pueda ocurrir en el proceso de agregar la partida
        {
            var obra = _context.Obras.Find(obraId); // Se crea una variable para consultar la obra que se quiere ingresar la partida mediante Id
            if (obra != null) // Se crea una sentencia para consultar si la obra existe
            {
                partida.IdObra = obraId; // Aqui se establece una relacion entre y la obra, asignando el obraId al campo IdObra de la partida
                _context.Partidas.Add(partida); // Agrega la partida en la base de datos
                _context.SaveChanges(); // Guarda la partida en la base de datos
                return RedirectToAction("Detalles", new { id = obraId }); // Se redirije a la vista detalles de la obra correspondiente
            }
            else
            { // En el caso de que no se encuentre la obra al agregar la partda, arrojará un error
                ModelState.AddModelError("", "Obra no encontrada.");
                return View(partida);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error al agregar la partida: " + ex.Message);
            return View(partida);
        }
    }

    public IActionResult EditarPartida(int id) // Aqui se muestra el formulario de la partida a editar a la obra que corresponde
    {
        var partida = _context.Partidas.Include(p => p.Materiales).FirstOrDefault(p => p.IdPartida == id);

        if (partida == null)
        {
            return NotFound();
        }

        return View(partida);
    }

    [HttpPost]
    public async Task<IActionResult> EditarPartida(Partida partida)
    {
        if (!ModelState.IsValid)// Aqui se hace la setencia, para verificar la validacion del modelo, por el cumplimiento de la regla del mismo
        {
            return View(partida);
        }

        var partidaExistente = await _context.Partidas
            .Include(p => p.Materiales)
            .FirstOrDefaultAsync(p => p.IdPartida == partida.IdPartida); // Aqui se hace la consulta de en el contexto de la base de datos de la tabla partidas, en el que se incluye los materiales y por default identifica el Id de la partida

        if (partidaExistente == null) // En el caso de que la partida sea nulo, arrojara un error 4HTTP 404
        {
            return NotFound();
        }

        //Aqui colecciona el nombre y el valor de la mano de obra
        partidaExistente.Nombre = partida.Nombre;
        partidaExistente.ManoDeObra = partida.ManoDeObra;

        
        foreach (var material in partida.Materiales) // En esta sentencia se itera los datos de los materiales ingresados en la partida
        {
            var materialExistente = partidaExistente.Materiales.FirstOrDefault(m => m.Id == material.Id);
            if (materialExistente != null)
            {
                materialExistente.Nombre = material.Nombre;
                materialExistente.Cantidad = material.Cantidad;
                materialExistente.Precio = material.Precio;
                materialExistente.TipoUnidad = material.TipoUnidad;
            }
        }

        
        await _context.SaveChangesAsync(); // se sincronaza la edicion y es guardado en la base de datos
        return RedirectToAction("Detalles", new { id = partida.IdObra });// redirecciona a la vista detalles con los datos cambiados
    }


    [Authorize(Roles = "Admin")]
    public IActionResult ConfirmarEliminarPartida(int idPartida, int idObra) // Aqui se muestra la vista para confirmar la eliminacion de la partida
    {
        var partida = _context.Partidas.Include(p => p.Obra).FirstOrDefault(p => p.IdPartida == idPartida);

        if (partida == null)
        {
            return NotFound();
        }

        ViewBag.IdObra = idObra;
        return View(partida);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult EliminarPartida(int idPartida)
    {
        var partida = _context.Partidas.Include(p => p.Materiales)
                                       .FirstOrDefault(p => p.IdPartida == idPartida);

        if (partida != null)
        {
            // Aqui se obtiene el id de la obra para la eliminacion de la partida
            int idObra = partida.IdObra;

            // Para eliminar la partida se tiene que tener asociados los materiales que se eliminaran
            foreach (var material in partida.Materiales)
            {
                _context.Materiales.Remove(material);
            }

            // Se eliminara la partida con las materiales asociados
            _context.Partidas.Remove(partida);

            // Se guardan los cambios una vez eliminados las partidas
            _context.SaveChanges();

            // Se redirecciona a los detalles de la obra
            return RedirectToAction("Detalles", "Obra", new { id = idObra });
        }

        // En caso de que no exista la partida, se redirecciona a la vista de lista de obras
        return RedirectToAction("Index");
    }



    public IActionResult AgregarMaterial(int idObra, int idPartida) // Aqui se agrega el material a la partida de la obra correspondiente
    {
        var partida = _context.Partidas.Include(p => p.Materiales).FirstOrDefault(p => p.IdPartida == idPartida);

        if (partida == null)
        {
            return NotFound();
        }

        var material = new Material
        {
            IdPartida = idPartida 
        };

        ViewBag.IdObra = idObra;
        ViewBag.IdPartida = idPartida;


        return View(material);
    }

    [HttpPost]
    public IActionResult AgregarMaterial(int idObra, int idPartida, Material material)
    {
        try // En este caso se utiliza le try para manejar las excepciones que pueda ocurrir en el proceso de agregar la partida
        {
            var partida = _context.Partidas.Include(p => p.Materiales).FirstOrDefault(p => p.IdPartida == idPartida); // Se realiza una consulta a la base de datos de las partidas en la que se incluye el material, y como por defecto el Id de la partida

            if (partida == null) // Aqui se hace una sentencia para en el caso de que la aprtida no exista y no cumpla con las reglas del modelo
            {
                ModelState.AddModelError("", "Partida no encontrada.");
                return RedirectToAction("Detalles", new { id = idObra });
            }

            material.IdPartida = idPartida; // Aqui se agrega el material a la partida existente
            _context.Materiales.Add(material); // Se agrega el material al contexto de la base de datos
            _context.SaveChanges();

            return RedirectToAction("Detalles", new { id = idObra }); // Redirecciona a la vista detalle a la obra correspondiente
        }
        catch (Exception ex) // En el caso de que no se encuentre la obra al agregar la partda, arrojará un error
        {
            ModelState.AddModelError("", "Error al agregar el material: " + ex.Message);
            return RedirectToAction("Detalles", new { id = idObra }); 
        }
    }

    public IActionResult ExportarPdf(int id)
    {
        var obra = _context.Obras
            .Include(o => o.Partidas)
            .ThenInclude(p => p.Materiales)
            .FirstOrDefault(o => o.IdObra == id);

        if (obra == null)
        {
            return NotFound();
        }

        using (MemoryStream ms = new MemoryStream())
        {
            PdfWriter writer;

            try
            {
                writer = new PdfWriter(ms);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest($"Error al crear el PdfWriter: {ex.Message}");
            }

            using (PdfDocument pdf = new PdfDocument(writer))
            {
                Document document = new Document(pdf, PageSize.A4);
                document.SetMargins(20, 20, 20, 20);

   
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                
                document.Add(new Paragraph($"Obra: {obra.NombreObra}")
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10));

                document.Add(new Paragraph($"Cliente: {obra.Cliente}")
                    .SetFont(normalFont)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10));

                document.Add(new Paragraph($"Total Presupuesto: {obra.Total:C}")
                    .SetFont(normalFont)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                
                Table table = new Table(new float[] { 1, 2, 2 }).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("Partida").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Materiales").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Subtotal").SetFont(boldFont)));

                foreach (var partida in obra.Partidas)
                {
                    
                    table.AddCell(new Cell(1, 1).Add(new Paragraph(partida.Nombre).SetFont(normalFont)));

                    
                    Table materialTable = new Table(new float[] { 2, 1, 1 }).UseAllAvailableWidth();
                    materialTable.AddHeaderCell(new Cell().Add(new Paragraph("Material").SetFont(boldFont)));
                    materialTable.AddHeaderCell(new Cell().Add(new Paragraph("Cantidad").SetFont(boldFont)));
                    materialTable.AddHeaderCell(new Cell().Add(new Paragraph("Precio").SetFont(boldFont)));

                    foreach (var material in partida.Materiales)
                    {
                        materialTable.AddCell(new Cell().Add(new Paragraph(material.Nombre).SetFont(normalFont)));
                        materialTable.AddCell(new Cell().Add(new Paragraph(material.Cantidad.ToString()).SetFont(normalFont)));
                        materialTable.AddCell(new Cell().Add(new Paragraph($"{material.Precio:C}").SetFont(normalFont)));
                    }

                    
                    table.AddCell(new Cell().Add(materialTable));

                    
                    table.AddCell(new Cell(1, 1).Add(new Paragraph($"{partida.Subtotal:C}").SetFont(normalFont)));
                }

                
                document.Add(table);

                
                document.Close();
            }

            return File(ms.ToArray(), "application/pdf", $"Presupuesto_Obra_{obra.NombreObra}.pdf");
        }
    }

    [HttpGet]
    public IActionResult ExportarExcel(int idObra)
    {
        var obra = _context.Obras
            .Include(o => o.Partidas)
            .ThenInclude(p => p.Materiales)
            .FirstOrDefault(o => o.IdObra == idObra);

        if (obra == null)
        {
            return NotFound();
        }

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Detalles de la Obra");

            // Se agregan los encabezados de para los titulos
            worksheet.Cell(1, 1).Value = "Cliente";
            worksheet.Cell(1, 2).Value = "Nombre de la Obra";
            worksheet.Cell(1, 3).Value = "Total Presupuesto";
            worksheet.Cell(1, 4).Value = "IVA";
            worksheet.Cell(1, 5).Value = "Total";

            worksheet.Cell(2, 1).Value = obra.Cliente;
            worksheet.Cell(2, 2).Value = obra.NombreObra;
            worksheet.Cell(2, 3).Value = obra.TotalPresupuesto;
            worksheet.Cell(2, 4).Value = obra.IVA;
            worksheet.Cell(2, 5).Value = obra.Total;

            // Se agregan estilos a los encabezados
            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Se visualzaran las partidas que fueron agregadas
            int currentRow = 4;
            worksheet.Cell(currentRow, 1).Value = "Partidas y Materiales";
            currentRow++;

            foreach (var partida in obra.Partidas)
            {
                worksheet.Cell(currentRow, 1).Value = "Partida";
                worksheet.Cell(currentRow, 2).Value = partida.Nombre;
                worksheet.Cell(currentRow, 3).Value = partida.Subtotal;
                worksheet.Cell(currentRow, 4).Value = partida.ManoDeObra;

                // Se visualizarán los titulos de cada partida
                var partidaRange = worksheet.Range(currentRow, 1, currentRow, 4);
                partidaRange.Style.Font.Bold = true;
                partidaRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                currentRow++;

                // Se agregan los materiales
                worksheet.Cell(currentRow, 2).Value = "Material";
                worksheet.Cell(currentRow, 3).Value = "Cantidad";
                worksheet.Cell(currentRow, 4).Value = "Precio";

                currentRow++;

                foreach (var material in partida.Materiales)
                {
                    worksheet.Cell(currentRow, 2).Value = material.Nombre;
                    worksheet.Cell(currentRow, 3).Value = material.Cantidad;
                    worksheet.Cell(currentRow, 4).Value = material.Precio;
                    currentRow++;
                }
            }

            // Se realiza el ajustes de los anchos de forma automática
            worksheet.Columns().AdjustToContents();

            // Se guarda el archivo en memoria y se convierte en una descarga
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Obra_{obra.NombreObra}.xlsx");
            }
        }
    }
}
