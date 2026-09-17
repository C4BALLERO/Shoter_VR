using UnityEngine;

namespace Medallas.Data
{
    public enum MedalType
    {
        Exploracion,
        Puntaje,
        Especial
    }

    // Un asset por medalla (Medal_01.asset, Medal_02.asset, ...). Permite a diseno
    // agregar/ajustar medallas sin tocar los scripts que las recogen o las cuentan.
    [CreateAssetMenu(fileName = "Medal_00", menuName = "Medallas/Medal Data")]
    public class MedalData : ScriptableObject
    {
        public string medalId;
        public string medalName;
        [TextArea] public string description;
        public Sprite icon;
        public int value = 1;
        public MedalType type = MedalType.Exploracion;
        public RewardData associatedReward;
    }
}
