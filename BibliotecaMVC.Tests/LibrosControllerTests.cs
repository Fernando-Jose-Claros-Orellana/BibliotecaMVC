using System.Collections.Generic;
using BibliotecaMVC.Controllers;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BibliotecaMVC.Tests;

public class LibrosControllerTests
{
    [Fact]
    public async Task Index_ReturnsBooksOrderedByTitle()
    {
        using var context = ControllerTestContext.CreateDatabase();
        context.Libros.AddRange(
            ControllerTestContext.CreateLibro(1, "Zeta"),
            ControllerTestContext.CreateLibro(2, "Alfa"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var libros = Assert.IsAssignableFrom<IEnumerable<Libro>>(view.Model).ToList();
        Assert.Equal(new[] { "Alfa", "Zeta" }, libros.Select(libro => libro.Titulo));
        Assert.All(libros, libro =>
        {
            Assert.NotNull(libro.Autor);
            Assert.NotNull(libro.Categoria);
        });
    }

    [Fact]
    public async Task Index_ReturnsEmptyCollectionWhenThereAreNoBooks()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Empty(Assert.IsAssignableFrom<IEnumerable<Libro>>(view.Model));
    }

    [Fact]
    public async Task CreateGet_ReturnsViewWithAuthorsAndCategoriesOrderedByName()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.Create();

        Assert.IsType<ViewResult>(result);
        var autores = Assert.IsAssignableFrom<IEnumerable<SelectListItem>>(controller.ViewData["Autores"]);
        var categorias = Assert.IsAssignableFrom<IEnumerable<SelectListItem>>(controller.ViewData["Categorias"]);
        Assert.Equal(new[] { "Ana", "Zoe" }, autores.Select(item => item.Text));
        Assert.Equal(new[] { "Arte", "Zoología" }, categorias.Select(item => item.Text));
    }

    [Fact]
    public async Task CreatePost_ValidBookPersistsRedirectsAndSetsSuccessMessage()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var logger = new Mock<ILogger<LibrosController>>();
        var controller = ControllerTestContext.CreateController(context, logger.Object);
        var libro = ControllerTestContext.CreateLibro(titulo: "Nuevo libro");

