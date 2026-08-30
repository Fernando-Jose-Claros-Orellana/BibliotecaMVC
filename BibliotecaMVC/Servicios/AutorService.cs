using BibliotecaMVC.Models;

namespace BibliotecaMVC.Servicios;

public class AutorService : IAutorService
{
    private static readonly List<Autor> _autores =
    [
        new Autor { Id = 1, Nombre = "Gabriel García", Apellido = "Márquez", Nacionalidad = "Colombiana", FechaNacimiento = new DateTime(1927, 3, 6), Activo = true },
        new Autor { Id = 2, Nombre = "Isabel", Apellido = "Allende", Nacionalidad = "Chilena", FechaNacimiento = new DateTime(1942, 8, 2), Activo = true },
        new Autor { Id = 3, Nombre = "Jorge Luis", Apellido = "Borges", Nacionalidad = "Argentina", FechaNacimiento = new DateTime(1899, 8, 24), Activo = false },
        new Autor { Id = 4, Nombre = "Jane", Apellido = "Austen", Nacionalidad = "Británica", FechaNacimiento = new DateTime(1775, 12, 16), Activo = false },
        new Autor { Id = 5, Nombre = "Mario", Apellido = "Vargas Llosa", Nacionalidad = "Peruana", FechaNacimiento = new DateTime(1936, 3, 28), Activo = true }
    ];

    public IEnumerable<Autor> ObtenerTodos()
    {
        return _autores;
    }

    public Autor? ObtenerPorId(int id)
    {
        return _autores.FirstOrDefault(autor => autor.Id == id);
    }

    public void Agregar(Autor autor)
    {
        _autores.Add(autor);
    }

    public void Actualizar(Autor autor)
    {
        Autor? autorExistente = _autores.FirstOrDefault(a => a.Id == autor.Id);

        if (autorExistente == null)
        {
            return;
        }

        autorExistente.Nombre = autor.Nombre;
        autorExistente.Apellido = autor.Apellido;
        autorExistente.Nacionalidad = autor.Nacionalidad;
        autorExistente.FechaNacimiento = autor.FechaNacimiento;
        autorExistente.Activo = autor.Activo;
    }

    public void Eliminar(int id)
    {
        Autor? autor = _autores.FirstOrDefault(a => a.Id == id);

        if (autor != null)
        {
            _autores.Remove(autor);
        }
    }

    public int ObtenerSiguienteId()
    {
        return _autores.Any() ? _autores.Max(a => a.Id) + 1 : 1;
    }
}
