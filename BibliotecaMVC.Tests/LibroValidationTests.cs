using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;
using Xunit;

namespace BibliotecaMVC.Tests;

public class LibroValidationTests
{
    [Fact]
    public void Titulo_IsRequired()
    {
        var libro = CreateValidLibro();
        libro.Titulo = "";

        var results = Validate(libro);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(Libro.Titulo)));
    }

    [Theory]
    [InlineData(150, true)]
    [InlineData(151, false)]
    public void Titulo_EnforcesMaximumLength(int length, bool expectedValid)
    {
        var libro = CreateValidLibro();
        libro.Titulo = new string('T', length);

        Assert.Equal(expectedValid, IsValid(libro));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Isbn_IsOptionalAndHasMaximumLength(int? length, bool expectedValid)
    {
        var libro = CreateValidLibro();
        libro.ISBN = length is null ? null : new string('I', length.Value);

        Assert.Equal(expectedValid, IsValid(libro));
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(9999, true)]
    [InlineData(10000, false)]
    [InlineData(null, true)]
    public void AnioPublicacion_MustBeWithinRangeOrNull(int? year, bool expectedValid)
    {
        var libro = CreateValidLibro();
        libro.AnioPublicacion = year;

        Assert.Equal(expectedValid, IsValid(libro));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    public void AutorId_MustBeGreaterThanZero(int autorId, bool expectedValid)
    {
        var libro = CreateValidLibro();
        libro.AutorId = autorId;

        Assert.Equal(expectedValid, IsValid(libro));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    public void CategoriaId_MustBeGreaterThanZero(int categoriaId, bool expectedValid)
    {
        var libro = CreateValidLibro();
        libro.CategoriaId = categoriaId;

        Assert.Equal(expectedValid, IsValid(libro));
    }

    private static Libro CreateValidLibro()
    {
        return new Libro
        {
            Titulo = "Libro válido",
            AutorId = 1,
            CategoriaId = 1
        };
    }

    private static bool IsValid(Libro libro)
    {
        return Validate(libro).Count == 0;
    }

    private static List<ValidationResult> Validate(Libro libro)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(libro, new ValidationContext(libro), results, validateAllProperties: true);
        return results;
    }
}
