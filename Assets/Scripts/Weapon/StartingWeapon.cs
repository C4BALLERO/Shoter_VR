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

        IEnumerator Start()
        {
            yield return null;
            yield return new WaitForSeconds(0.3f);

            if (weapon == null) yield break;

            var manager = Object.FindFirstObjectByType<XRInteractionManager>();
            if (manager == null) yield break;

            var interactors = Object.FindObjectsByType<NearFarInteractor>(FindObjectsSortMode.None);
            if (interactors.Length == 0) yield break;

            NearFarInteractor chosen = null;
            foreach (var interactor in interactors)
            {
                bool isRight = interactor.name.ToLower().Contains("right");
                if (isRight == rightHand) { chosen = interactor; break; }
            }
            if (chosen == null) chosen = interactors[0];

            manager.SelectEnter(chosen, (IXRSelectInteractable)weapon);
        }
    }
}
