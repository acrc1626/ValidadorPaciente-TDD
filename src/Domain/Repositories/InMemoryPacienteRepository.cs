using Domain.Models;

namespace Domain.Repositories;

public class InMemoryPacienteRepository : IPacienteRepository
{
    private readonly List<Paciente> _pacientes = [];

    public bool ExistePorDocumento(string documento) =>
        _pacientes.Exists(p => p.Documento == documento);

    public void Agregar(Paciente paciente) => _pacientes.Add(paciente);

    public IReadOnlyList<Paciente> ObtenerTodos() => _pacientes.AsReadOnly();
}
