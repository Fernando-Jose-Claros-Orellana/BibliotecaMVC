using BibliotecaMVC.Models;

namespace BibliotecaMVC.Servicios;

public class AutorServiceAlternativo : IAutorService
{
    private static readonly List<Autor> _autores =
    [
        new Autor { Id = 1, Nombre = "Octavio", Apellido = "Paz", Nacionalidad = "Mexicana", FechaNacimiento = new DateTime(1914, 3, 31), Activo = false },
        new Autor { Id = 2, Nombre = "Virginia", Apellido = "Woolf", Nacionalidad = "Británica", FechaNacimiento = new DateTime(1882, 1, 25), Activo = false },
        new Autor { Id = 3, Nombre = "Haruki", Apellido = "Murakami", Nacionalidad = "Japonesa", FechaNacimiento = new DateTime(1949, 1, 12), Activo = true },
        new Autor { Id = 4, Nombre = "Chinua", Apellido = "Achebe", Nacionalidad = "Nigeriana", FechaNacimiento = new DateTime(1930, 11, 16), Activo = false },
        new Autor { Id = 5, Nombre = "Elena", Apellido = "Poniatowska", Nacionalidad = "Mexicana", FechaNacimiento = new DateTime(1932, 5, 19), Activo = true }
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
