using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using BibliotecaMVC.Reports;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace BibliotecaMVC.Controllers;

[Authorize(Roles = "admin,bibliotecario,usuario")]
public class ReportesController : Controller
{
    private readonly BibliotecaContext _context;

    public ReportesController(BibliotecaContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? autorId, int? categoriaId)
    {
        var libros = await ConsultarLibrosAsync(autorId, categoriaId);
        ViewData["Autores"] = new SelectList(
            await _context.Autores.AsNoTracking().OrderBy(autor => autor.Nombre).ThenBy(autor => autor.Apellido).ToListAsync(),
            nameof(Autor.Id),
            nameof(Autor.Nombre),
            autorId);
        ViewData["Categorias"] = new SelectList(
            await _context.Set<Categoria>().AsNoTracking().OrderBy(categoria => categoria.Nombre).ToListAsync(),
            nameof(Categoria.Id),
            nameof(Categoria.Nombre),
            categoriaId);
        ViewData["AutorId"] = autorId;
        ViewData["CategoriaId"] = categoriaId;
        ViewData["CantidadLibros"] = libros.Count;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerarPdf(int? autorId, int? categoriaId)
    {
        var libros = await ConsultarLibrosAsync(autorId, categoriaId);

        var documento = new ReporteLibrosDocument(libros, DateTime.Now);
        var contenidoPdf = documento.GeneratePdf();

        return File(contenidoPdf, "application/pdf", "ReporteLibros.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> GenerarExcel(int? autorId, int? categoriaId)
    {
        var libros = await ConsultarLibrosAsync(autorId, categoriaId);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Reporte de Libros");

        worksheet.Cell("A1").Value = "BibliotecaMVC";
        worksheet.Cell("A2").Value = "Reporte de Libros";
        worksheet.Cell("A3").Value = "Fecha de generación";
        worksheet.Cell("B3").Value = DateTime.Now;
        worksheet.Cell("B3").Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
        worksheet.Cell("A4").Value = "Cantidad de libros";
        worksheet.Cell("B4").Value = libros.Count;

        worksheet.Range("A1:D1").Merge();
        worksheet.Range("A2:D2").Merge();
        worksheet.Range("A1:D1").Style.Font.Bold = true;
        worksheet.Range("A1:D1").Style.Font.FontSize = 14;
        worksheet.Range("A1:D1").Style.Font.FontColor = XLColor.White;
        worksheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78");
        worksheet.Range("A2:D2").Style.Font.Bold = true;
        worksheet.Range("A2:D2").Style.Font.FontSize = 12;
        worksheet.Range("A2:D2").Style.Font.FontColor = XLColor.FromHtml("#1F1F1F");
        worksheet.Range("A3:B4").Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
        worksheet.Range("A3:B4").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range("A3:B4").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        var headerRow = 6;
        worksheet.Cell(headerRow, 1).Value = "ID";
        worksheet.Cell(headerRow, 2).Value = "Nombre del libro";
        worksheet.Cell(headerRow, 3).Value = "Autor";
        worksheet.Cell(headerRow, 4).Value = "Categoría";

        var header = worksheet.Range(headerRow, 1, headerRow, 4);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        for (var index = 0; index < libros.Count; index++)
        {
            var libro = libros[index];
            var row = headerRow + index + 1;
            worksheet.Cell(row, 1).Value = libro.Id;
            worksheet.Cell(row, 2).Value = libro.Titulo;
            worksheet.Cell(row, 3).Value = ObtenerNombreAutor(libro);
            worksheet.Cell(row, 4).Value = string.IsNullOrWhiteSpace(libro.Categoria?.Nombre)
                ? "Sin categoría"
                : libro.Categoria.Nombre;

            var dataRow = worksheet.Range(row, 1, row, 4);
            dataRow.Style.Fill.BackgroundColor = index % 2 == 0 ? XLColor.White : XLColor.FromHtml("#F2F2F2");
            dataRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        worksheet.Columns(1, 4).AdjustToContents();
        for (var column = 1; column <= 4; column++)
        {
            worksheet.Column(column).Width = Math.Min(worksheet.Column(column).Width, 50);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ReporteLibros.xlsx");
    }

    private Task<List<Libro>> ConsultarLibrosAsync(int? autorId, int? categoriaId)
    {
        IQueryable<Libro> consulta = _context.Libros
            .AsNoTracking()
            .Include(libro => libro.Autor)
            .Include(libro => libro.Categoria);

        if (autorId.HasValue)
        {
            consulta = consulta.Where(libro => libro.AutorId == autorId.Value);
        }

        if (categoriaId.HasValue)
        {
            consulta = consulta.Where(libro => libro.CategoriaId == categoriaId.Value);
        }

        return consulta
            .OrderBy(libro => libro.Titulo)
            .ThenBy(libro => libro.Id)
            .ToListAsync();
    }

    private static string ObtenerNombreAutor(Libro libro)
    {
        var nombre = string.Join(" ", new[] { libro.Autor?.Nombre, libro.Autor?.Apellido }
            .Where(valor => !string.IsNullOrWhiteSpace(valor)));

        return string.IsNullOrWhiteSpace(nombre) ? "Sin autor" : nombre;
    }
}
