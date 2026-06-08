using Domain.Models;

namespace Domain.Repositories;

public class InMemoryPacienteRepository : IPacienteRepository
{
    private readonly List<Paciente> _pacientes = [];

    public Task<bool> ExistePorDocumento(string documento) =>
        Task.FromResult(_pacientes.Exists(p => p.Documento == documento));

    public Task Agregar(Paciente paciente)
    {
        _pacientes.Add(paciente);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Paciente>> ObtenerTodos() =>
        Task.FromResult<IReadOnlyList<Paciente>>(_pacientes.AsReadOnly());
}
