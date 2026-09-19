using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var libros = await _context.Libros
                .Include(l => l.Autor)
                .Include(l => l.Categoria)
                .OrderBy(l => l.Titulo)
                .ToListAsync();

            return View(libros);
        }

        public async Task<IActionResult> Create()
        {
            await CargarListasAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasAsync(libro.AutorId, libro.CategoriaId);
                return View(libro);
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Libro guardado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var libro = await _context.Libros.FindAsync(id);

            if (libro == null)
            {
                return NotFound();
            }

            await CargarListasAsync(libro.AutorId, libro.CategoriaId);
            return View(libro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Libro libro)
        {
            if (id != libro.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await CargarListasAsync(libro.AutorId, libro.CategoriaId);
                return View(libro);
            }

            var existe = await _context.Libros.AnyAsync(l => l.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _context.Libros.Update(libro);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Libro actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var libro = await _context.Libros.FindAsync(id);

            if (libro == null)
            {
                return NotFound();
            }

            _context.Libros.Remove(libro);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Libro eliminado correctamente.";
            return RedirectToAction(nameof(Index));
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
