using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Medallas.UI
{
    // Puente entre el boton fisico de inicio y el StartMenuController
    // (UnityEvent del inspector no hace falta: se conecta por codigo).
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class StartButtonRelay : MonoBehaviour
    {
        public StartMenuController controller;
        [Tooltip("Boton azul de continuar la partida guardada (en vez del verde de iniciar).")]
        public bool isContinueButton;

        void OnEnable()
        {
            GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnSelectEntered);
        }

        void OnDisable()
        {
            GetComponent<XRSimpleInteractable>().selectEntered.RemoveListener(OnSelectEntered);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (controller == null) return;
            if (isContinueButton) controller.ContinueGame();
            else controller.StartGame();
        }
    }
}
