using UnityEngine;
using Medallas.Enemies;
using Medallas.Weapon;

namespace Medallas.UI
{
    // Pantalla de inicio simple: mientras esta activa, las oleadas de
    // enemigos no arrancan y el jugador no tiene arma en mano. Se cierra
    // tocando un boton fisico (XR Simple Interactable).
    public class StartMenuController : MonoBehaviour
    {
        public GameObject menuPanel;
        public WaveManager waveManager;
        public HUDController hud;
        public StartingWeapon startingWeapon;

        public void StartGame()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
            startingWeapon?.Equip();
            waveManager?.BeginGame();
            hud?.ShowMessage("COMIENZA LA PARTIDA");
        }
    }
}
