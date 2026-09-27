using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Medallas.Data;

namespace Medallas.UI
{
    // Boton fisico para elegir dificultad en el menu de inicio. Brilla cuando
    // su modo es el seleccionado.
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class DifficultyButton : MonoBehaviour
    {
        public StartMenuController menu;
        public int difficultyIndex;
        public Renderer buttonRenderer;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        XRSimpleInteractable interactable;
        MaterialPropertyBlock block;
        Vector3 baseScale;

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            if (buttonRenderer == null) buttonRenderer = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
            baseScale = transform.localScale;
        }

        void OnEnable()
        {
            interactable.selectEntered.AddListener(OnPressed);
            if (menu != null)
            {
                menu.DifficultyChanged += Refresh;
                Refresh(menu.SelectedDifficulty);
            }
        }

        void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnPressed);
            if (menu != null) menu.DifficultyChanged -= Refresh;
        }

        void OnPressed(SelectEnterEventArgs args)
        {
            menu?.SelectDifficulty(difficultyIndex);
        }

        void Refresh(DifficultyData selected)
        {
            if (menu == null || buttonRenderer == null) return;
            var mine = menu.GetDifficulty(difficultyIndex);
            if (mine == null) return;

            bool isSelected = mine == selected;
            buttonRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, isSelected ? mine.buttonColor : mine.buttonColor * 0.45f);
            block.SetColor(EmissionId, mine.buttonColor * (isSelected ? 2.2f : 0.05f));
            buttonRenderer.SetPropertyBlock(block);

            // El seleccionado sobresale de la pared, como un boton presionado al reves.
            transform.localScale = isSelected ? new Vector3(baseScale.x, baseScale.y, baseScale.z * 1.8f) : baseScale;
        }
    }
}
