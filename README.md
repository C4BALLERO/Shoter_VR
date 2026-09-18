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
- **Armas**: Pistola (semiautomática), Escopeta (6 perdigones con dispersión, corto alcance) y Subfusil (automático, cadencia alta). Todas disparan por raycast y consumen munición.
- **Galería de tiro**: 3 objetivos reutilizables que suman puntos y reaparecen tras un tiempo.
- **Enemigos**: 2 tipos simples (Rondador, Corredor) que persiguen y atacan al jugador usando NavMesh.
- **Máquina de recompensa**: al reunir la cantidad configurada de medallas, entrega una recompensa (llave de salida).
- **Guardado**: progreso persistente en JSON, con estaciones físicas de Guardar/Cargar en el escenario.

## Arquitectura

```
Assets/
  Scripts/
    Core/            Health, IDamageable, ScoreManager, ShootingTarget
    Weapon/           WeaponController, AmmoSystem, DamageSystem
    Medals/           Medal, MedalManager
    Enemies/          EnemyAI
    RewardMachine/     RewardMachineController
    SaveSystem/       SaveData, SaveSystem, SaveLoadTrigger
    UI/               HUDController
    ScriptableObjects/ GameConfig, MedalData, WeaponData, EnemyData, RewardData
  Data/               Assets .asset reales (medallas, armas, enemigos, recompensa, config)
  Samples/            XR Interaction Toolkit Starter Assets + XR Device Simulator
  Scenes/SampleScene.unity   Escena única (gray box) con todo conectado
```

### Scriptable Objects

Cada uno resuelve un problema real de mantenimiento/balance, evitando hardcodear valores en el código:

| Scriptable Object | Qué contiene | Por qué existe |
|---|---|---|
| `GameConfig` | Medallas requeridas, munición inicial, puntajes, comfort VR | Un solo lugar para ajustar el balance general sin tocar scripts |
| `MedalData` | id, nombre, descripción, valor, tipo, recompensa asociada | Cada medalla es un asset independiente; agregar/quitar medallas no requiere código nuevo |
| `WeaponData` | daño, munición, cadencia, alcance, automático, perdigones/dispersión | Permite tener 3 armas distintas reusando el mismo `WeaponController` |
| `EnemyData` | salud, velocidad, daño, rangos de detección/ataque, puntaje | Balance de IA separado del script de comportamiento |
| `RewardData` | nombre, descripción, tipo, prefab de recompensa | La recompensa final se cambia sin tocar `RewardMachineController` |

## Sistema de guardado

`SaveSystem` (estático) serializa un `SaveData` (medallas recogidas, puntaje, munición, si la recompensa ya fue entregada) a JSON en `Application.persistentDataPath/savegame.json` mediante `JsonUtility`. Se demuestra con las dos estaciones físicas "SaveStation" / "LoadStation" en la escena (interacción VR simple), o llamando a `SaveSystem.SaveGame()` / `SaveSystem.LoadGame()` desde código.

## Controles

- **Movimiento**: joystick del controlador (locomoción continua, Starter Assets de XRI).
- **Giro**: snap turn configurable.
- **Agarrar objetos** (medallas, armas): botón de grip, acercando la mano al objeto.
- **Disparar**: gatillo del controlador mientras se sostiene un arma.
- **Interactuar** (máquina de recompensa, estaciones de guardado): botón de selección apuntando/tocando el objeto.
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
