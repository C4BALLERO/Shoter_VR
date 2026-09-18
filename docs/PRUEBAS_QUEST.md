# Lista de pruebas físicas en Oculus Quest

Marcar cada prueba con el resultado real al ejecutarla en el visor. No marcar "OK" sin haberla corrido.

| ID | Prueba | Pasos | Resultado esperado | Resultado real |
|---|---|---|---|---|
| TEST-001 | Movimiento | Mover el joystick izquierdo | El jugador se desplaza sin saltos ni mareo | |
| TEST-002 | Agarrar medalla | Acercar la mano a una medalla y usar grip | La medalla se pega a la mano | |
| TEST-003 | Obtener medalla | Soltar el agarre / mantenerla agarrada un momento | Se registra en el contador, aparece "MEDALLA OBTENIDA" | |
| TEST-004 | Disparo | Agarrar un arma y apretar el gatillo | Se dispara, se ve/oye el disparo, baja la munición | |
| TEST-005 | Objetivo destruido | Disparar a un objetivo de la galería | El objetivo reacciona y suma puntos | |
| TEST-006 | Máquina | Interactuar con la máquina sin suficientes medallas / con suficientes | Mensaje de "faltan medallas" o "máquina activada" según corresponda | |
| TEST-007 | Recompensa | Activar la máquina con medallas suficientes | Aparece el objeto de recompensa | |
| TEST-008 | Guardado | Interactuar con la estación de guardado | Se crea el archivo de guardado (revisar log o carpeta persistente) | |
| TEST-009 | Carga | Cerrar y reabrir la app, interactuar con la estación de carga | Se recupera el progreso (medallas, puntos, munición) | |
| TEST-010 | UI | Observar el HUD durante el juego | Medallas/puntos/munición se actualizan en tiempo real, mensajes se leen bien | |
| TEST-011 | Prueba completa | Recorrido de principio a fin: explorar → disparar → recoger medallas → máquina → recompensa | La experiencia se completa sin errores ni caídas de FPS notorias | |

## Cosas a verificar además del resultado esperado

- Comodidad (sin mareo tras 2-3 minutos de juego).
- Escala correcta de los objetos respecto a la altura del jugador.
- Que los controles de Quest respondan igual que el XR Device Simulator.
- FPS estable (usar el profiler o el contador de estadísticas de Quest).
- Que no haya errores visibles en el visor (texturas rosadas, objetos flotando, etc.).
