using Api.Models;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacienteController : ControllerBase
{
    private readonly IRegistroPaciente _registro;

    public PacienteController(IRegistroPaciente registro)
    {
        _registro = registro;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] PacienteRequest request)
    {
        var paciente = new Paciente
        {
            Documento = request.Documento,
            Nombre    = request.Nombre,
            Edad      = request.Edad,
            Vivo      = request.Vivo,
            Etnia     = request.Etnia
        };

        var resultado = await _registro.Registrar(paciente);

        return resultado switch
        {
            ResultadoRegistro.Exitoso            => Created($"/api/paciente/{request.Documento}", resultado.ToString()),
            ResultadoRegistro.DocumentoDuplicado => Conflict(new { error = resultado.ToString() }),
            _                                    => BadRequest(new { error = resultado.ToString() })
        };
    }
}
