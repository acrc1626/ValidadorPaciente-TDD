# Wiki — ValidadorPaciente

> Universidad de La Sabana · Maestría en Arquitectura de Software  
> Curso: Testing y Validación de Software · 2026  
> Autora: Astrid Carolina Rodríguez Cristancho

---

## Tabla de contenido

1. [Arquitectura del proyecto](#1-arquitectura-del-proyecto)
2. [Reglas de negocio del dominio](#2-reglas-de-negocio-del-dominio)
3. [Tipos de prueba implementadas](#3-tipos-de-prueba-implementadas)
4. [Matriz de pruebas](#4-matriz-de-pruebas)
5. [Pipeline CI/CD](#5-pipeline-cicd)
6. [Cobertura de código](#6-cobertura-de-código)
7. [Gestión de defectos](#7-gestión-de-defectos)
8. [Reflexión final](#8-reflexión-final)

---

## 1. Arquitectura del proyecto

El proyecto sigue **Clean Architecture** con separación estricta en tres capas:

```
ValidadorPacientes/
├── src/
│   ├── Domain/                  # Núcleo — sin dependencias externas
│   │   ├── Models/
│   │   │   ├── Paciente.cs      # Entidad (record inmutable)
│   │   │   └── ResultadoRegistro.cs  # Enum de resultados
│   │   ├── Repositories/
│   │   │   ├── IPacienteRepository.cs       # Contrato async
│   │   │   └── InMemoryPacienteRepository.cs
│   │   └── Services/
│   │       ├── IRegistroPaciente.cs         # Contrato del servicio
│   │       └── RegistroPaciente.cs          # Lógica de negocio
│   ├── Infrastructure/          # Implementación EF Core
│   │   ├── Data/PacienteDbContext.cs
│   │   └── Repositories/PacienteRepository.cs
│   └── Api/                     # Capa HTTP (ASP.NET Core)
│       ├── Controllers/PacienteController.cs
│       ├── Models/PacienteRequest.cs
│       └── Program.cs
└── tests/
    ├── Domain.Tests/            # MSTest — 23 pruebas unitarias
    └── Integration.Tests/       # xUnit — 13 pruebas (EF, Moq, HTTP)
```

### Decisiones de diseño clave

| Decisión | Justificación |
|----------|---------------|
| `Paciente` como `record` con `init` | Inmutabilidad: los invariantes validados en el servicio no pueden romperse tras el registro |
| `IPacienteRepository` async | El controlador ASP.NET Core es async; operaciones síncronas bloquean el thread pool |
| `IRegistroPaciente` interface | El controlador depende de la abstracción, no de la clase concreta (DIP) |
| Constructor único en `RegistroPaciente` | Elimina la dependencia implícita — la inyección es explícita y rastreable |

---

## 2. Reglas de negocio del dominio

`RegistroPaciente.Registrar()` aplica las validaciones **en este orden exacto**:

| # | Regla | Error retornado |
|---|-------|-----------------|
| 1 | Documento no nulo, no vacío, no numérico negativo | `DocumentoInvalido` |
| 2 | Edad entre 0 y 120 años inclusive | `EdadInvalida` |
| 3 | El paciente debe estar vivo (`Vivo == true`) | `PacienteFallecido` |
| 4 | El documento no debe existir ya en el repositorio | `DocumentoDuplicado` |
| ✓ | Todas las reglas pasan | `Exitoso` |

> El orden importa: si un paciente tiene documento inválido **y** edad inválida, se reporta `DocumentoInvalido` (regla 1). Los tests del Grupo 6 verifican esta prioridad.

### Casos límite validados

| Campo | Límite inferior | Límite superior |
|-------|-----------------|-----------------|
| Edad | 0 (recién nacido) | 120 |
| Documento numérico | `"0"` → válido | Negativo → inválido |
| Documento alfanumérico | `"PA123456"` → válido (pasaporte) | — |
| Etnia | Campo opcional — `null` o `""` → válido | — |

---

## 3. Tipos de prueba implementadas

| Tipo | Framework | Proyecto | Cantidad |
|------|-----------|----------|----------|
| Unitarias — dominio puro | MSTest + Coverlet | `Domain.Tests` | 23 |
| Integración — EF Core InMemory | xUnit + FluentAssertions | `Integration.Tests` | 5 |
| Unitarias — Mock (Moq) | xUnit + Moq + FluentAssertions | `Integration.Tests` | 5 |
| HTTP end-to-end | xUnit + WebApplicationFactory | `Integration.Tests` | 3 |
| **Total** | | | **36** |

### Grupos de las 23 pruebas unitarias (Domain.Tests)

| Grupo | Descripción | Pruebas |
|-------|-------------|---------|
| 1 | Registro exitoso | 4 |
| 2 | Validación de edad | 4 |
| 3 | Validación de documento | 5 |
| 4 | Estado de vida (Vivo) | 2 |
| 5 | Duplicados | 3 |
| 6 | Orden de validaciones | 2 |
| 7 | Casos límite | 3 |

---

## 4. Matriz de pruebas

| # | Caso de prueba | EF InMemory | Mock | HTTP |
|---|----------------|:-----------:|:----:|:----:|
| 1 | Paciente válido → retorna `Exitoso` | ✓ | ✓ | ✓ |
| 2 | Paciente válido → persiste en BD | ✓ | | |
| 3 | Documento duplicado → rechaza y no persiste | ✓ | ✓ | ✓ |
| 4 | Edad inválida → no persiste | ✓ | | ✓ |
| 5 | Dos pacientes distintos → persiste los dos | ✓ | | |
| 6 | Paciente válido → invoca `Agregar` exactamente una vez | | ✓ | |
| 7 | Documento inválido → no interactúa con repositorio | | ✓ | |
| 8 | Paciente fallecido → no interactúa con repositorio | | ✓ | |
| 9 | Repositorio en memoria → registra y consulta | | ✓ | |
| 10 | Paciente válido → HTTP 201 Created | | | ✓ |
| 11 | Edad inválida → HTTP 400 BadRequest | | | ✓ |
| 12 | Documento duplicado → HTTP 409 Conflict | | | ✓ |

### Semántica HTTP del controlador

| Resultado de negocio | HTTP Status | Body |
|----------------------|-------------|------|
| `Exitoso` | `201 Created` | `"Exitoso"` |
| `DocumentoDuplicado` | `409 Conflict` | `{ "error": "DocumentoDuplicado" }` |
| Cualquier otro error | `400 Bad Request` | `{ "error": "<nombre>" }` |

---

## 5. Pipeline CI/CD

El archivo `.github/workflows/ci.yml` se activa en cada `push` y `pull request` hacia `main`.

### Pasos del pipeline

| Paso | Herramienta | Falla si… |
|------|-------------|-----------|
| **Checkout** | `actions/checkout` | El repositorio no es accesible |
| **Setup .NET 9** | `actions/setup-dotnet` | El SDK no está disponible |
| **Restore** | `dotnet restore` | Hay dependencias NuGet rotas |
| **Build** | `dotnet build` | Hay errores de compilación |
| **Test unitarios** | `dotnet test` (Domain.Tests) | Un test falla o cobertura < 80% |
| **Test integración** | `dotnet test` (Integration.Tests) | Un test falla o cobertura < 80% |

### Política de merge

> El pipeline **bloquea el merge a `main`** si cualquier paso falla.  
> Un test roto, un error de compilación o cobertura por debajo del umbral son suficientes para impedir la integración.  
> Esto garantiza que `main` siempre esté en estado verde y desplegable.

### Umbrales de cobertura configurados

```xml
<!-- En *.csproj de cada proyecto de tests -->
<Threshold>80</Threshold>
<ThresholdType>line,branch,method</ThresholdType>
```

---

## 6. Cobertura de código

Resultados tras el refactor a Clean Code + SOLID (async, DIP, OCP):

| Módulo | Line | Branch | Method |
|--------|------|--------|--------|
| Api | 100% | 100% | 100% |
| Domain | 96.77% | 87.5% | 100% |
| Infrastructure | 91.66% | 100% | 85.71% |
| **Total** | **97.26%** | **87.5%** | **96.87%** |

### Cómo generar el reporte HTML localmente

```bash
dotnet test tests/Domain.Tests/ --collect:"XPlat Code Coverage"
reportgenerator \
  -reports:"tests/Domain.Tests/TestResults/**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:Html

# Abrir coverage-report/index.html
```

---

## 7. Gestión de defectos

| ID | Descripción | Causa raíz | Impacto | Estado |
|----|-------------|------------|---------|--------|
| DEF-001 | `ThresholdType` incorrecto en CI | Valor `lines` en lugar de `line` en el YAML | Pipeline fallaba en validación de cobertura | Cerrado |
| DEF-002 | Constructor sin parámetros en `RegistroPaciente` | Acoplamiento directo a `InMemoryPacienteRepository` | Violación de DIP; tests acoplados a implementación concreta | Cerrado |
| DEF-003 | HTTP retornaba `200 OK` para todos los resultados | Faltaba mapear errores de negocio a códigos HTTP semánticos | Clientes no podían distinguir éxito de error | Cerrado |

---

## 8. Reflexión final

### ¿Qué capas fueron más difíciles de probar y por qué?

La capa de **Infrastructure** fue la más compleja. Las pruebas de integración con EF Core InMemory requieren que cada test use una base de datos con nombre único (via `Guid.NewGuid()`) para garantizar aislamiento total. Sin ese patrón, los tests se contaminan entre sí cuando corren en paralelo, produciendo falsos negativos difíciles de rastrear.

### ¿Cuándo conviene usar Mocks y cuándo EF Core InMemory?

| Situación | Herramienta recomendada |
|-----------|------------------------|
| Verificar que el servicio llama al repositorio con los parámetros correctos | **Mock (Moq)** |
| Verificar que ciertos errores impiden llamar al repositorio | **Mock (Moq)** |
| Verificar que los datos realmente se persisten en la base de datos | **EF Core InMemory** |
| Verificar comportamiento ante duplicados a nivel de BD | **EF Core InMemory** |
| Probar el stack HTTP completo (serialización, routing, status codes) | **WebApplicationFactory** |

**Resumen:** los Mocks prueban *cómo* se usa el repositorio; EF InMemory prueba *qué* queda en la base de datos.

### ¿Qué aprendiste al integrar CI/CD con umbrales de cobertura?

El valor del CI/CD no está en ejecutar los tests — eso se puede hacer localmente. El valor está en que **el pipeline es el guardián del equipo**: ningún desarrollador puede integrar código a `main` que rompa tests o baje la cobertura, aunque lo intente. La calidad deja de depender de la disciplina individual y se convierte en una restricción técnica del proceso.
