using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PacienteDbContext : DbContext
{
    public PacienteDbContext(DbContextOptions<PacienteDbContext> options) : base(options) { }

    public DbSet<Paciente> Pacientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Paciente>().HasKey(p => p.Documento);
    }
}
