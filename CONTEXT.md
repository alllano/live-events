# Contexto Técnico — Prueba Técnica Fullstack .NET + Angular

> Documento de referencia para uso con Claude Code. Resume el enunciado completo de la prueba técnica de EventosVivos. No es necesario releer el PDF original.

---

## 1. Objetivo de la prueba

Construir el **núcleo de un sistema de reservas de eventos** para "EventosVivos", una startup que organiza eventos culturales, conferencias y talleres. Actualmente gestionan todo con hojas de cálculo y formularios en papel, lo que genera tres problemas concretos a resolver con el sistema:

- Sobreventa de entradas (exceden capacidad del venue por falta de control en tiempo real).
- Conflictos de horario cuando un venue tiene múltiples eventos.
- Pérdida de horas en validación manual de reservas y pagos.

**Plazo de entrega: 3 días.**

---

## 2. Requisitos técnicos

- **Backend**: .NET Core (última versión).
- **Frontend**: Angular (última versión), debe ser una app funcional que consuma la API.
- **Base de datos**: libre elección (en memoria, SQL Server, PostgreSQL, etc.).
- **Arquitectura**: libre elección — se evalúa la calidad de la decisión arquitectónica, no una arquitectura "correcta" predefinida.
- **API**: debe exponer endpoints RESTful bien diseñados.
- **Testing**: pruebas automatizadas obligatorias (mínimo unitarias).
- Uso de IA (Copilot, ChatGPT, Claude, etc.) permitido como parte del flujo de trabajo.

---

## 3. Criterios de aceptación — Funcionalidad requerida

### RF-01 — Crear Evento

Campos y validaciones:

- Título: obligatorio, 5-100 caracteres
- Descripción: obligatorio, 10-500 caracteres
- Venue: obligatorio, referencia a un lugar preexistente
- Capacidad máxima: obligatorio, entero positivo, debe ser ≤ capacidad del venue
- Fecha/hora de inicio: obligatorio, debe ser futura
- Fecha/hora de fin: obligatorio, posterior al inicio
- Precio de entrada: obligatorio, decimal positivo
- Tipo de evento: obligatorio (`conferencia`, `taller`, `concierto`)
- Estado: automático (activo al crear, se actualiza según RN-06)

### RF-02 — Listar Eventos con Filtros

Filtros opcionales combinables:

- Por tipo de evento
- Por fecha (rango de inicio)
- Por venue
- Por estado (activo, cancelado, completado)
- Búsqueda por título (parcial, case-insensitive)

### RF-03 — Reservar Entrada

Datos requeridos: `eventoId`, cantidad, nombre del comprador, email del comprador.

Validaciones:
- Existencia de entradas disponibles (no exceder capacidad)
- Formato de email válido
- Cantidad ≥ 1

Resultado: crea una reserva en estado `pendiente_pago`.

**Regla especial de prioridad**: si el evento tiene menos de 24 horas para iniciar, solo se permite reservar **máximo 5 entradas por transacción**. Esta restricción **tiene prioridad sobre RN-05** (la del límite por precio).

### RF-04 — Confirmar Pago de Reserva

- Cambia estado de `pendiente_pago` → `confirmada`
- Genera código de reserva único, formato: `EV-{6 dígitos}`
- Si la reserva ya está `confirmada` → rechazar con error
- Si la reserva fue `cancelada` → rechazar con error

### RF-05 — Cancelar Reserva

- Solo se puede cancelar reservas en estado `confirmada`
- Si la reserva ya está `cancelada` o en `pendiente_pago` → rechazar con error apropiado
- Cambia estado de `confirmada` → `cancelada`
- Libera las entradas reservadas para que estén disponibles nuevamente, **salvo que aplique penalización por RN-07**, en cuyo caso se marcan como "perdidas" y no se liberan para venta
- Registrar la fecha/hora de cancelación

### RF-06 — Reporte de Ocupación (por evento)

Debe mostrar:
- Total de entradas vendidas (confirmadas)
- Total de entradas disponibles restantes (excluyendo entradas "perdidas" por RN-07)
- Porcentaje de ocupación
- Total de ingresos (precio × entradas confirmadas)
- Estado del evento (activo, cancelado, completado)

---

## 4. Reglas de negocio (RN)

