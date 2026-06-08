using Domain.Models;

namespace Domain.Repositories;

public interface IPacienteRepository
{
    Task<bool> ExistePorDocumento(string documento);
    Task Agregar(Paciente paciente);
    Task<IReadOnlyList<Paciente>> ObtenerTodos();
}
