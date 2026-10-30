using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly ILogger<LibrosController> _logger;

        public LibrosController(BibliotecaContext context, ILogger<LibrosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "admin,bibliotecario,usuario")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Starting book list retrieval.");

            try
            {
                var libros = await _context.Libros
                    .Include(l => l.Autor)
                    .Include(l => l.Categoria)
                    .OrderBy(l => l.Titulo)
                    .ToListAsync();

                _logger.LogInformation("Book list retrieved successfully. Count: {BookCount}", libros.Count);
                return View(libros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the book list.");
                throw;
            }
        }

        [Authorize(Roles = "admin,bibliotecario")]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Opening the create book form.");

            try
            {
                await CargarListasAsync();
                _logger.LogInformation("Book create form loaded successfully.");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while preparing the create book form.");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,bibliotecario")]
        public async Task<IActionResult> Create(Libro libro)
        {
            _logger.LogInformation("Starting book creation.");

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Book creation was rejected because the submitted model is invalid.");
                    await CargarListasAsync(libro.AutorId, libro.CategoriaId);
                    return View(libro);
                }

                _context.Libros.Add(libro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book created successfully. {LibroId}", libro.Id);
                TempData["SuccessMessage"] = "Libro guardado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a book.");
                throw;
            }
        }

        [Authorize(Roles = "admin,bibliotecario")]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Starting book edit lookup. {LibroId}", id);

            try
            {
                var libro = await _context.Libros.FindAsync(id);

                if (libro == null)
                {
                    _logger.LogWarning("Book not found for editing. {LibroId}", id);
                    return NotFound();
                }

                await CargarListasAsync(libro.AutorId, libro.CategoriaId);
                _logger.LogInformation("Book edit form loaded successfully. {LibroId}", id);
                return View(libro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading a book for editing. {LibroId}", id);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,bibliotecario")]
        public async Task<IActionResult> Edit(int id, Libro libro)
        {
            _logger.LogInformation("Starting book update. {LibroId}", id);

            try
            {
                if (id != libro.Id)
                {
                    _logger.LogWarning("Book update rejected because route and model IDs do not match. {LibroId}", id);
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Book update rejected because the submitted model is invalid. {LibroId}", id);
                    await CargarListasAsync(libro.AutorId, libro.CategoriaId);
                    return View(libro);
                }

                var existe = await _context.Libros.AnyAsync(l => l.Id == id);

                if (!existe)
                {
                    _logger.LogWarning("Book not found for update. {LibroId}", id);
                    return NotFound();
                }

                _context.Libros.Update(libro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book updated successfully. {LibroId}", id);
                TempData["SuccessMessage"] = "Libro actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a book. {LibroId}", id);
                throw;
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,bibliotecario")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Starting book deletion. {LibroId}", id);

            try
            {
                var libro = await _context.Libros.FindAsync(id);

                if (libro == null)
                {
                    _logger.LogWarning("Book not found for deletion. {LibroId}", id);
                    return NotFound();
                }

                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book deleted successfully. {LibroId}", id);
                TempData["SuccessMessage"] = "Libro eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a book. {LibroId}", id);
                throw;
            }
        }

        private async Task CargarListasAsync(int? autorSeleccionado = null, int? categoriaSeleccionada = null)
        {
            ViewBag.Autores = new SelectList(
                await _context.Autores.OrderBy(a => a.Nombre).ToListAsync(),
                "Id",
                "Nombre",
                autorSeleccionado);

            ViewBag.Categorias = new SelectList(
                await _context.Set<Categoria>().OrderBy(c => c.Nombre).ToListAsync(),
                "Id",
                "Nombre",
                categoriaSeleccionada);
        }

    }
}
