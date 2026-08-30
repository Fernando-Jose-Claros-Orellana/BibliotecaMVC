using BibliotecaMVC.Models;

namespace BibliotecaMVC.Servicios;

public class LibroService : ILibroService
{
    private static readonly List<Libro> _libros =
    [
        new Libro { Id = 1, Titulo = "Clean Code", Autor = "Robert Martin", Categoria = "Programación", Precio = 35.5m, Disponible = true },
        new Libro { Id = 2, Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Literatura", Precio = 18, Disponible = false }
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
        libroExistente.Autor = libro.Autor;
        libroExistente.Categoria = libro.Categoria;
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
