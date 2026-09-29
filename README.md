# MEDALLAS: Zona Perdida

Shooter VR corto de exploración y supervivencia para Oculus Quest, desarrollado en Unity con XR Interaction Toolkit como proyecto de auto-aprendizaje de Realidad Virtual y Aumentada.

## Descripción y objetivo

El jugador despierta en una pequeña instalación abandonada. Debe explorar el lugar, disparar objetivos y enemigos, encontrar medallas escondidas y llevarlas a una máquina de recompensas antigua para desbloquear la salida.

Todo el contenido (nombres, escenario, mecánica) es original; el proyecto **no** reutiliza personajes, mapas, modelos, sonidos ni marcas de ningún juego comercial existente.

## Público objetivo

Jugadores casuales de VR que buscan una experiencia corta (10-20 min), de exploración y puntería, sin curva de aprendizaje pronunciada ni requerimientos de espacio de juego amplio (locomoción cómoda, sentado o de pie).

## Mecánica principal / Core Loop

```
EXPLORAR → LOCALIZAR OBJETIVO → DISPARAR / INTERACTUAR → OBTENER MEDALLA
   → CONSEGUIR MÁS MEDALLAS → REGRESAR A LA MÁQUINA → INTRODUCIR MEDALLAS
   → ACTIVAR MÁQUINA → RECIBIR RECOMPENSA → COMPLETAR OBJETIVO
```

- **Medallas**: 8 coleccionables físicos repartidos por el escenario, se recogen con agarre VR (XR Grab Interactable).
- **Armas**: Pistola (semiautomática), Escopeta (6 perdigones con dispersión, corto alcance) y Subfusil (automático, cadencia alta). Todas disparan por raycast y consumen munición del cargador; se recargan desde la reserva (la pistola tiene reserva infinita para que el jugador nunca quede indefenso).
- **Máquina expendedora ("tiro de suerte")**: cada tirada gasta 1 medalla disponible y suelta un arma al azar en su bandeja (Pistola 45%, Subfusil 35%, Escopeta 20%). Gastar medallas no resta de las recogidas: la máquina de recompensa final sigue contando todas las que encontraste.
- **Tienda de medallas**: panel en la pared sur con 4 compras: Curar +50 vida (1 medalla), Daño x2 por 20 s (2), Escudo -50% de daño por 20 s (2) y Disparo rápido por 20 s (1). Si la compra no se puede aplicar (vida llena o jugador caído) no se cobra; comprar un power-up activo suma tiempo.
- **Dificultad**: Fácil, Medio o Difícil, elegida con los botones de la barrera antes de iniciar. Cambia la vida del jugador (150/100/80), la vida, daño y velocidad de los enemigos, su frecuencia y puntería al lanzar hachas, y cuántos aparecen por oleada (4/5/7).
- **Niveles**: un nivel son 5 oleadas. Al terminarlo se autoguarda y el botón verde pasa a "Siguiente nivel", más difícil que el anterior (+25% vida y +15% daño enemigo, +5% velocidad, hachas un 10% más seguido y +1 enemigo por oleada por nivel, sobre el modo elegido). Desde el nivel 2 algunos enemigos cargan medallas extra (hasta 4 por nivel) que solo suman saldo para la tienda y la expendedora.
- **Continuar otro día**: el menú muestra "Nueva partida" (verde) y, si hay partida guardada, "Continuar" (azul) con el nivel y modo guardados. Continuar restaura medallas, puntos y bajas y retoma desde el inicio del nivel guardado; Nueva partida reemplaza la partida guardada.
- **Al quedarse sin vida**: el combate se congela y aparece frente al jugador el menú "HAS CAÍDO" con dos opciones: **Reiniciar nivel** (vuelve al inicio del nivel con las medallas, puntos y bajas que tenía al empezarlo) o **Continuar por 2 medallas** (revive en el mismo punto con la vida llena y 3 s de invulnerabilidad).
- **Galería de tiro**: 3 objetivos reutilizables que suman puntos y reaparecen tras un tiempo.
- **Enemigos**: 2 tipos simples (Rondador, Corredor) que persiguen y atacan al jugador usando NavMesh.
- **Máquina de recompensa**: al reunir la cantidad configurada de medallas, entrega una recompensa (llave de salida).
- **Guardado**: progreso persistente en JSON, con estaciones físicas de Guardar/Cargar en el escenario.
- **HUD**: arriba a la izquierda medallas, puntos y bajas; abajo a la izquierda el icono del arma en uso con su munición; abajo al centro la barra de vida. Se dibuja siempre por encima de la escena (shader `Medallas/UI Overlay`).

## Arquitectura

