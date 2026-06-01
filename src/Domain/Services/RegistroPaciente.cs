using Domain.Models;

namespace Domain.Services;

/// <summary>
/// Servicio de dominio que implementa las reglas de negocio
/// para registrar pacientes en el sistema de salud epidemiológica.
///
/// Reglas de negocio aplicadas (en orden):
///   1. Documento válido  → no nulo, no vacío, no numérico negativo.
///   2. Edad válida       → entre 0 y 120 años inclusive.
///   3. Paciente vivo     → Vivo == true.
///   4. Sin duplicados    → documento único en el repositorio en memoria.
///
/// Nota: Etnia es opcional y no se valida.
/// </summary>
public class RegistroPaciente
{
    // ─── Repositorio en memoria ───────────────────────────────────────────────
    private readonly List<Paciente> _pacientes = [];

    // ─── Constantes de dominio ────────────────────────────────────────────────
    public const int EdadMinima = 0;
    public const int EdadMaxima = 120;

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Intenta registrar el <paramref name="paciente"/> según las reglas de negocio.
    /// </summary>
    /// <param name="paciente">Objeto paciente a registrar.</param>
    /// <returns>
    /// <see cref="ResultadoRegistro.Exitoso"/> si se registró correctamente;
    /// de lo contrario, el resultado que indica el primer error encontrado.
    /// </returns>
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
        if (ExisteDocumento(paciente.Documento))
            return ResultadoRegistro.DocumentoDuplicado;

        _pacientes.Add(paciente);
        return ResultadoRegistro.Exitoso;
    }

    /// <summary>Devuelve una vista de solo lectura de todos los pacientes registrados.</summary>
    public IReadOnlyList<Paciente> ObtenerPacientes() => _pacientes.AsReadOnly();

    /// <summary>
    /// Devuelve la cantidad de pacientes actualmente registrados.
    /// Útil para verificar que el registro no persistió ante errores.
    /// </summary>
    public int TotalRegistrados => _pacientes.Count;

    // ─── Métodos privados de validación ──────────────────────────────────────

    /// <summary>
    /// Valida que el documento no sea nulo/vacío y, si es numérico, no sea negativo.
    /// </summary>
    private static bool DocumentoEsValido(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return false;

        // Si el documento puede interpretarse como número, no debe ser negativo
        if (long.TryParse(documento, out long numeroDoc) && numeroDoc < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Valida que la edad se encuentre en el rango [EdadMinima, EdadMaxima].
    /// </summary>
    private static bool EdadEsValida(int edad) =>
        edad >= EdadMinima && edad <= EdadMaxima;

    /// <summary>
    /// Comprueba si ya existe un paciente registrado con el mismo documento.
    /// Comparación exacta (case-sensitive) para documentos alfanuméricos.
    /// </summary>
    private bool ExisteDocumento(string documento) =>
        _pacientes.Exists(p => p.Documento == documento);
}
