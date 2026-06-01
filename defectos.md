# Registro de Defectos – ValidadorPaciente

> Este archivo documenta defectos detectados durante el proceso TDD.
> Cada entrada sigue el ciclo **RED → GREEN → REFACTOR**.

---

## Plantilla de defecto

```
ID       : DEF-XXX
Categoría: [Dominio | Validación | Integración]
Estado   : [Abierto | Resuelto | Cerrado]
Detectado: YYYY-MM-DD
Resuelto : YYYY-MM-DD

### Descripción
Breve descripción del defecto encontrado.

### Prueba que lo detectó (RED)
`NombreDelTestMethod`

### Causa raíz
Explicación técnica del problema en el código de producción.

### Corrección aplicada (GREEN)
Cambio realizado en el código para hacer pasar la prueba.

### Refactoring posterior
Mejoras de diseño/estructura realizadas sin romper las pruebas.
```

---

## Defectos registrados

*No se han registrado defectos aún. Los defectos se documentarán aquí
durante las iteraciones TDD del taller.*

---

## Historial de iteraciones TDD

| Iteración | Prueba                                         | RED | GREEN | REFACTOR |
|-----------|------------------------------------------------|-----|-------|----------|
| 1         | Registrar_PacienteValido_RetornaExitoso        | ✅  | ✅    | ✅       |
| 2         | Registrar_EdadNegativa_RetornaEdadInvalida     | ✅  | ✅    | ✅       |
| 3         | Registrar_DocumentoNulo_RetornaDocumentoInv.   | ✅  | ✅    | ✅       |
| 4         | Registrar_PacienteFallecido_Retorna...         | ✅  | ✅    | ✅       |
| 5         | Registrar_DocumentoDuplicado_Retorna...        | ✅  | ✅    | ✅       |
