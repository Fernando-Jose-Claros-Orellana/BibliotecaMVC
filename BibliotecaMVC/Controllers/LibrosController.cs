using BibliotecaMVC.Models;
using BibliotecaMVC.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        private readonly ILibroService _libroService;

        public LibrosController(ILibroService libroService)
        {
            _libroService = libroService;
        }

        public IActionResult Index()
        {
            var libros = _libroService.ObtenerTodos();
            return View(libros);
        }

        public IActionResult Details(int id)
        {
            Libro? libro = _libroService.ObtenerPorId(id);

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

            libro.Id = _libroService.ObtenerSiguienteId();

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

            _libroService.Agregar(libro);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Libro? libro = _libroService.ObtenerPorId(id);

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

            Libro? libroExistente = _libroService.ObtenerPorId(libro.Id);

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

            _libroService.Actualizar(libroExistente);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            Libro? libro = _libroService.ObtenerPorId(id);

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
            Libro? libro = _libroService.ObtenerPorId(id);

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

            _libroService.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
