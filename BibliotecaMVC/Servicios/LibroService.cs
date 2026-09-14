using BibliotecaMVC.Models;

namespace BibliotecaMVC.Servicios;

public class LibroService : ILibroService
{
    private static readonly List<Libro> _libros =
    [
        new Libro { Id = 1, Titulo = "Clean Code", AutorId = 1, CategoriaId = 1, Precio = 35.5m, Disponible = true },
        new Libro { Id = 2, Titulo = "Cien años de soledad", AutorId = 1, CategoriaId = 1, Precio = 18, Disponible = false }
    ];

    public IEnumerable<Libro> ObtenerTodos()
    {
        return _libros;
    }

    public Libro? ObtenerPorId(int id)
    {
        return _libros.FirstOrDefault(libro => libro.Id == id);
    }

    public void Agregar(Libro libro)
    {
        _libros.Add(libro);
    }

    public void Actualizar(Libro libro)
    {
        Libro? libroExistente = _libros.FirstOrDefault(l => l.Id == libro.Id);

        if (libroExistente == null)
        {
            return;
        }

        libroExistente.Titulo = libro.Titulo;
        libroExistente.AutorId = libro.AutorId;
        libroExistente.CategoriaId = libro.CategoriaId;
        libroExistente.Precio = libro.Precio;
        libroExistente.Disponible = libro.Disponible;
        libroExistente.ImagenUrl = libro.ImagenUrl;
    }

    public void Eliminar(int id)
    {
        Libro? libro = _libros.FirstOrDefault(l => l.Id == id);

        if (libro != null)
        {
            _libros.Remove(libro);
        }
    }

    public int ObtenerSiguienteId()
    {
        return _libros.Any() ? _libros.Max(l => l.Id) + 1 : 1;
    }
}
