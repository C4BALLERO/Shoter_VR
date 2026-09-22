using UnityEngine;
using Medallas.Enemies;

namespace Medallas.UI
{
    // Pantalla de inicio simple: mientras esta activa, las oleadas de
    // enemigos no arrancan. El jugador la cierra tocando un boton fisico
    // (XR Simple Interactable) para dar tiempo a ubicarse antes de jugar.
    public class StartMenuController : MonoBehaviour
    {
        public GameObject menuPanel;
        public WaveManager waveManager;
        public HUDController hud;

        public void StartGame()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
            waveManager?.BeginGame();
            hud?.ShowMessage("COMIENZA LA PARTIDA");
        }
    }
}
