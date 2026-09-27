using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Medallas.Weapon
{
    // Fuerza el agarre de un arma al iniciar la partida, para que el jugador
    // no tenga que buscarla antes de poder defenderse.
    public class StartingWeapon : MonoBehaviour
    {
        public XRGrabInteractable weapon;
        public bool rightHand = true;

        // Se llama desde StartMenuController al presionar el boton de inicio,
        // no automaticamente al cargar la escena: el jugador no debe tener el
        // arma en mano mientras todavia esta mirando el menu.
        public void Equip()
        {
            StartCoroutine(EquipRoutine());
        }

        IEnumerator EquipRoutine()
        {
            yield return null;

            if (weapon == null) yield break;

            var manager = Object.FindFirstObjectByType<XRInteractionManager>();
            if (manager == null) yield break;

            var interactors = Object.FindObjectsByType<NearFarInteractor>(FindObjectsSortMode.None);
            if (interactors.Length == 0) yield break;

            // Los dos interactores se llaman igual ("Near-Far Interactor"); la
            // mano se sabe por su handedness, no por el nombre.
            var wanted = rightHand ? InteractorHandedness.Right : InteractorHandedness.Left;
            NearFarInteractor chosen = null;
            foreach (var interactor in interactors)
            {
                if (interactor.handedness == wanted) { chosen = interactor; break; }
            }
            if (chosen == null) chosen = interactors[0];

            manager.SelectEnter(chosen, (IXRSelectInteractable)weapon);
        }
    }
}