```
Assets/
  Scripts/
    Core/            Health, IDamageable, ScoreManager, ShootingTarget, PlayerHurtbox, PlayerPowerUps
    Weapon/           WeaponController, AmmoSystem, DamageSystem
    Medals/           Medal, MedalManager
    Enemies/          EnemyAI, WaveManager, ThrownProjectile
    RewardMachine/     RewardMachineController, VendingMachineController, MedalShopController
    SaveSystem/       SaveData, SaveSystem, SaveLoadTrigger
    UI/               HUDController, StartMenuController, DifficultyButton, GameOverController
    ScriptableObjects/ GameConfig, MedalData, WeaponData, EnemyData, RewardData, DifficultyData
  Data/               Assets .asset reales (medallas, armas, enemigos, recompensa, config)
  Samples/            XR Interaction Toolkit Starter Assets + XR Device Simulator
  Scenes/SampleScene.unity   Escena única (gray box) con todo conectado
```

### Scriptable Objects

Cada uno resuelve un problema real de mantenimiento/balance, evitando hardcodear valores en el código:

| Scriptable Object | Qué contiene | Por qué existe |
|---|---|---|
| `GameConfig` | Medallas requeridas, puntajes, comfort VR | Un solo lugar para ajustar el balance general sin tocar scripts |
| `MedalData` | id, nombre, descripción, valor, tipo, recompensa asociada | Cada medalla es un asset independiente; agregar/quitar medallas no requiere código nuevo |
| `WeaponData` | daño, cargador, cargadores de repuesto, tiempo de recarga, cadencia, alcance, automático, perdigones/dispersión | Permite tener 3 armas distintas reusando el mismo `WeaponController` |
| `EnemyData` | salud, velocidad, daño, rangos de detección/ataque, puntaje | Balance de IA separado del script de comportamiento |
| `RewardData` | nombre, descripción, tipo, prefab de recompensa | La recompensa final se cambia sin tocar `RewardMachineController` |
| `DifficultyData` | vida del jugador, multiplicadores de vida/daño/velocidad/lanzamiento/puntería de enemigos, enemigos por oleada | Un asset por modo (Fácil/Medio/Difícil): se balancea cada modo sin tocar `EnemyAI` ni `WaveManager` |

## Sistema de guardado

`SaveSystem` (estático) serializa un `SaveData` (medallas recogidas, extra y gastadas, puntaje, bajas, munición, si la recompensa ya fue entregada, nivel a retomar y dificultad elegida) a JSON en `Application.persistentDataPath/savegame.json` mediante `JsonUtility`. Se demuestra con las dos estaciones físicas "SaveStation" / "LoadStation" en la escena (interacción VR simple), o llamando a `SaveSystem.SaveGame()` / `SaveSystem.LoadGame()` desde código.

## Controles

- **Movimiento**: joystick del controlador (locomoción continua, Starter Assets de XRI).
- **Giro**: snap turn configurable.
- **Agarrar objetos** (medallas, armas): botón de grip, acercando la mano al objeto.
- **Disparar**: gatillo del controlador mientras se sostiene un arma.
- **Recargar**: tecla **M** (simulador en PC) o botón **A/X** del control (Quest), con el arma en la mano.
- **Interactuar** (botones de dificultad e inicio, máquina de recompensa, máquina expendedora, tienda, estaciones de guardado): botón de selección apuntando/tocando el objeto.
- En el editor, todo lo anterior se prueba con el **XR Device Simulator** (mouse + teclado) sin necesidad del headset.

## Requisitos y ejecución

- Unity **6000.3.23f1** (Unity 6.3 LTS).
- Paquetes: XR Interaction Toolkit 3.3.2, XR Plugin Management 4.5.4, OpenXR Plugin 1.16.1.
- Build target: Android (IL2CPP, ARM64, minSdk 29) para Oculus Quest.
- Para probar en el editor: abrir `Assets/Scenes/SampleScene.unity` y entrar en Play Mode (usa el XR Device Simulator automáticamente si no hay headset conectado).
- Para compilar a Quest: `File > Build Settings`, con Android ya seleccionado como plataforma activa, y el visor conectado por cable o Link/Air Link.

## Problemas conocidos

- La escena (`SampleScene.unity`) se guarda en formato binario en vez de YAML de texto legible; no afecta la funcionalidad, pero dificulta los diffs de Git en revisión de código.
- El perfil de interacción "Oculus Touch Controller Profile" de OpenXR está habilitado manualmente para Android y Standalone.
- Falta arte final, sonido ambiental y pruebas físicas en dispositivo Quest.

## Estado del proyecto

Ver la matriz de requisitos y el tablero de Trello/Miro del equipo para el detalle de qué está implementado, probado y pendiente.
