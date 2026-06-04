using Domain.Models;
using Domain.Repositories;

namespace Domain.Services;

/// <summary>
/// Servicio de dominio que implementa las reglas de negocio
/// para registrar pacientes en el sistema de salud epidemiológica.
///
/// Reglas de negocio aplicadas (en orden):
///   1. Documento válido  → no nulo, no vacío, no numérico negativo.
///   2. Edad válida       → entre 0 y 120 años inclusive.
///   3. Paciente vivo     → Vivo == true.
///   4. Sin duplicados    → documento único en el repositorio.
///
/// Nota: Etnia es opcional y no se valida.
/// </summary>
public class RegistroPaciente
{
    private readonly IPacienteRepository _repository;

    // ─── Constantes de dominio ────────────────────────────────────────────────
    public const int EdadMinima = 0;
    public const int EdadMaxima = 120;

    // Constructor sin parámetros: mantiene compatibilidad con tests existentes.
    public RegistroPaciente() : this(new InMemoryPacienteRepository()) { }

    public RegistroPaciente(IPacienteRepository repository)
    {
        _repository = repository;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Intenta registrar el <paramref name="paciente"/> según las reglas de negocio.
    /// </summary>
    public ResultadoRegistro Registrar(Paciente paciente)
    {
        // Regla 1 – Documento válido
        if (!DocumentoEsValido(paciente.Documento))
            return ResultadoRegistro.DocumentoInvalido;

        // Regla 2 – Edad válida
        if (!EdadEsValida(paciente.Edad))
            return ResultadoRegistro.EdadInvalida;

        // Regla 3 – Paciente vivo
        if (!paciente.Vivo)
            return ResultadoRegistro.PacienteFallecido;

        // Regla 4 – Sin duplicados
        if (_repository.ExistePorDocumento(paciente.Documento))
            return ResultadoRegistro.DocumentoDuplicado;

        _repository.Agregar(paciente);
        return ResultadoRegistro.Exitoso;
    }

    /// <summary>Devuelve una vista de solo lectura de todos los pacientes registrados.</summary>
    public IReadOnlyList<Paciente> ObtenerPacientes() => _repository.ObtenerTodos();

    /// <summary>
    /// Devuelve la cantidad de pacientes actualmente registrados.
    /// Útil para verificar que el registro no persistió ante errores.
    /// </summary>
    public int TotalRegistrados => _repository.ObtenerTodos().Count;

    // ─── Métodos privados de validación ──────────────────────────────────────

    private static bool DocumentoEsValido(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return false;

        // Si el documento puede interpretarse como número, no debe ser negativo
        if (long.TryParse(documento, out long numeroDoc) && numeroDoc < 0)
            return false;

        return true;
    }

    private static bool EdadEsValida(int edad) =>
        edad >= EdadMinima && edad <= EdadMaxima;
}
