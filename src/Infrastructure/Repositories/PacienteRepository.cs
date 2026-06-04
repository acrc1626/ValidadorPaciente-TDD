using Domain.Models;
using Domain.Repositories;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly PacienteDbContext _context;

    public PacienteRepository(PacienteDbContext context)
    {
        _context = context;
    }

    public bool ExistePorDocumento(string documento) =>
        _context.Pacientes.Any(p => p.Documento == documento);

    public void Agregar(Paciente paciente)
    {
        _context.Pacientes.Add(paciente);
        _context.SaveChanges();
    }

    public IReadOnlyList<Paciente> ObtenerTodos() =>
        _context.Pacientes.ToList().AsReadOnly();
}
