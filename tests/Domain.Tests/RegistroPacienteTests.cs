/*
 * ============================================================
 *  TALLER PRACTICO – PRUEBAS UNITARIAS CON TDD
 *  Dominio: ValidadorPaciente – Sistema de Salud Epidemiológica
 * ============================================================
 *
 *  Metodología TDD – ciclo Red → Green → Refactor
 *  ─────────────────────────────────────────────────────────
 *  RED    : Escribir la prueba antes que el código de producción.
 *           La prueba DEBE fallar en la primera ejecución.
 *  GREEN  : Escribir el mínimo código en RegistroPaciente.cs
 *           para hacer pasar la prueba.
 *  REFACTOR: Mejorar estructura/legibilidad sin cambiar comportamiento.
 *            Las pruebas deben seguir pasando tras el refactor.
 *
 *  Patrón AAA
 *  ─────────────────────────────────────────────────────────
 *  Arrange : configurar datos y dependencias del test.
 *  Act     : ejecutar el método bajo prueba (SUT).
 *  Assert  : verificar que el resultado cumple la expectativa.
 *
 *  Patrón BDD (documentado en comentarios)
 *  ─────────────────────────────────────────────────────────
 *  Given   : estado inicial / precondiciones.
 *  When    : acción que se ejecuta.
 *  Then    : resultado esperado.
 * ============================================================
 */

using Domain.Models;
using Domain.Repositories;
using Domain.Services;

namespace Domain.Tests;

