using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Management;

namespace Medallas.Core
{
    // El XR Device Simulator elimina del Input System cualquier visor real al
    // activarse (y no lo devuelve al desactivarse), asi que con un Quest
    // conectado el tracking de la cabeza no llegaria al juego. Por eso el
    // simulador queda INACTIVO en la escena y este arranque solo lo enciende
    // en el editor cuando no hay un visor real funcionando.
    public static class XRSimulatorBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ActivateSimulatorIfNoHeadset()
        {
            if (!Application.isEditor || IsRealHeadsetRunning()) return;

            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.activeSelf) continue;
                if (root.GetComponentInChildren<XRDeviceSimulator>(true) == null) continue;

                root.SetActive(true);
                Debug.Log("[XRSimulatorBootstrap] Sin visor conectado: se activa el XR Device Simulator.");
            }
        }

        public static bool IsRealHeadsetRunning()
        {
            var manager = XRGeneralSettings.Instance != null ? XRGeneralSettings.Instance.Manager : null;
            if (manager == null || manager.activeLoader == null) return false;

            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);
            foreach (var display in displays)
                if (display.running) return true;
            return false;
        }
    }
}
