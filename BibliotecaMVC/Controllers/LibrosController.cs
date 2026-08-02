using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        public IActionResult Index()
        {
            List<Libro> libros =
            [
                new Libro { Id = 1, Titulo = "Clean Code", Autor = "Robert Martin", Categoria = "Programación", Precio = 35.5m, Disponible = true },
                new Libro { Id = 2, Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Literatura", Precio = 18, Disponible = false }
            ];

            ViewBag.Libros = libros;
            return View();
        }
    }
}