[TestClass]
public class RegistroPacienteTests
{
    // ─── SUT (System Under Test) ──────────────────────────────────────────────
    // Se instancia en cada test para garantizar aislamiento (sin estado compartido).
    private RegistroPaciente _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicio = new RegistroPaciente(new InMemoryPacienteRepository());
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GRUPO 1 – REGISTRO EXITOSO
    //  Regla: un paciente válido (documento, edad, vivo) debe registrarse.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Given  un paciente con todos los campos válidos
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser ResultadoRegistro.Exitoso
    /// </summary>
    [TestMethod]
    [TestCategory("RegistroExitoso")]
    public async Task Registrar_PacienteValido_RetornaExitoso()
    {
        // Arrange
        var paciente = CrearPacienteValido();

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado,
            "Un paciente con datos válidos debe registrarse exitosamente.");
    }

    /// <summary>
    /// Given  un paciente válido registrado
    /// When   se consulta ObtenerPacientes()
    /// Then   la lista contiene exactamente un paciente con ese documento
    /// </summary>
    [TestMethod]
    [TestCategory("RegistroExitoso")]
    public async Task Registrar_PacienteValido_AgregaAlRepositorio()
    {
        // Arrange
        var paciente = CrearPacienteValido();

        // Act
        await _servicio.Registrar(paciente);

        // Assert
        var pacientes = await _servicio.ObtenerPacientes();
        Assert.AreEqual(1, pacientes.Count,
            "Después de un registro exitoso debe haber exactamente 1 paciente.");
        Assert.AreEqual(paciente.Documento, pacientes[0].Documento);
    }

    /// <summary>
    /// Given  un paciente con etnia nula (campo opcional)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser Exitoso (etnia no es obligatoria)
    /// </summary>
    [TestMethod]
    [TestCategory("RegistroExitoso")]
    public async Task Registrar_PacienteSinEtnia_EtniaOpcionalNoAfectaRegistro()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Etnia = null };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado,
            "La etnia es un campo opcional; su ausencia no debe impedir el registro.");
    }

    /// <summary>
    /// Given  un paciente con etnia vacía (campo opcional)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser Exitoso
    /// </summary>
    [TestMethod]
    [TestCategory("RegistroExitoso")]
    public async Task Registrar_PacienteConEtniaVacia_EtniaOpcionalNoAfectaRegistro()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Etnia = string.Empty };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado,
            "La etnia vacía no debe impedir el registro del paciente.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GRUPO 2 – VALIDACIÓN DE EDAD
    //  Regla: edad debe estar en el rango [0, 120].
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Given  un paciente con edad -1 (negativa)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser EdadInvalida
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionEdad")]
    public async Task Registrar_EdadNegativa_RetornaEdadInvalida()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Edad = -1 };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.EdadInvalida, resultado,
            "Una edad negativa debe retornar EdadInvalida.");
    }

    /// <summary>
    /// Given  un paciente con edad 121 (mayor al máximo)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser EdadInvalida
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionEdad")]
    public async Task Registrar_EdadMayorA120_RetornaEdadInvalida()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Edad = 121 };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.EdadInvalida, resultado,
            "Una edad mayor a 120 debe retornar EdadInvalida.");
    }

    /// <summary>
    /// Given  un paciente con edad 0 (límite inferior válido)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser Exitoso (recién nacido)
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionEdad")]
    public async Task Registrar_EdadCero_LimiteInferiorValido_RetornaExitoso()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Edad = 0 };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado,
            "Edad 0 (recién nacido) es el límite inferior válido y debe registrarse.");
    }

    /// <summary>
    /// Given  un paciente con edad 120 (límite superior válido)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser Exitoso
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionEdad")]
    public async Task Registrar_Edad120_LimiteSuperiorValido_RetornaExitoso()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Edad = 120 };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado,
            "Edad 120 es el límite superior válido y debe registrarse.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GRUPO 3 – VALIDACIÓN DE DOCUMENTO
    //  Regla: no nulo, no vacío, no numérico negativo.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Given  un paciente con documento null
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser DocumentoInvalido
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionDocumento")]
    public async Task Registrar_DocumentoNulo_RetornaDocumentoInvalido()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Documento = null! };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.DocumentoInvalido, resultado,
            "Un documento nulo debe retornar DocumentoInvalido.");
    }

    /// <summary>
    /// Given  un paciente con documento vacío ("")
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser DocumentoInvalido
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionDocumento")]
    public async Task Registrar_DocumentoVacio_RetornaDocumentoInvalido()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Documento = string.Empty };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.DocumentoInvalido, resultado,
            "Un documento vacío debe retornar DocumentoInvalido.");
    }

    /// <summary>
    /// Given  un paciente con documento compuesto solo de espacios
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser DocumentoInvalido
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionDocumento")]
    public async Task Registrar_DocumentoSoloEspacios_RetornaDocumentoInvalido()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Documento = "   " };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.DocumentoInvalido, resultado,
            "Un documento con solo espacios debe retornar DocumentoInvalido.");
    }

    /// <summary>
    /// Given  un paciente con documento numérico negativo ("-12345")
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser DocumentoInvalido
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionDocumento")]
    public async Task Registrar_DocumentoNumericoNegativo_RetornaDocumentoInvalido()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Documento = "-12345" };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.DocumentoInvalido, resultado,
            "Un documento numérico negativo debe retornar DocumentoInvalido.");
    }

    /// <summary>
    /// Given  un paciente con documento alfanumérico (pasaporte)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser Exitoso (los pasaportes son válidos)
    /// </summary>
    [TestMethod]
    [TestCategory("ValidacionDocumento")]
    public async Task Registrar_DocumentoAlfanumerico_Pasaporte_RetornaExitoso()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Documento = "PA123456" };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado,
            "Un documento alfanumérico (pasaporte) debe ser considerado válido.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GRUPO 4 – ESTADO DE VIDA DEL PACIENTE
    //  Regla: Vivo debe ser true para registrar.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Given  un paciente con Vivo = false
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser PacienteFallecido
    /// </summary>
    [TestMethod]
    [TestCategory("EstadoVida")]
    public async Task Registrar_PacienteFallecido_RetornaPacienteFallecido()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Vivo = false };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.PacienteFallecido, resultado,
            "Un paciente con Vivo=false no debe registrarse en el sistema.");
    }

    /// <summary>
    /// Given  un paciente fallecido
    /// When   se intenta registrar
    /// Then   el total de registrados no debe aumentar
    /// </summary>
    [TestMethod]
    [TestCategory("EstadoVida")]
    public async Task Registrar_PacienteFallecido_NoAgregaAlRepositorio()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Vivo = false };

        // Act
        await _servicio.Registrar(paciente);

        // Assert
        var pacientes = await _servicio.ObtenerPacientes();
        Assert.AreEqual(0, pacientes.Count,
            "Un paciente fallecido no debe persistir en el repositorio.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GRUPO 5 – DUPLICADOS
    //  Regla: no puede haber dos pacientes con el mismo número de documento.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Given  un paciente ya registrado con documento "11111111"
    /// When   se intenta registrar otro paciente con el mismo documento
    /// Then   el resultado debe ser DocumentoDuplicado
    /// </summary>
    [TestMethod]
    [TestCategory("Duplicados")]
    public async Task Registrar_DocumentoDuplicado_RetornaDocumentoDuplicado()
    {
        // Arrange
        var paciente1 = CrearPacienteValido();
        var paciente2 = CrearPacienteValido(); // mismo documento que paciente1

        await _servicio.Registrar(paciente1); // primer registro exitoso

        // Act
        var resultado = await _servicio.Registrar(paciente2);

        // Assert
        Assert.AreEqual(ResultadoRegistro.DocumentoDuplicado, resultado,
            "Un segundo paciente con el mismo documento debe retornar DocumentoDuplicado.");
    }

    /// <summary>
    /// Given  un paciente ya registrado
    /// When   se intenta registrar un duplicado
    /// Then   el total de registrados sigue siendo 1 (no se duplicó)
    /// </summary>
    [TestMethod]
    [TestCategory("Duplicados")]
    public async Task Registrar_DocumentoDuplicado_NoAgregaSegundoPaciente()
    {
        // Arrange
        var paciente1 = CrearPacienteValido();
        var paciente2 = CrearPacienteValido();

        await _servicio.Registrar(paciente1);

        // Act
        await _servicio.Registrar(paciente2);

        // Assert
        var pacientes = await _servicio.ObtenerPacientes();
        Assert.AreEqual(1, pacientes.Count,
            "El repositorio no debe contener duplicados.");
    }

    /// <summary>
    /// Given  dos pacientes con documentos distintos
    /// When   se registran ambos
    /// Then   ambos se registran exitosamente (no son duplicados)
    /// </summary>
    [TestMethod]
    [TestCategory("Duplicados")]
    public async Task Registrar_DosPacientesConDocumentosDistintos_RegistraAmbos()
    {
        // Arrange
        var paciente1 = CrearPacienteValido("11111111");
        var paciente2 = CrearPacienteValido("22222222");

        // Act
        var resultado1 = await _servicio.Registrar(paciente1);
        var resultado2 = await _servicio.Registrar(paciente2);

        // Assert
        var pacientes = await _servicio.ObtenerPacientes();
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado1);
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado2);
        Assert.AreEqual(2, pacientes.Count,
            "Dos pacientes con diferentes documentos deben registrarse sin conflicto.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GRUPO 6 – ORDEN DE VALIDACIONES (prioridad de reglas)
    //  Verifica que las reglas se aplican en el orden correcto.
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Given  un paciente con documento inválido Y edad inválida
    /// When   se llama a Registrar()
    /// Then   se reporta DocumentoInvalido (primera regla que falla)
    /// </summary>
    [TestMethod]
    [TestCategory("OrdenValidaciones")]
    public async Task Registrar_DocumentoInvalidoYEdadInvalida_PriorizaDocumentoInvalido()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Documento = string.Empty, Edad = -5 };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.DocumentoInvalido, resultado,
            "La validación de documento debe ejecutarse antes que la de edad.");
    }

    /// <summary>
    /// Given  un paciente con edad inválida y Vivo = false
    /// When   se llama a Registrar()
    /// Then   se reporta EdadInvalida (segunda regla, antes que estado de vida)
    /// </summary>
    [TestMethod]
    [TestCategory("OrdenValidaciones")]
    public async Task Registrar_EdadInvalidaYPacienteFallecido_PriorizaEdadInvalida()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Edad = 200, Vivo = false };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.EdadInvalida, resultado,
            "La validación de edad debe ejecutarse antes que la de estado de vida.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GRUPO 7 – CASOS LÍMITE (Boundary Values)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Given  un paciente con documento "0" (cero como string)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser Exitoso (cero no es negativo)
    /// </summary>
    [TestMethod]
    [TestCategory("CasosLimite")]
    public async Task Registrar_DocumentoCero_EsValido()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Documento = "0" };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado,
            "El documento '0' es un valor numérico no negativo y debe ser válido.");
    }

    /// <summary>
    /// Given  un paciente con edad 1 (mínimo no-neonato)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser Exitoso
    /// </summary>
    [TestMethod]
    [TestCategory("CasosLimite")]
    public async Task Registrar_EdadUno_EsValida()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Edad = 1 };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado);
    }

    /// <summary>
    /// Given  un paciente con edad 119 (límite superior -1)
    /// When   se llama a Registrar()
    /// Then   el resultado debe ser Exitoso
    /// </summary>
    [TestMethod]
    [TestCategory("CasosLimite")]
    public async Task Registrar_Edad119_EsValida()
    {
        // Arrange
        var paciente = CrearPacienteValido() with { Edad = 119 };

        // Act
        var resultado = await _servicio.Registrar(paciente);

        // Assert
        Assert.AreEqual(ResultadoRegistro.Exitoso, resultado);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  HELPERS PRIVADOS
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea un paciente con datos válidos por defecto.
    /// Todos los tests pueden modificar solo la propiedad que quieren probar.
    /// </summary>
    private static Paciente CrearPacienteValido(string documento = "11111111") =>
        new()
        {
            Documento = documento,
            Nombre    = "Ana Torres",
            Edad      = 30,
            Vivo      = true,
            Etnia     = "Mestiza"
        };
}
