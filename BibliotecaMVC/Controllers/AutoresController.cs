using BibliotecaMVC.Models;
using BibliotecaMVC.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class AutoresController : Controller
    {
        private readonly IAutorService _autorService;

        public AutoresController(IAutorService autorService)
        {
            _autorService = autorService;
        }

        public IActionResult Index()
        {
            return View(_autorService.ObtenerTodos());
        }

        public IActionResult Details(int id)
        {
            Autor? autor = _autorService.ObtenerPorId(id);

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

            autor.Id = _autorService.ObtenerSiguienteId();
            _autorService.Agregar(autor);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            Autor? autor = _autorService.ObtenerPorId(id);

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

            Autor? autorExistente = _autorService.ObtenerPorId(autor.Id);

            if (autorExistente == null)
            {
                return NotFound();
            }

            autorExistente.Nombre = autor.Nombre;
            autorExistente.Apellido = autor.Apellido;
            autorExistente.Nacionalidad = autor.Nacionalidad;
            autorExistente.FechaNacimiento = autor.FechaNacimiento;
            autorExistente.Activo = autor.Activo;

            _autorService.Actualizar(autorExistente);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            Autor? autor = _autorService.ObtenerPorId(id);

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
            Autor? autor = _autorService.ObtenerPorId(id);

            if (autor == null)
            {
                return NotFound();
            }

            _autorService.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
