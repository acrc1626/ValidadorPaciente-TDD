# ValidadorPaciente — Taller de Pruebas con TDD

Universidad de La Sabana · Maestría en Arquitectura de Software  
Curso: Testing y Validación de Software · 2026  
Autora: Astrid Carolina Rodríguez Cristancho

## Descripción

Sistema de validación de registro de pacientes para salud epidemiológica en Colombia. Implementado en C# con .NET 9 bajo Clean Architecture — el dominio es completamente independiente de bases de datos, frameworks y servicios externos.

## Stack

- Lenguaje: C# con .NET 9
- Pruebas unitarias: MSTest + Coverlet + ReportGenerator
- Pruebas integración: xUnit + Moq + EF Core InMemory
- Pruebas HTTP: WebApplicationFactory
- CI/CD: GitHub Actions

## Resultados

- U3: 23 pruebas unitarias · 100% cobertura
- U4: 36 pruebas totales · 97% cobertura · CI/CD activo

## Cómo ejecutar las pruebas

```bash
# Pruebas unitarias (Domain)
dotnet test tests/Domain.Tests/

# Pruebas de integración
dotnet test tests/Integration.Tests/

# Todas las pruebas
dotnet test
```

## Cobertura

| Módulo | Line | Branch | Method |
|--------|------|--------|--------|
| Api | 100% | 100% | 100% |
| Domain | 96.77% | 87.5% | 100% |
| Infrastructure | 91.66% | 100% | 85.71% |
| **Total** | **97.26%** | **87.5%** | **96.87%** |

## Gestión de defectos

| ID | Descripción | Causa | Estado |
|----|-------------|-------|--------|
| DEF-003 | HTTP retornaba 200 con doc inválido | Faltaba mapear errores a HTTP 400/409 | Cerrado |

## Reflexión

- **Capa más difícil:** Infrastructure — requiere aislamiento con GUID único por prueba para evitar contaminación entre tests
- **Mocks vs EF InMemory:** Mocks para verificar lógica de negocio aislada; EF InMemory para verificar persistencia real contra la base de datos
- **CI/CD:** El valor está en bloquear automáticamente código que rompe tests — la calidad se protege sin intervención manual
