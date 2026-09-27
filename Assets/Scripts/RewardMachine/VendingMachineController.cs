using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Medallas.Medals;
using Medallas.UI;

namespace Medallas.RewardMachine
{
    [Serializable]
    public class VendingPrize
    {
        public string label;
        public GameObject weaponPrefab;
        [Min(0f)] public float weight = 1f;
    }

    // "Tiro de suerte": cada tirada gasta medallas disponibles y suelta un
    // arma al azar. El peso de cada premio define su rareza.
    public class VendingMachineController : MonoBehaviour
    {
        public XRSimpleInteractable button;
        public int costInMedals = 1;
        public VendingPrize[] prizes;
        public Transform dispensePoint;
        public float dispenseSpeed = 1.2f;
        public float spinDuration = 1.2f;
        public Light machineLight;
        public AudioClip spinSound;
        public AudioClip dispenseSound;
        public AudioClip denySound;
        public HUDController hud;

        public event Action<VendingPrize, GameObject> PrizeDispensed;

        AudioSource audioSource;
        bool busy;

        public bool IsBusy => busy;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        void OnEnable()
        {
            if (button != null) button.selectEntered.AddListener(OnButtonPressed);
        }

        void OnDisable()
        {
            if (button != null) button.selectEntered.RemoveListener(OnButtonPressed);
            busy = false;
        }

        void OnButtonPressed(SelectEnterEventArgs args)
        {
            TryPull();
        }

        public bool TryPull()
        {
            if (busy) return false;

            var medals = MedalManager.Instance;
            if (medals == null || !medals.TrySpendMedals(costInMedals))
            {
                Play(denySound);
                hud?.ShowMessage($"NECESITAS {costInMedals} MEDALLA{(costInMedals == 1 ? "" : "S")}");
                return false;
            }

            StartCoroutine(PullRoutine());
            return true;
        }

        IEnumerator PullRoutine()
        {
            busy = true;
            hud?.ShowMessage("TIRO DE SUERTE...");
            Play(spinSound);

            float baseIntensity = machineLight != null ? machineLight.intensity : 0f;
            for (float t = 0f; t < spinDuration; t += Time.deltaTime)
            {
                if (machineLight != null)
                    machineLight.intensity = baseIntensity * (0.3f + Mathf.PingPong(t * 10f, 1.4f));
                yield return null;
            }
            if (machineLight != null) machineLight.intensity = baseIntensity;

            var prize = RollPrize();
            if (prize != null && prize.weaponPrefab != null)
            {
                Transform spawn = dispensePoint != null ? dispensePoint : transform;
                var weapon = Instantiate(prize.weaponPrefab, spawn.position, spawn.rotation);
                if (weapon.TryGetComponent(out Rigidbody rb))
                    rb.linearVelocity = spawn.forward * dispenseSpeed;

                Play(dispenseSound);
                hud?.ShowMessage("TE TOCO: " + prize.label.ToUpper());
                PrizeDispensed?.Invoke(prize, weapon);
            }

            busy = false;
        }

        VendingPrize RollPrize()
        {
            if (prizes == null || prizes.Length == 0) return null;

            float total = 0f;
            foreach (var p in prizes) total += Mathf.Max(0f, p.weight);
            if (total <= 0f) return prizes[0];

            float roll = UnityEngine.Random.value * total;
            foreach (var p in prizes)
            {
                roll -= Mathf.Max(0f, p.weight);
                if (roll <= 0f) return p;
            }
            return prizes[prizes.Length - 1];
        }

        void Play(AudioClip clip)
        {
            if (clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
