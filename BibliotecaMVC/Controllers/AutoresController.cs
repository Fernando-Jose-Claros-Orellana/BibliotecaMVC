using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class AutoresController : Controller
    {
        private static List<Autor> _autores =
        [
            new Autor { Id = 1, Nombre = "Gabriel García", Apellido = "Márquez", Nacionalidad = "Colombiana", FechaNacimiento = new DateTime(1927, 3, 6), Activo = true },
            new Autor { Id = 2, Nombre = "Isabel", Apellido = "Allende", Nacionalidad = "Chilena", FechaNacimiento = new DateTime(1942, 8, 2), Activo = true },
            new Autor { Id = 3, Nombre = "Jorge Luis", Apellido = "Borges", Nacionalidad = "Argentina", FechaNacimiento = new DateTime(1899, 8, 24), Activo = false },
            new Autor { Id = 4, Nombre = "Jane", Apellido = "Austen", Nacionalidad = "Británica", FechaNacimiento = new DateTime(1775, 12, 16), Activo = false },
            new Autor { Id = 5, Nombre = "Mario", Apellido = "Vargas Llosa", Nacionalidad = "Peruana", FechaNacimiento = new DateTime(1936, 3, 28), Activo = true }
        ];

        public IActionResult Index()
        {
            return View(_autores);
        }

        public IActionResult Details(int id)
        {
            Autor? autor = _autores.FirstOrDefault(a => a.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Autor autor)
        {
            if (!ModelState.IsValid)
            {
                return View(autor);
            }

            autor.Id = _autores.Any() ? _autores.Max(a => a.Id) + 1 : 1;
            _autores.Add(autor);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Autor? autor = _autores.FirstOrDefault(a => a.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Autor autor)
        {
            if (!ModelState.IsValid)
            {
                return View(autor);
            }

            Autor? autorExistente = _autores.FirstOrDefault(a => a.Id == autor.Id);

            if (autorExistente == null)
            {
                return NotFound();
            }

            autorExistente.Nombre = autor.Nombre;
            autorExistente.Apellido = autor.Apellido;
            autorExistente.Nacionalidad = autor.Nacionalidad;
            autorExistente.FechaNacimiento = autor.FechaNacimiento;
            autorExistente.Activo = autor.Activo;

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            Autor? autor = _autores.FirstOrDefault(a => a.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Autor? autor = _autores.FirstOrDefault(a => a.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            _autores.Remove(autor);
            return RedirectToAction(nameof(Index));
        }
    }
}
