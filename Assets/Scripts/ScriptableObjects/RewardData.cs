using UnityEngine;

namespace Medallas.Data
{
    public enum RewardType
    {
        Llave,
        Municion,
        Mejora,
        AccesoZona
    }

    // Informacion de la recompensa que entrega la maquina al reunir suficientes
    // medallas. Separar esto en datos permite cambiar la recompensa final sin
    // tocar RewardMachineController.
    [CreateAssetMenu(fileName = "Reward_00", menuName = "Medallas/Reward Data")]
    public class RewardData : ScriptableObject
    {
        public string rewardName;
        [TextArea] public string description;
        public Sprite icon;
        public RewardType type = RewardType.Llave;
        public GameObject rewardPrefab;
    }
}
