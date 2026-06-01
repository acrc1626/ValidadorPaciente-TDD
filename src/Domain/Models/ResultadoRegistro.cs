namespace Domain.Models;

/// <summary>
/// Resultado devuelto por <see cref="Domain.Services.RegistroPaciente"/>
/// después de intentar registrar un paciente.
/// </summary>
public enum ResultadoRegistro
{
    /// <summary>El paciente fue registrado sin errores.</summary>
    Exitoso,

    /// <summary>La edad está fuera del rango permitido (0 – 120).</summary>
    EdadInvalida,

    /// <summary>El documento es nulo, vacío o tiene un valor negativo.</summary>
    DocumentoInvalido,

    /// <summary>El paciente no está vivo (Vivo = false).</summary>
    PacienteFallecido,

    /// <summary>Ya existe un paciente registrado con el mismo número de documento.</summary>
    DocumentoDuplicado
}
