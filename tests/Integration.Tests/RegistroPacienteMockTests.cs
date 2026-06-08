/*
 * RegistroPacienteMockTests
 * Pruebas unitarias con Moq para verificar las interacciones
 * del servicio RegistroPaciente con el repositorio.
 *
 * Patrón: AAA  +  Given-When-Then (en comentarios)
 * Framework: xUnit + Moq + FluentAssertions
 */

using Domain.Models;
using Domain.Repositories;
using Domain.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Integration.Tests;

public class RegistroPacienteMockTests
{
    private readonly Mock<IPacienteRepository> _repositoryMock;
    private readonly RegistroPaciente _registro;

    public RegistroPacienteMockTests()
    {
        _repositoryMock = new Mock<IPacienteRepository>();
        _registro       = new RegistroPaciente(_repositoryMock.Object);
    }

    // Given: repositorio sin duplicados para el documento
    // When:  se registra un paciente válido
    // Then:  se invoca Agregar exactamente una vez con el paciente correcto
    [Fact]
    public async Task Registrar_PacienteValido_InvocaAgregarUnaVez()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ExistePorDocumento(It.IsAny<string>()))
            .ReturnsAsync(false);

        var paciente = CrearPacienteValido("12345678");

        // Act
        await _registro.Registrar(paciente);

        // Assert
        _repositoryMock.Verify(r => r.Agregar(paciente), Times.Once);
    }

    // Given: el documento ya existe en el repositorio
    // When:  se intenta registrar el paciente
    // Then:  retorna DocumentoDuplicado y nunca llama a Agregar
    [Fact]
    public async Task Registrar_DocumentoExistente_NuncaInvocaAgregar()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ExistePorDocumento("12345678"))
            .ReturnsAsync(true);

        var paciente = CrearPacienteValido("12345678");

        // Act
        var resultado = await _registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.DocumentoDuplicado);
        _repositoryMock.Verify(r => r.Agregar(It.IsAny<Paciente>()), Times.Never);
    }

    // Given: paciente con Vivo = false (falla antes de verificar duplicados)
    // When:  se intenta registrar
    // Then:  retorna PacienteFallecido y nunca consulta ni agrega en el repositorio
    [Fact]
    public async Task Registrar_PacienteFallecido_NoInteractuaConRepositorio()
    {
        // Arrange
        var paciente = CrearPacienteValido("12345678") with { Vivo = false };

        // Act
        var resultado = await _registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.PacienteFallecido);
        _repositoryMock.Verify(r => r.ExistePorDocumento(It.IsAny<string>()), Times.Never);
        _repositoryMock.Verify(r => r.Agregar(It.IsAny<Paciente>()),          Times.Never);
    }

    // Given: paciente con documento inválido (vacío)
    // When:  se intenta registrar
    // Then:  retorna DocumentoInvalido y el repositorio no es consultado
    [Fact]
    public async Task Registrar_DocumentoInvalido_NoInteractuaConRepositorio()
    {
        // Arrange
        var paciente = CrearPacienteValido("12345678") with { Documento = string.Empty };

        // Act
        var resultado = await _registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.DocumentoInvalido);
        _repositoryMock.Verify(r => r.ExistePorDocumento(It.IsAny<string>()), Times.Never);
        _repositoryMock.Verify(r => r.Agregar(It.IsAny<Paciente>()),          Times.Never);
    }

    // Given: servicio con repositorio en memoria inyectado explícitamente
    // When:  se registra un paciente válido y se consultan los pacientes
    // Then:  ObtenerPacientes refleja el registro correcto
    [Fact]
    public async Task RegistroPaciente_ConRepositorioEnMemoria_RegistraYConsulta()
    {
        // Arrange
        var registro = new RegistroPaciente(new InMemoryPacienteRepository());
        var paciente = CrearPacienteValido("88888888");

        // Act
        var resultado = await registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.Exitoso);
        var pacientes = await registro.ObtenerPacientes();
        pacientes.Should().ContainSingle(p => p.Documento == "88888888");
    }

    private static Paciente CrearPacienteValido(string documento) => new()
    {
        Documento = documento,
        Nombre    = "Ana Torres",
        Edad      = 30,
        Vivo      = true
    };
}
