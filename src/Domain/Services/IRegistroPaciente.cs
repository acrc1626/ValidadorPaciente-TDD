using Domain.Models;

namespace Domain.Services;

public interface IRegistroPaciente
{
    Task<ResultadoRegistro> Registrar(Paciente paciente);
    Task<IReadOnlyList<Paciente>> ObtenerPacientes();
}
