using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class AutoresController : Controller
    {
        public IActionResult Index()
        {
            List<Autor> autores =
            [
                new Autor { Id = 1, Nombre = "Gabriel García", Apellido = "Márquez", Nacionalidad = "Colombiana", FechaNacimiento = new DateTime(1927, 3, 6), Activo = true },
                new Autor { Id = 2, Nombre = "Isabel", Apellido = "Allende", Nacionalidad = "Chilena", FechaNacimiento = new DateTime(1942, 8, 2), Activo = true },
                new Autor { Id = 3, Nombre = "Jorge Luis", Apellido = "Borges", Nacionalidad = "Argentina", FechaNacimiento = new DateTime(1899, 8, 24), Activo = false },
                new Autor { Id = 4, Nombre = "Jane", Apellido = "Austen", Nacionalidad = "Británica", FechaNacimiento = new DateTime(1775, 12, 16), Activo = false },
                new Autor { Id = 5, Nombre = "Mario", Apellido = "Vargas Llosa", Nacionalidad = "Peruana", FechaNacimiento = new DateTime(1936, 3, 28), Activo = true }
            ];

            ViewBag.Autores = autores;
            return View();
        }
    }
}
