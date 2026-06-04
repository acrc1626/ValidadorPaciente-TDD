using Domain.Models;

namespace Domain.Repositories;

public interface IPacienteRepository
{
    bool ExistePorDocumento(string documento);
    void Agregar(Paciente paciente);
    IReadOnlyList<Paciente> ObtenerTodos();
}
