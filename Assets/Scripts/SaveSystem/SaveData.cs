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
        public int score;
        public int ammo;
        public bool rewardGranted;
    }
}
