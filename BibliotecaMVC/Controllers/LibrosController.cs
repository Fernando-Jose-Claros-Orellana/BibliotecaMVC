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
        public async Task<IActionResult> Create(Libro libro, IFormFile? imagen)
        {
            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            libro.Id = _libros.Any() ? _libros.Max(l => l.Id) + 1 : 1;

            if (imagen != null && imagen.Length > 0)
            {
                string carpetaImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(carpetaImagenes);

                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                string rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);

                using (FileStream stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                libro.ImagenUrl = "/images/" + nombreArchivo;
            }

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
        public async Task<IActionResult> Edit(Libro libro, IFormFile? imagen)
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

            if (imagen != null && imagen.Length > 0)
            {
                string carpetaImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(carpetaImagenes);

                string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                string rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);

                using (FileStream stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                string? imagenAnterior = libroExistente.ImagenUrl;
                libroExistente.ImagenUrl = "/images/" + nombreArchivo;

                if (!string.IsNullOrEmpty(imagenAnterior))
                {
                    string nombreAnterior = Path.GetFileName(imagenAnterior);
                    string rutaAnterior = Path.Combine(carpetaImagenes, nombreAnterior);

                    if (System.IO.File.Exists(rutaAnterior))
                    {
                        System.IO.File.Delete(rutaAnterior);
                    }
                }
            }

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

            if (!string.IsNullOrEmpty(libro.ImagenUrl))
            {
                string carpetaImagenes = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                string nombreArchivo = Path.GetFileName(libro.ImagenUrl);
                string rutaArchivo = Path.Combine(carpetaImagenes, nombreArchivo);

                if (System.IO.File.Exists(rutaArchivo))
                {
                    System.IO.File.Delete(rutaArchivo);
                }
            }

            _libros.Remove(libro);
            return RedirectToAction(nameof(Index));
        }

    }
}