        var result = await controller.Create(libro);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibrosController.Index), redirect.ActionName);
        Assert.Equal("Libro guardado correctamente.", controller.TempData["SuccessMessage"]);
        var saved = await context.Libros.AsNoTracking().SingleAsync();
        Assert.Equal("Nuevo libro", saved.Titulo);
        VerifyLog(logger, LogLevel.Information, "Book created successfully.");
    }

    [Fact]
    public async Task CreatePost_InvalidModelDoesNotPersistAndReloadsLists()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var logger = new Mock<ILogger<LibrosController>>();
        var controller = ControllerTestContext.CreateController(context, logger.Object);
        var libro = ControllerTestContext.CreateLibro(titulo: "");
        libro.AutorId = 2;
        libro.CategoriaId = 2;
        controller.ModelState.AddModelError(nameof(Libro.Titulo), "El título es obligatorio.");

        var result = await controller.Create(libro);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(libro, view.Model);
        Assert.Empty(await context.Libros.ToListAsync());
        Assert.Equal("2", Assert.IsType<SelectList>(controller.ViewData["Autores"]).SelectedValue?.ToString());
        Assert.Equal("2", Assert.IsType<SelectList>(controller.ViewData["Categorias"]).SelectedValue?.ToString());
        VerifyLog(logger, LogLevel.Warning, "Book creation was rejected");
    }

    [Fact]
    public async Task EditGet_ExistingBookReturnsViewWithSelectedRelatedLists()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var libro = ControllerTestContext.CreateLibro(8, "Libro existente", 2, 2);
        context.Libros.Add(libro);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.Edit(libro.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(libro.Id, Assert.IsType<Libro>(view.Model).Id);
        Assert.Equal("2", Assert.IsType<SelectList>(controller.ViewData["Autores"]).SelectedValue?.ToString());
        Assert.Equal("2", Assert.IsType<SelectList>(controller.ViewData["Categorias"]).SelectedValue?.ToString());
        Assert.Equal(new[] { "Ana", "Zoe" },
            Assert.IsAssignableFrom<IEnumerable<SelectListItem>>(controller.ViewData["Autores"]).Select(item => item.Text));
        Assert.Equal(new[] { "Arte", "Zoología" },
            Assert.IsAssignableFrom<IEnumerable<SelectListItem>>(controller.ViewData["Categorias"]).Select(item => item.Text));
    }

    [Fact]
    public async Task EditGet_MissingBookReturnsNotFound()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.Edit(404);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_MismatchedIdsReturnsBadRequest()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.Edit(1, ControllerTestContext.CreateLibro(2));

        Assert.IsType<BadRequestResult>(result);
        Assert.Empty(await context.Libros.ToListAsync());
    }

    [Fact]
    public async Task EditPost_InvalidModelDoesNotModifyBookAndReloadsLists()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var original = ControllerTestContext.CreateLibro(5, "Título original");
        context.Libros.Add(original);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());
        var submitted = ControllerTestContext.CreateLibro(5, "Título modificado", 2, 2);
        controller.ModelState.AddModelError(nameof(Libro.Titulo), "Título inválido.");

        var result = await controller.Edit(5, submitted);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(submitted, view.Model);
        Assert.Equal("Título original", (await context.Libros.AsNoTracking().SingleAsync()).Titulo);
        Assert.Equal("2", Assert.IsType<SelectList>(controller.ViewData["Autores"]).SelectedValue?.ToString());
        Assert.Equal("2", Assert.IsType<SelectList>(controller.ViewData["Categorias"]).SelectedValue?.ToString());
    }

    [Fact]
    public async Task EditPost_ExistingBookIsUpdatedAndRedirectsToIndex()
    {
        using var context = ControllerTestContext.CreateDatabase();
        context.Libros.Add(ControllerTestContext.CreateLibro(5, "Título original"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());
        var submitted = ControllerTestContext.CreateLibro(5, "Título actualizado", 2, 2);

        var result = await controller.Edit(5, submitted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibrosController.Index), redirect.ActionName);
        Assert.Equal("Libro actualizado correctamente.", controller.TempData["SuccessMessage"]);
        var updated = await context.Libros.AsNoTracking().SingleAsync();
        Assert.Equal("Título actualizado", updated.Titulo);
        Assert.Equal(2, updated.AutorId);
        Assert.Equal(2, updated.CategoriaId);
    }

    [Fact]
    public async Task EditPost_MissingBookReturnsNotFound()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.Edit(404, ControllerTestContext.CreateLibro(404));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_ExistingBookIsRemovedAndRedirectsToIndex()
    {
        using var context = ControllerTestContext.CreateDatabase();
        context.Libros.Add(ControllerTestContext.CreateLibro(9, "Libro a eliminar"));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.DeleteConfirmed(9);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(LibrosController.Index), redirect.ActionName);
        Assert.Equal("Libro eliminado correctamente.", controller.TempData["SuccessMessage"]);
        Assert.False(await context.Libros.AnyAsync(libro => libro.Id == 9));
    }

    [Fact]
    public async Task DeleteConfirmed_MissingBookReturnsNotFound()
    {
        using var context = ControllerTestContext.CreateDatabase();
        var controller = ControllerTestContext.CreateController(context, Mock.Of<ILogger<LibrosController>>());

        var result = await controller.DeleteConfirmed(404);

        Assert.IsType<NotFoundResult>(result);
        Assert.Empty(await context.Libros.ToListAsync());
    }

    private static void VerifyLog(
        Mock<ILogger<LibrosController>> logger,
        LogLevel level,
        string message)
    {
        logger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(message)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
