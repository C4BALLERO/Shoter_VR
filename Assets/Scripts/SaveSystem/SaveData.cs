using System;
using System.Collections.Generic;

namespace Medallas.SaveSystem
{
    // Todo lo que se persiste entre sesiones. Un unico objeto serializable en
    // vez de que cada sistema lea/escriba su propio archivo.
    [Serializable]
    public class SaveData
    {
        public List<string> collectedMedalIds = new List<string>();
        public int spentMedals;
        public int bonusMedals;
        public int score;
        public int kills;
        public int ammo;
        public bool rewardGranted;

        // Nivel desde el que se retoma al elegir "Continuar" y modo elegido.
        public int level = 1;
        public int difficultyIndex = 1;
        public string savedAt;
    }
}
