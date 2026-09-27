using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Medallas.Core;
using Medallas.Medals;
using Medallas.UI;

namespace Medallas.RewardMachine
{
    public enum ShopItemType
    {
        Heal,
        DoubleDamage,
        Shield,
        RapidFire
    }

    [Serializable]
    public class ShopItem
    {
        public string label;
        public ShopItemType type;
        public int cost = 1;
        public int healAmount = 50;
        public float duration = 20f;
        public XRSimpleInteractable button;
    }

    // Tienda fisica: cada boton gasta medallas disponibles en curacion o en
    // un power-up temporal. Si la compra no se puede aplicar (vida llena,
    // jugador caido) no se cobra.
    public class MedalShopController : MonoBehaviour
    {
        public ShopItem[] items;
        public Health playerHealth;
        public PlayerPowerUps powerUps;
        public HUDController hud;
        public AudioClip buySound;
        public AudioClip denySound;

        public event Action<ShopItem> Purchased;

        AudioSource audioSource;
        readonly List<(XRSimpleInteractable button, UnityEngine.Events.UnityAction<SelectEnterEventArgs> handler)> bindings
            = new List<(XRSimpleInteractable, UnityEngine.Events.UnityAction<SelectEnterEventArgs>)>();

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        void OnEnable()
        {
            if (items == null) return;
            foreach (var item in items)
            {
                if (item.button == null) continue;
                var captured = item;
                UnityEngine.Events.UnityAction<SelectEnterEventArgs> handler = _ => TryBuy(captured);
                item.button.selectEntered.AddListener(handler);
                bindings.Add((item.button, handler));
            }
        }

        void OnDisable()
        {
            foreach (var (button, handler) in bindings)
                if (button != null) button.selectEntered.RemoveListener(handler);
            bindings.Clear();
        }

        public bool TryBuy(ShopItem item)
        {
            if (item == null) return false;

            if (playerHealth != null && playerHealth.IsDead)
                return Deny("HAS CAIDO - NO PUEDES COMPRAR");

            if (item.type == ShopItemType.Heal && playerHealth != null && playerHealth.IsFull)
                return Deny("TU VIDA YA ESTA COMPLETA");

            var medals = MedalManager.Instance;
            if (medals == null || !medals.TrySpendMedals(item.cost))
                return Deny($"NECESITAS {item.cost} MEDALLA{(item.cost == 1 ? "" : "S")}");

            Apply(item);
            if (buySound != null) audioSource.PlayOneShot(buySound);
            hud?.ShowMessage("COMPRADO: " + item.label.ToUpper());
            Purchased?.Invoke(item);
            return true;
        }

        void Apply(ShopItem item)
        {
            switch (item.type)
            {
                case ShopItemType.Heal:
                    playerHealth?.Heal(item.healAmount);
                    break;
                case ShopItemType.DoubleDamage:
                    powerUps?.Activate(PowerUpType.DoubleDamage, item.duration);
                    break;
                case ShopItemType.Shield:
                    powerUps?.Activate(PowerUpType.Shield, item.duration);
                    break;
                case ShopItemType.RapidFire:
                    powerUps?.Activate(PowerUpType.RapidFire, item.duration);
                    break;
            }
        }

        bool Deny(string message)
        {
            if (denySound != null) audioSource.PlayOneShot(denySound);
            hud?.ShowMessage(message);
            return false;
        }
    }
}
