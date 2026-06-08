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
public class RegistroPaciente : IRegistroPaciente
{
    private readonly IPacienteRepository _repository;

    public const int EdadMinima = 0;
    public const int EdadMaxima = 120;

    public RegistroPaciente(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoRegistro> Registrar(Paciente paciente)
    {
        if (!DocumentoEsValido(paciente.Documento))
            return ResultadoRegistro.DocumentoInvalido;

        if (!EdadEsValida(paciente.Edad))
            return ResultadoRegistro.EdadInvalida;

        if (!paciente.Vivo)
            return ResultadoRegistro.PacienteFallecido;

        if (await _repository.ExistePorDocumento(paciente.Documento))
            return ResultadoRegistro.DocumentoDuplicado;

        await _repository.Agregar(paciente);
        return ResultadoRegistro.Exitoso;
    }

    public Task<IReadOnlyList<Paciente>> ObtenerPacientes() => _repository.ObtenerTodos();

    private static bool DocumentoEsValido(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return false;

        if (long.TryParse(documento, out long numeroDoc) && numeroDoc < 0)
            return false;

        return true;
    }

    private static bool EdadEsValida(int edad) =>
        edad >= EdadMinima && edad <= EdadMaxima;
}
