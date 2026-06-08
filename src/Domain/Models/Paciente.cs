namespace Domain.Models;

/// <summary>
/// Representa un paciente dentro del sistema de salud epidemiológica.
/// </summary>
public record Paciente
{
    /// <summary>Número de documento de identidad (cédula / pasaporte).</summary>
    public string Documento { get; init; } = string.Empty;

    /// <summary>Nombre completo del paciente.</summary>
    public string Nombre { get; init; } = string.Empty;

    /// <summary>Edad en años (0 – 120).</summary>
    public int Edad { get; init; }

    /// <summary>
    /// Indica si el paciente está vivo.
    /// Solo se registran pacientes con Vivo = true.
    /// </summary>
    public bool Vivo { get; init; }

    /// <summary>
    /// Etnia del paciente (campo opcional).
    /// Puede ser null o vacío sin afectar el registro.
    /// </summary>
    public string? Etnia { get; init; }
}
