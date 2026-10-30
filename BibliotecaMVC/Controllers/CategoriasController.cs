using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BibliotecaMVC.Controllers;

public class CategoriasController : Controller
{
    private readonly string _connectionString;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(IConfiguration configuration, ILogger<CategoriasController> logger)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión BibliotecaDB.");
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Starting category list retrieval.");
        List<Categoria> categorias = new();

        try
        {
            using SqlConnection conexion = new(_connectionString);
            using SqlCommand comando = new(@"SELECT Id, Nombre, Descripcion FROM dbo.Categorias ORDER BY Id;", conexion);

            conexion.Open();
            using SqlDataReader lector = comando.ExecuteReader();

            while (lector.Read())
            {
                categorias.Add(new Categoria
                {
                    Id = lector.GetInt32(0),
                    Nombre = lector.GetString(1),
                    Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2)
                });
            }

            _logger.LogInformation("Category list retrieved successfully. Count: {CategoriaCount}", categorias.Count);
            return View(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the category list.");
            throw;
        }
    }

    public IActionResult Create()
    {
        _logger.LogInformation("Category create form displayed.");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Categoria categoria)
    {
        _logger.LogInformation("Starting category creation.");

        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            _logger.LogWarning("Category creation was rejected because the name is empty.");
            ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            return View(categoria);
        }

        try
        {
            using SqlConnection conexion = new(_connectionString);
            using SqlCommand comando = new(@"INSERT INTO dbo.Categorias (Nombre, Descripcion) VALUES (@Nombre, @Descripcion);", conexion);
            comando.Parameters.AddWithValue("@Nombre", categoria.Nombre.Trim());
            comando.Parameters.AddWithValue("@Descripcion", (object?)categoria.Descripcion?.Trim() ?? DBNull.Value);

            conexion.Open();
            comando.ExecuteNonQuery();

            _logger.LogInformation("Category created successfully.");
            TempData["SuccessMessage"] = "Categoría guardada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a category.");
            throw;
        }
    }

    public IActionResult Edit(int id)
    {
        _logger.LogInformation("Starting category edit lookup. {CategoriaId}", id);

        try
        {
            Categoria? categoria = ObtenerPorId(id);
            if (categoria == null)
            {
                _logger.LogWarning("Category not found for editing. {CategoriaId}", id);
                return NotFound();
            }

            _logger.LogInformation("Category edit form loaded successfully. {CategoriaId}", id);
            return View(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loading a category for editing. {CategoriaId}", id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Categoria categoria)
    {
        _logger.LogInformation("Starting category update. {CategoriaId}", categoria.Id);

        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            _logger.LogWarning("Category update was rejected because the name is empty. {CategoriaId}", categoria.Id);
            ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            return View(categoria);
        }

        try
        {
            using SqlConnection conexion = new(_connectionString);
            using SqlCommand comando = new(@"UPDATE dbo.Categorias SET Nombre = @Nombre, Descripcion = @Descripcion WHERE Id = @Id;", conexion);
            comando.Parameters.AddWithValue("@Id", categoria.Id);
            comando.Parameters.AddWithValue("@Nombre", categoria.Nombre.Trim());
            comando.Parameters.AddWithValue("@Descripcion", (object?)categoria.Descripcion?.Trim() ?? DBNull.Value);

            conexion.Open();
            int filas = comando.ExecuteNonQuery();
            if (filas == 0)
            {
                _logger.LogWarning("Category not found for update. {CategoriaId}", categoria.Id);
                return NotFound();
            }

            _logger.LogInformation("Category updated successfully. {CategoriaId}", categoria.Id);
            TempData["SuccessMessage"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating a category. {CategoriaId}", categoria.Id);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation("Starting category deletion. {CategoriaId}", id);

        try
        {
            using SqlConnection conexion = new(_connectionString);
            using SqlCommand comando = new(@"DELETE FROM dbo.Categorias WHERE Id = @Id;", conexion);
            comando.Parameters.AddWithValue("@Id", id);

            conexion.Open();
            int filas = comando.ExecuteNonQuery();
            if (filas == 0)
            {
                _logger.LogWarning("Category not found for deletion. {CategoriaId}", id);
                return NotFound();
            }

            _logger.LogInformation("Category deleted successfully. {CategoriaId}", id);
            TempData["SuccessMessage"] = "Categoría eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting a category. {CategoriaId}", id);
            throw;
        }
    }

    private Categoria? ObtenerPorId(int id)
    {
        using SqlConnection conexion = new(_connectionString);
        using SqlCommand comando = new(@"SELECT Id, Nombre, Descripcion FROM dbo.Categorias WHERE Id = @Id;", conexion);
        comando.Parameters.AddWithValue("@Id", id);

        conexion.Open();
        using SqlDataReader lector = comando.ExecuteReader();

        if (!lector.Read())
        {
            return null;
        }

        return new Categoria
        {
            Id = lector.GetInt32(0),
            Nombre = lector.GetString(1),
            Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2)
        };
    }
}
