using Domain.Models;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly PacienteDbContext _context;

    public PacienteRepository(PacienteDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistePorDocumento(string documento) =>
        _context.Pacientes.AnyAsync(p => p.Documento == documento);

    public async Task Agregar(Paciente paciente)
    {
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Paciente>> ObtenerTodos()
    {
        var list = await _context.Pacientes.ToListAsync();
        return list.AsReadOnly();
    }
}
