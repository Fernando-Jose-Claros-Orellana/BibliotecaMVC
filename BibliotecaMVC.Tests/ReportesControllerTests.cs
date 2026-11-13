using System.Text;
using System.Reflection;
using BibliotecaMVC.Controllers;
using BibliotecaMVC.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuestPDF.Infrastructure;
using UglyToad.PdfPig;
using Xunit;

namespace BibliotecaMVC.Tests;

public class ReportesControllerTests
{
    public ReportesControllerTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Fact]
    public async Task Index_WithoutFilters_ReturnsAllBooksAndFilterOptions()
    {
        using var context = CreateReportDatabase();
        var controller = CreateController(context);

        var result = await controller.Index(null, null);

        Assert.IsType<ViewResult>(result);
        Assert.Equal(4, controller.ViewData["CantidadLibros"]);
        Assert.Equal(2, Assert.IsAssignableFrom<IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>>(controller.ViewData["Autores"]).Count());
        Assert.Equal(2, Assert.IsAssignableFrom<IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>>(controller.ViewData["Categorias"]).Count());
    }

    [Fact]
    public async Task Index_FilterByAuthor_ReturnsOnlyThatAuthorsBooksAndKeepsSelection()
    {
        using var context = CreateReportDatabase();
        var controller = CreateController(context);

        await controller.Index(1, null);

        Assert.Equal(2, controller.ViewData["CantidadLibros"]);
        Assert.Equal(1, controller.ViewData["AutorId"]);
        Assert.Null(controller.ViewData["CategoriaId"]);
    }

    [Fact]
    public async Task Index_FilterByCategory_ReturnsOnlyThatCategoryBooksAndKeepsSelection()
    {
        using var context = CreateReportDatabase();
        var controller = CreateController(context);

        await controller.Index(null, 2);

        Assert.Equal(2, controller.ViewData["CantidadLibros"]);
        Assert.Null(controller.ViewData["AutorId"]);
        Assert.Equal(2, controller.ViewData["CategoriaId"]);
    }

    [Fact]
    public async Task Index_CombinedFilters_ReturnsIntersection()
    {
        using var context = CreateReportDatabase();
        var controller = CreateController(context);

        await controller.Index(1, 2);

        Assert.Equal(1, controller.ViewData["CantidadLibros"]);
    }

    [Fact]
    public async Task Index_FiltersWithoutMatches_ReturnsZero()
    {
        using var context = CreateReportDatabase();
        var controller = CreateController(context);

        await controller.Index(99, 99);

        Assert.Equal(0, controller.ViewData["CantidadLibros"]);
    }

    [Fact]
    public async Task GenerarPdf_WithoutFilters_ContainsAllBooksAndSummary()
    {
        using var context = CreateReportDatabase();
        var controller = CreateController(context);

        var result = Assert.IsType<FileContentResult>(await controller.GenerarPdf(null, null));
        var text = ExtractPdfText(result.FileContents);

        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("ReporteLibros.pdf", result.FileDownloadName);
        Assert.Contains("Total de libros: 4", text);
        Assert.Contains("Alfa", text);
        Assert.Contains("Beta", text);
        Assert.Contains("Gamma", text);
        Assert.Contains("Zeta", text);
    }

    [Fact]
    public async Task GenerarPdf_WithFilters_ContainsOnlyMatchingBooksAndFilteredSummary()
    {
        using var context = CreateReportDatabase();
        var controller = CreateController(context);

        var result = Assert.IsType<FileContentResult>(await controller.GenerarPdf(1, 2));
        var text = ExtractPdfText(result.FileContents);

        Assert.Contains("Total de libros: 1", text);
        Assert.Contains("Beta", text);
        Assert.DoesNotContain("Alfa", text);
        Assert.DoesNotContain("Gamma", text);
        Assert.DoesNotContain("Zeta", text);
    }

    [Fact]
    public async Task GenerarPdf_WithNoMatches_ReturnsValidPdfWithEmptyMessageAndZeroSummary()
    {
        using var context = CreateReportDatabase();
        var controller = CreateController(context);

        var result = Assert.IsType<FileContentResult>(await controller.GenerarPdf(99, 99));
        var text = ExtractPdfText(result.FileContents);

        Assert.Contains("Total de libros: 0", text);
        Assert.Contains("No hay libros registrados en la base de datos.", text);
    }

    [Fact]
    public async Task GenerarExcel_WithoutFilters_ContainsAllBooksAndReportMetadata()
    {
        using var context = CreateReportDatabase();
        var result = Assert.IsType<FileContentResult>(await CreateController(context).GenerarExcel(null, null));

        using var workbook = OpenWorkbook(result.FileContents);
        var worksheet = AssertExcelDownload(result, workbook);

        Assert.Equal("BibliotecaMVC", worksheet.Cell("A1").GetString());
        Assert.Equal("Reporte de Libros", worksheet.Cell("A2").GetString());
        Assert.Equal("Fecha de generación", worksheet.Cell("A3").GetString());
        Assert.True(worksheet.Cell("B3").GetDateTime() <= DateTime.Now);
        Assert.Equal("Cantidad de libros", worksheet.Cell("A4").GetString());
        Assert.Equal(4, worksheet.Cell("B4").GetValue<int>());
        Assert.Equal("ID", worksheet.Cell("A6").GetString());
        Assert.Equal("Nombre del libro", worksheet.Cell("B6").GetString());
        Assert.Equal("Autor", worksheet.Cell("C6").GetString());
        Assert.Equal("Categoría", worksheet.Cell("D6").GetString());
        Assert.Equal(4, GetBookTitles(worksheet).Length);
        Assert.Equal("Ana Uno", worksheet.Cell("C8").GetString());
        Assert.Equal("Arte", worksheet.Cell("D8").GetString());
        Assert.Equal(XLColor.FromHtml("#4472C4"), worksheet.Cell("A6").Style.Fill.BackgroundColor);
        Assert.Equal(XLBorderStyleValues.Thin, worksheet.Cell("A7").Style.Border.BottomBorder);
    }

    [Fact]
    public async Task GenerarExcel_FilterByAuthor_ContainsOnlyMatchingBooks()
    {
        using var context = CreateReportDatabase();
        var result = Assert.IsType<FileContentResult>(await CreateController(context).GenerarExcel(1, null));
        using var workbook = OpenWorkbook(result.FileContents);
        var worksheet = AssertExcelDownload(result, workbook);

        Assert.Equal(2, worksheet.Cell("B4").GetValue<int>());
        Assert.Equal(new[] { "Alfa", "Beta" }, GetBookTitles(worksheet));
        Assert.All(worksheet.Rows(7, 8), row => Assert.Equal("Ana Uno", row.Cell(3).GetString()));
    }

    [Fact]
    public async Task GenerarExcel_FilterByCategory_ContainsOnlyMatchingBooks()
    {
        using var context = CreateReportDatabase();
        var result = Assert.IsType<FileContentResult>(await CreateController(context).GenerarExcel(null, 2));
        using var workbook = OpenWorkbook(result.FileContents);
        var worksheet = AssertExcelDownload(result, workbook);

        Assert.Equal(2, worksheet.Cell("B4").GetValue<int>());
        Assert.Equal(new[] { "Beta", "Gamma" }, GetBookTitles(worksheet));
        Assert.All(worksheet.Rows(7, 8), row => Assert.Equal("Arte", row.Cell(4).GetString()));
    }

    [Fact]
    public async Task GenerarExcel_CombinedFilters_ContainsIntersectionOnly()
    {
        using var context = CreateReportDatabase();
        var result = Assert.IsType<FileContentResult>(await CreateController(context).GenerarExcel(1, 2));
        using var workbook = OpenWorkbook(result.FileContents);
        var worksheet = AssertExcelDownload(result, workbook);

        Assert.Equal(1, worksheet.Cell("B4").GetValue<int>());
        Assert.Equal(new[] { "Beta" }, GetBookTitles(worksheet));
        Assert.Equal(2, worksheet.Cell("A7").GetValue<int>());
        Assert.Equal("Ana Uno", worksheet.Cell("C7").GetString());
        Assert.Equal("Arte", worksheet.Cell("D7").GetString());
    }

    [Fact]
    public async Task GenerarExcel_WithoutMatches_CreatesValidWorkbookWithoutDataRows()
    {
        using var context = CreateReportDatabase();
        var result = Assert.IsType<FileContentResult>(await CreateController(context).GenerarExcel(99, 99));
        using var workbook = OpenWorkbook(result.FileContents);
        var worksheet = AssertExcelDownload(result, workbook);

        Assert.Equal(0, worksheet.Cell("B4").GetValue<int>());
        Assert.True(worksheet.Cell("A7").IsEmpty());
        Assert.Equal("Categoría", worksheet.Cell("D6").GetString());
    }

    private static BibliotecaMVC.Data.BibliotecaContext CreateReportDatabase()
    {
        var options = new DbContextOptionsBuilder<BibliotecaMVC.Data.BibliotecaContext>()
            .UseInMemoryDatabase($"BibliotecaMVC.Tests.Reportes.{Guid.NewGuid()}")
            .Options;
        var context = new BibliotecaMVC.Data.BibliotecaContext(options);
        context.Autores.AddRange(
            new Autor { Id = 1, Nombre = "Ana", Apellido = "Uno" },
            new Autor { Id = 2, Nombre = "Beto", Apellido = "Dos" });
        context.Set<Categoria>().AddRange(
            new Categoria { Id = 1, Nombre = "Historia" },
            new Categoria { Id = 2, Nombre = "Arte" });
        context.Libros.AddRange(
            new Libro { Id = 1, Titulo = "Alfa", AutorId = 1, CategoriaId = 1 },
            new Libro { Id = 2, Titulo = "Beta", AutorId = 1, CategoriaId = 2 },
            new Libro { Id = 3, Titulo = "Gamma", AutorId = 2, CategoriaId = 2 },
            new Libro { Id = 4, Titulo = "Zeta", AutorId = 2, CategoriaId = 1 });
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return context;
    }

    private static ReportesController CreateController(BibliotecaMVC.Data.BibliotecaContext context)
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerName = "Reportes",
            ActionName = "Index",
            ControllerTypeInfo = typeof(ReportesController).GetTypeInfo()
        };
        var controller = new ReportesController(context)
        {
            ControllerContext = new ControllerContext(
                new ActionContext(httpContext, new RouteData(), actionDescriptor, modelState)),
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelState)
        };
        return controller;
    }

    private static string ExtractPdfText(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var document = PdfDocument.Open(stream);
        return string.Join("\n", document.GetPages().Select(page => page.Text));
    }

    private static XLWorkbook OpenWorkbook(byte[] bytes)
    {
        return new XLWorkbook(new MemoryStream(bytes));
    }

    private static IXLWorksheet AssertExcelDownload(FileContentResult result, XLWorkbook workbook)
    {
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
        Assert.Equal("ReporteLibros.xlsx", result.FileDownloadName);
        Assert.NotEmpty(result.FileContents);
        var worksheet = Assert.Single(workbook.Worksheets);
        Assert.Equal("Reporte de Libros", worksheet.Name);
        return worksheet;
    }

    private static string[] GetBookTitles(IXLWorksheet worksheet)
    {
        return worksheet.RowsUsed()
            .Where(row => row.RowNumber() > 6)
            .Select(row => row.Cell(2).GetString())
            .ToArray();
    }
}
