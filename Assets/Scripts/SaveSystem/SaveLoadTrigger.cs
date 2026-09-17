using UnityEngine;

namespace Medallas.SaveSystem
{
    // Puente para que un boton de UI o una interaccion VR puedan llamar a
    // SaveSystem (UnityEvent del inspector no puede apuntar a metodos estaticos).
    public class SaveLoadTrigger : MonoBehaviour
    {
        public void SaveGame() => SaveSystem.SaveGame();
        public void LoadGame() => SaveSystem.LoadGame();
    }
}
