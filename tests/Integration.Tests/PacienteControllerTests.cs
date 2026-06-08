/*
 * PacienteControllerTests
 * Pruebas HTTP end-to-end con WebApplicationFactory.
 * Se usa IClassFixture para reutilizar el servidor de prueba entre tests.
 *
 * Patrón: AAA  +  Given-When-Then (en comentarios)
 * Framework: xUnit + FluentAssertions + Microsoft.AspNetCore.Mvc.Testing
 */

using System.Net;
using System.Net.Http.Json;
using Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Integration.Tests;

public class PacienteControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PacienteControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Given: request con todos los datos válidos
    // When:  POST /api/paciente/registrar
    // Then:  HTTP 201 Created y body contiene "Exitoso"
    [Fact]
    public async Task PostRegistrar_PacienteValido_Retorna201Creado()
    {
        // Arrange
        var request = new PacienteRequest("30000001", "Ana García", 30, true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/paciente/registrar", request);
        var body     = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body.Should().Contain("Exitoso");
    }

    // Given: request con edad fuera de rango (200)
    // When:  POST /api/paciente/registrar
    // Then:  HTTP 400 BadRequest y body contiene "EdadInvalida"
    [Fact]
    public async Task PostRegistrar_EdadInvalida_Retorna400BadRequest()
    {
        // Arrange
        var request = new PacienteRequest("30000002", "Juan Pérez", 200, true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/paciente/registrar", request);
        var body     = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.Should().Contain("EdadInvalida");
    }

    // Given: primer registro exitoso del documento "30000003"
    // When:  se envía el mismo documento por segunda vez
    // Then:  HTTP 409 Conflict y body contiene "DocumentoDuplicado"
    [Fact]
    public async Task PostRegistrar_DocumentoDuplicado_Retorna409Conflict()
    {
        // Arrange
        var request = new PacienteRequest("30000003", "María López", 25, true);
        await _client.PostAsJsonAsync("/api/paciente/registrar", request);

        // Act
        var response = await _client.PostAsJsonAsync("/api/paciente/registrar", request);
        var body     = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        body.Should().Contain("DocumentoDuplicado");
    }
}
