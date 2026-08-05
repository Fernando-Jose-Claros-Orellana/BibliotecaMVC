using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        private static List<Libro> _libros =
        [
            new Libro { Id = 1, Titulo = "Clean Code", Autor = "Robert Martin", Categoria = "Programación", Precio = 35.5m, Disponible = true },
            new Libro { Id = 2, Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Literatura", Precio = 18, Disponible = false }
        ];

        public IActionResult Index()
        {
            return View(_libros);
        }

        public IActionResult Details(int id)
        {
            Libro? libro = _libros.FirstOrDefault(l => l.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            libro.Id = _libros.Any() ? _libros.Max(l => l.Id) + 1 : 1;
            _libros.Add(libro);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Libro? libro = _libros.FirstOrDefault(l => l.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            Libro? libroExistente = _libros.FirstOrDefault(l => l.Id == libro.Id);

            if (libroExistente == null)
            {
                return NotFound();
            }

            libroExistente.Titulo = libro.Titulo;
            libroExistente.Autor = libro.Autor;
            libroExistente.Categoria = libro.Categoria;
            libroExistente.Precio = libro.Precio;
            libroExistente.Disponible = libro.Disponible;

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            Libro? libro = _libros.FirstOrDefault(l => l.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Libro? libro = _libros.FirstOrDefault(l => l.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            _libros.Remove(libro);
            return RedirectToAction(nameof(Index));
        }

    }
}
