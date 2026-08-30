using BibliotecaMVC.Models;

namespace BibliotecaMVC.Servicios;

public interface IAutorService
{
    IEnumerable<Autor> ObtenerTodos();
    Autor? ObtenerPorId(int id);
    void Agregar(Autor autor);
    void Actualizar(Autor autor);
    void Eliminar(int id);
    int ObtenerSiguienteId();
}
