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
    public void Registrar_PacienteValido_InvocaAgregarUnaVez()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ExistePorDocumento(It.IsAny<string>()))
            .Returns(false);

        var paciente = CrearPacienteValido("12345678");

        // Act
        _registro.Registrar(paciente);

        // Assert
        _repositoryMock.Verify(r => r.Agregar(paciente), Times.Once);
    }

    // Given: el documento ya existe en el repositorio
    // When:  se intenta registrar el paciente
    // Then:  retorna DocumentoDuplicado y nunca llama a Agregar
    [Fact]
    public void Registrar_DocumentoExistente_NuncaInvocaAgregar()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ExistePorDocumento("12345678"))
            .Returns(true);

        var paciente = CrearPacienteValido("12345678");

        // Act
        var resultado = _registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.DocumentoDuplicado);
        _repositoryMock.Verify(r => r.Agregar(It.IsAny<Paciente>()), Times.Never);
    }

    // Given: paciente con Vivo = false (falla antes de verificar duplicados)
    // When:  se intenta registrar
    // Then:  retorna PacienteFallecido y nunca consulta ni agrega en el repositorio
    [Fact]
    public void Registrar_PacienteFallecido_NoInteractuaConRepositorio()
    {
        // Arrange
        var paciente = CrearPacienteValido("12345678");
        paciente.Vivo = false;

        // Act
        var resultado = _registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.PacienteFallecido);
        _repositoryMock.Verify(r => r.ExistePorDocumento(It.IsAny<string>()), Times.Never);
        _repositoryMock.Verify(r => r.Agregar(It.IsAny<Paciente>()),          Times.Never);
    }

    // Given: paciente con documento inválido (vacío)
    // When:  se intenta registrar
    // Then:  retorna DocumentoInvalido y el repositorio no es consultado
    [Fact]
    public void Registrar_DocumentoInvalido_NoInteractuaConRepositorio()
    {
        // Arrange
        var paciente = CrearPacienteValido("12345678");
        paciente.Documento = string.Empty;

        // Act
        var resultado = _registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.DocumentoInvalido);
        _repositoryMock.Verify(r => r.ExistePorDocumento(It.IsAny<string>()), Times.Never);
        _repositoryMock.Verify(r => r.Agregar(It.IsAny<Paciente>()),          Times.Never);
    }

    // Given: constructor sin parámetros (usa InMemoryPacienteRepository internamente)
    // When:  se registra un paciente válido y se consultan los pacientes
    // Then:  TotalRegistrados y ObtenerPacientes reflejan el registro correcto
    [Fact]
    public void ConstructorSinParametros_UsaRepositorioEnMemoria_RegistraYConsulta()
    {
        // Arrange
        var registro = new RegistroPaciente(); // crea InMemoryPacienteRepository internamente
        var paciente = CrearPacienteValido("88888888");

        // Act
        var resultado = registro.Registrar(paciente);

        // Assert
        resultado.Should().Be(ResultadoRegistro.Exitoso);
        registro.TotalRegistrados.Should().Be(1);
        registro.ObtenerPacientes().Should().ContainSingle(p => p.Documento == "88888888");
    }

    private static Paciente CrearPacienteValido(string documento) => new()
    {
        Documento = documento,
        Nombre    = "Ana Torres",
        Edad      = 30,
        Vivo      = true
    };
}