| ID | Regla | Descripción |
|---|---|---|
| RN-01 | Capacidad del venue | Un evento no puede exceder la capacidad del venue asignado |
| RN-02 | Superposición de venues | Dos eventos activos no pueden compartir el mismo venue con horarios superpuestos |
| RN-03 | Restricción de horario nocturno | Eventos en fin de semana (sáb/dom) no pueden iniciar después de las 22:00 |
| RN-04 | Restricción de reserva tardía | No se permiten reservas para eventos que inicien en menos de 1 hora |
| RN-05 | Limitación de entradas por transacción | Eventos con precio > $100 limitan a máximo 10 entradas por transacción |
| RN-06 | Estado del evento | Un evento se marca `completado` automáticamente cuando la fecha actual supera su hora de fin |
| RN-07 | Cancelación con penalización | Si se cancela una reserva confirmada con menos de 48 horas del evento, se registra como "perdida" (no se libera para venta, solo cuenta para reporte) |

**⚠️ Nota crítica de prioridad de reglas**: RF-03 (restricción <24h → máx. 5 entradas) **prevalece sobre** RN-05 (restricción por precio >$100 → máx. 10 entradas) cuando ambas aplicarían simultáneamente. Este caso de interacción entre reglas está puesto a prueba deliberadamente en el enunciado.

### Estados de una Reserva

| Estado | Descripción |
|---|---|
| `pendiente_pago` | Reserva creada, esperando confirmación de pago |
| `confirmada` | Pago verificado, reserva activa (equivale a "pagada", mismo estado) |
| `cancelada` | Reserva cancelada |

---

## 5. Datos de referencia

### Venues (preexistentes — no requieren CRUD ni gestión)

| ID | Nombre | Capacidad | Ciudad |
|---|---|---|---|
| 1 | Auditorio Central | 200 | Bogotá |
| 2 | Sala Norte | 50 | Bogotá |
| 3 | Arena Sur | 500 | Medellín |

### Tipos de evento válidos

`conferencia`, `taller`, `concierto`

---

## 6. Contexto del dominio

Negocio de gestión de eventos en vivo (cultura, conferencias, talleres). El dolor real es operativo: control de inventario de entradas en tiempo real, gestión de disponibilidad de venues compartidos entre eventos, y automatización del ciclo de vida pago → confirmación → cancelación, incluyendo políticas de penalización por cancelaciones tardías (lógica similar a no-show/overbooking en industrias de eventos/aerolíneas).

---

## 7. Entregables esperados

1. **Repositorio GitHub público** con el código fuente completo.
2. **README.md** del proyecto con:
   - Instrucciones claras para ejecutar el proyecto localmente.
   - Descripción de la arquitectura elegida y su justificación.
   - Tecnologías utilizadas.
3. **Tests automatizados** que validen los flujos de negocio (no solo CRUD básico).
4. **URL de aplicación desplegada en la nube** (opcional, valorado positivamente como diferenciador).

---

## 8. Criterios de evaluación

- Cumplimiento de los requerimientos funcionales y reglas de negocio
- Arquitectura y diseño de la solución
- Calidad del código y principios de diseño aplicados
- Manejo de errores y excepciones
- Seguridad de la aplicación
- Cobertura y calidad de las pruebas
- Documentación

---

## 9. Restricciones y consideraciones importantes

- **No hay arquitectura "correcta" única** — se evalúa la calidad de la decisión y su justificación, no la elección en sí. Conviene documentar explícitamente el *por qué* de la arquitectura elegida (ej. Clean Architecture, capas, CQRS, etc.).
- **No es necesario implementar gestión de venues** (CRUD) — son datos de referencia/seed preexistentes.
- **Los casos borde y validaciones tienen el mismo peso que los flujos principales** en la evaluación — no basta con el happy path.
- **Manejo de errores y excepciones** es un criterio explícito (códigos de error apropiados: 404 vs 400 vs 409 según corresponda, sin filtrar detalles internos sensibles).
- **Seguridad de la aplicación** es un criterio explícito (validación de inputs, manejo seguro de errores, etc., aunque no se exige autenticación específicamente).
- **Cobertura y calidad de las pruebas** se evalúa, no solo su existencia — priorizar tests sobre reglas de negocio complejas (RN-02 superposición de horarios, interacción RF-03/RN-05, RN-07 penalización) sobre tests triviales de CRUD.
- Prestar atención especial a la **interacción entre reglas** (ej. RF-03 vs RN-05), ya que es un caso de diseño deliberadamente puesto a prueba en el enunciado.
