using System.Globalization;
using BibliotecaMVC.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BibliotecaMVC.Reports;

public sealed class ReporteLibrosDocument : IDocument
{
    private static readonly CultureInfo CulturaEspanola = CultureInfo.GetCultureInfo("es-ES");
    private readonly IReadOnlyList<Libro> _libros;
    private readonly DateTime _generadoEn;

    public ReporteLibrosDocument(IReadOnlyList<Libro> libros, DateTime generadoEn)
    {
        _libros = libros;
        _generadoEn = generadoEn;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(style => style.FontSize(9).FontColor(Colors.Grey.Darken3));
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer()
                .PaddingTop(12)
                .AlignCenter()
                .Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item()
                .Text("BibliotecaMVC")
                .FontSize(10)
                .SemiBold()
                .FontColor(Colors.Blue.Darken2);

            column.Item()
                .PaddingTop(4)
                .Text("Reporte de Libros")
                .FontSize(22)
                .Bold()
                .FontColor(Colors.Blue.Darken4);

            column.Item()
                .PaddingTop(4)
                .Text($"Generado: {_generadoEn.ToString("dd/MM/yyyy HH:mm", CulturaEspanola)}")
                .FontSize(9)
                .FontColor(Colors.Grey.Darken1);

            column.Item()
                .PaddingTop(12)
                .LineHorizontal(1)
                .LineColor(Colors.Blue.Lighten2);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(18).Column(column =>
        {
            column.Item()
                .Text($"Total de libros: {_libros.Count}")
                .SemiBold()
                .FontColor(Colors.Grey.Darken2);

            if (_libros.Count == 0)
            {
                column.Item()
                    .PaddingTop(16)
                    .Background(Colors.Blue.Lighten5)
                    .Border(1)
                    .BorderColor(Colors.Blue.Lighten2)
                    .Padding(18)
                    .Text("No hay libros registrados en la base de datos.")
                    .FontSize(11)
                    .FontColor(Colors.Blue.Darken3);

                return;
            }

            column.Item()
                .PaddingTop(10)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(42);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "ID", true);
                        HeaderCell(header.Cell(), "Nombre del libro");
                        HeaderCell(header.Cell(), "Autor");
                        HeaderCell(header.Cell(), "Categoría");
                    });

                    for (var index = 0; index < _libros.Count; index++)
                    {
                        var libro = _libros[index];
                        var colorFondo = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        BodyCell(table.Cell(), colorFondo, true).Text(libro.Id.ToString(CultureInfo.InvariantCulture));
                        BodyCell(table.Cell(), colorFondo).Text(libro.Titulo);
                        BodyCell(table.Cell(), colorFondo).Text(ObtenerNombreAutor(libro));
                        BodyCell(table.Cell(), colorFondo).Text(ObtenerNombreCategoria(libro));
                    }
                });
        });
    }

    private static void HeaderCell(IContainer container, string texto, bool centrado = false)
    {
        var cell = container
            .Background(Colors.Blue.Darken2)
            .Border(0.5f)
            .BorderColor(Colors.Blue.Darken1)
            .PaddingVertical(8)
            .PaddingHorizontal(6);

        if (centrado)
        {
            cell.AlignCenter().Text(texto).SemiBold().FontColor(Colors.White);
            return;
        }

        cell.Text(texto).SemiBold().FontColor(Colors.White);
    }

    private static IContainer BodyCell(IContainer container, string colorFondo, bool centrado = false)
    {
        var cell = container
            .Background(colorFondo)
            .Border(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(7)
            .PaddingHorizontal(6);

        return centrado ? cell.AlignCenter() : cell;
    }

    private static string ObtenerNombreAutor(Libro libro)
    {
        var nombre = string.Join(" ", new[] { libro.Autor?.Nombre, libro.Autor?.Apellido }
            .Where(valor => !string.IsNullOrWhiteSpace(valor)));

        return string.IsNullOrWhiteSpace(nombre) ? "Sin autor" : nombre;
    }

    private static string ObtenerNombreCategoria(Libro libro)
    {
        var nombre = libro.Categoria?.Nombre;
        return string.IsNullOrWhiteSpace(nombre) ? "Sin categoría" : nombre;
    }
}
