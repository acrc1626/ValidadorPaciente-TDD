/*
 * RegistroPacienteIntegrationTests
 * Pruebas de integración con EF Core InMemory.
 * Cada test usa una base de datos nueva (Guid) para garantizar aislamiento.
 *
 * Patrón: AAA  +  Given-When-Then (en comentarios)
 * Framework: xUnit + FluentAssertions
 */

using Domain.Models;
using Domain.Services;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration.Tests;

public class RegistroPacienteIntegrationTests : IDisposable
{
    private readonly PacienteDbContext _context;
    private readonly PacienteRepository _repository;
    private readonly RegistroPaciente _registro;

    public RegistroPacienteIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<PacienteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context    = new PacienteDbContext(options);
        _repository = new PacienteRepository(_context);
        _registro   = new RegistroPaciente(_repository);
    }

    // Given: paciente con todos los campos válidos
    // When:  se llama a Registrar()
    // Then:  retorna Exitoso
    [Fact]
    public void Registrar_PacienteValido_RetornaExitoso()
    {
        // Arrange
        var paciente = CrearPacienteValido("12345678");

        // Act
        var resultado = _registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.Exitoso);
    }

    // Given: paciente válido registrado
    // When:  se consulta la base de datos
    // Then:  el paciente está persistido con el documento correcto
    [Fact]
    public void Registrar_PacienteValido_PersistePacienteEnBD()
    {
        // Arrange
        var paciente = CrearPacienteValido("12345678");

        // Act
        _registro.Registrar(paciente);

        // Assert
        _context.Pacientes.Should().HaveCount(1);
        _context.Pacientes.First().Documento.Should().Be("12345678");
    }

    // Given: un paciente ya registrado con "12345678"
    // When:  se intenta registrar otro con el mismo documento
    // Then:  retorna DocumentoDuplicado y la BD sigue con 1 registro
    [Fact]
    public void Registrar_DocumentoDuplicado_RechazaYNoPersiste()
    {
        // Arrange
        var primero   = CrearPacienteValido("12345678");
        var duplicado = CrearPacienteValido("12345678");
        _registro.Registrar(primero);

        // Act
        var resultado = _registro.Registrar(duplicado);

        // Assert
        resultado.Should().Be(ResultadoRegistro.DocumentoDuplicado);
        _context.Pacientes.Should().HaveCount(1);
    }

    // Given: paciente con edad fuera de rango (150)
    // When:  se intenta registrar
    // Then:  retorna EdadInvalida y la BD queda vacía
    [Fact]
    public void Registrar_EdadInvalida_NoPersiste()
    {
        // Arrange
        var paciente = CrearPacienteValido("99999999");
        paciente.Edad = 150;

        // Act
        var resultado = _registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.EdadInvalida);
        _context.Pacientes.Should().BeEmpty();
    }

    // Given: dos pacientes con documentos distintos
    // When:  se registran ambos
    // Then:  la BD contiene exactamente 2 pacientes
    [Fact]
    public void Registrar_DosPacientesDistintos_PersisteLosDoS()
    {
        // Arrange
        var paciente1 = CrearPacienteValido("11111111");
        var paciente2 = CrearPacienteValido("22222222");

        // Act
        var r1 = _registro.Registrar(paciente1);
        var r2 = _registro.Registrar(paciente2);

        // Assert
        r1.Should().Be(ResultadoRegistro.Exitoso);
        r2.Should().Be(ResultadoRegistro.Exitoso);
        _context.Pacientes.Should().HaveCount(2);
    }

    public void Dispose() => _context.Dispose();

    private static Paciente CrearPacienteValido(string documento) => new()
    {
        Documento = documento,
        Nombre    = "Ana Torres",
        Edad      = 30,
        Vivo      = true,
        Etnia     = "Mestiza"
    };
}
