using Api.Models;
using Domain.Models;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacienteController : ControllerBase
{
    private readonly RegistroPaciente _registro;

    public PacienteController(RegistroPaciente registro)
    {
        _registro = registro;
    }

    [HttpPost("registrar")]
    public IActionResult Registrar([FromBody] PacienteRequest request)
    {
        var paciente = new Paciente
        {
            Documento = request.Documento,
            Nombre    = request.Nombre,
            Edad      = request.Edad,
            Vivo      = request.Vivo,
            Etnia     = request.Etnia
        };

        var resultado = _registro.Registrar(paciente);
        return Ok(resultado.ToString());
    }
}
