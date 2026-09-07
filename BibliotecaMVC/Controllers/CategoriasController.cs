using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BibliotecaMVC.Controllers;

public class CategoriasController : Controller
{
    private readonly string _connectionString;

    public CategoriasController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión BibliotecaDB.");
    }

    public IActionResult Index()
    {
        List<Categoria> categorias = new();

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

        return View(categorias);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            return View(categoria);
        }

        using SqlConnection conexion = new(_connectionString);
        using SqlCommand comando = new(@"INSERT INTO dbo.Categorias (Nombre, Descripcion) VALUES (@Nombre, @Descripcion);", conexion);
        comando.Parameters.AddWithValue("@Nombre", categoria.Nombre.Trim());
        comando.Parameters.AddWithValue("@Descripcion", (object?)categoria.Descripcion?.Trim() ?? DBNull.Value);

        conexion.Open();
        comando.ExecuteNonQuery();

        TempData["SuccessMessage"] = "Categoría guardada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        Categoria? categoria = ObtenerPorId(id);
        if (categoria == null)
        {
            return NotFound();
        }

        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            return View(categoria);
        }

        using SqlConnection conexion = new(_connectionString);
        using SqlCommand comando = new(@"UPDATE dbo.Categorias SET Nombre = @Nombre, Descripcion = @Descripcion WHERE Id = @Id;", conexion);
        comando.Parameters.AddWithValue("@Id", categoria.Id);
        comando.Parameters.AddWithValue("@Nombre", categoria.Nombre.Trim());
        comando.Parameters.AddWithValue("@Descripcion", (object?)categoria.Descripcion?.Trim() ?? DBNull.Value);

        conexion.Open();
        int filas = comando.ExecuteNonQuery();
        if (filas == 0)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Categoría actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        using SqlConnection conexion = new(_connectionString);
        using SqlCommand comando = new(@"DELETE FROM dbo.Categorias WHERE Id = @Id;", conexion);
        comando.Parameters.AddWithValue("@Id", id);

        conexion.Open();
        int filas = comando.ExecuteNonQuery();
        if (filas == 0)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Categoría eliminada correctamente.";
        return RedirectToAction(nameof(Index));
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
