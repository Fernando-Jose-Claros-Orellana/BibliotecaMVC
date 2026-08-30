using BibliotecaMVC.Models;

namespace BibliotecaMVC.Servicios;

public interface ILibroService
{
    IEnumerable<Libro> ObtenerTodos();
    Libro? ObtenerPorId(int id);
    void Agregar(Libro libro);
    void Actualizar(Libro libro);
    void Eliminar(int id);
    int ObtenerSiguienteId();
}
