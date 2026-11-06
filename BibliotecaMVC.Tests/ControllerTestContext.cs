using BibliotecaMVC.Controllers;
using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace BibliotecaMVC.Tests;

internal static class ControllerTestContext
{
    public static BibliotecaContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<BibliotecaContext>()
            .UseInMemoryDatabase($"BibliotecaMVC.Tests.{Guid.NewGuid()}")
            .Options;

        var context = new BibliotecaContext(options);
        context.Autores.AddRange(
            new Autor { Id = 1, Nombre = "Zoe", Apellido = "Autor" },
            new Autor { Id = 2, Nombre = "Ana", Apellido = "Autora" });
        context.Set<Categoria>().AddRange(
            new Categoria { Id = 1, Nombre = "Zoología" },
            new Categoria { Id = 2, Nombre = "Arte" });
        context.SaveChanges();

        return context;
    }

    public static LibrosController CreateController(
        BibliotecaContext context,
        ILogger<LibrosController> logger)
    {
        var httpContext = new DefaultHttpContext();
        var modelState = new ModelStateDictionary();
        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerName = "Libros",
            ActionName = "Index",
            ControllerTypeInfo = typeof(LibrosController).GetTypeInfo()
        };
        var controller = new LibrosController(context, logger)
        {
            ControllerContext = new ControllerContext(
                new ActionContext(httpContext, new RouteData(), actionDescriptor, modelState)),
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelState),
            TempData = new TempDataDictionary(httpContext, CreateTempDataProvider())
        };

        return controller;
    }

    public static Libro CreateLibro(
        int id = 1,
        string titulo = "Libro de prueba",
        int autorId = 1,
        int categoriaId = 1)
    {
        return new Libro
        {
            Id = id,
            Titulo = titulo,
            AutorId = autorId,
            CategoriaId = categoriaId
        };
    }

    private static ITempDataProvider CreateTempDataProvider()
    {
        var provider = new Mock<ITempDataProvider>();
        provider.Setup(x => x.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());
        return provider.Object;
    }
}
