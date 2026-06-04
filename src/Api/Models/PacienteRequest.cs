namespace Api.Models;

public record PacienteRequest(
    string Documento,
    string Nombre,
    int Edad,
    bool Vivo,
    string? Etnia = null
);
