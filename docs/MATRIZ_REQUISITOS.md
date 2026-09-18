# Matriz de requisitos — MEDALLAS: Zona Perdida

Estado al 2026-09-17 (día 1 de desarrollo). "Implementado" = existe en el código/proyecto. "Probado" = verificado corriendo (editor y/o Quest). No marcar como completo lo que no esté realmente verificado.

| Requisito | Implementado | Probado | Evidencia |
|---|---|---|---|
| Unity | Sí | Sí | Proyecto abre y compila en 6000.3.23f1 |
| Oculus Quest (build target) | Sí | No | Build target Android + IL2CPP + ARM64 configurado; no compilado ni probado en dispositivo físico aún |
| XR Interaction Toolkit | Sí | Sí | Paquete 3.3.2 instalado, XR Origin y interactuables funcionando en Play Mode |
| XR Device Simulator | Sí | Sí | Sample importado, probado en Play Mode (smoke test sin errores) |
| Movimiento VR | Sí | Sí (editor) | Locomoción de Starter Assets (XR Origin) probada con el simulador |
| Interacción VR (agarrar/rayos) | Sí | Sí (editor) | Medallas y armas usan XR Grab Interactable; máquina/estaciones usan XR Simple Interactable |
| Mecánica principal (core loop) | Sí | Parcial | Medallas → máquina → recompensa implementado; falta recorrido completo con jugador real |
| Feedback (visual/sonoro/contextual) | Parcial | No | Mensajes de HUD y efectos por código listos; faltan sonidos reales (sin assets de audio aún) |
| Scriptable Objects | Sí | Sí | GameConfig, MedalData, WeaponData, EnemyData, RewardData con datos reales creados |
| Guardado | Sí | Parcial | SaveSystem en JSON implementado; probado por código, falta demostrar ciclo completo jugar→guardar→cerrar→cargar con el jugador |
| Carga | Sí | Parcial | Igual que guardado |
| Git | Sí | Sí | Historial de commits pequeños y semánticos, ramas por feature, mergeadas a main |
| GitHub | Sí | Sí | Repo `C4BALLERO/Shoter_VR` con main y todas las ramas de feature subidas |
| Trello | No | No | Pendiente de crear por el equipo (columnas BACKLOG/TO DO/DOING/TO TEST/TESTING/DONE) |
| Miro | No | No | Pendiente de crear por el equipo (idea, referencias, core loop, wireframes, gray box) |
| Gray Box | Sí | Sí | Piso, paredes, divisor de zonas, todos los objetos jugables colocados con primitivos |
| Wireframes | No | No | Pendiente (menú, HUD, interacción con la máquina) |
| Pruebas XR Simulator | Sí | Sí | Smoke test automatizado sin errores en Play Mode |
| Pruebas Oculus Quest físico | No | No | Pendiente — requiere hardware físico, ver `docs/PRUEBAS_QUEST.md` |
| UI | Sí | Sí (editor) | HUD World Space con medallas/puntos/munición/mensajes, probado en Play Mode |
| Sonido | No | No | Sin assets de audio todavía; los scripts ya soportan AudioClip donde corresponde |
| Optimización | Parcial | No | Gray box con primitivos simples (bajo poly); falta medir FPS/memoria real en Quest |
| Presentación | No | No | Pendiente de preparar |

## Notas

- Las armas incluyen Pistola, Escopeta y Subfusil, cada una con su propio `WeaponData`.
- La escena guarda en formato binario (no YAML de texto); no afecta la funcionalidad pero limita el diff en revisiones de Git — ver README para detalle.
- Actualizar esta tabla a medida que se complete cada pendiente; no marcar "Sí" en Probado sin haberlo corrido de verdad.
