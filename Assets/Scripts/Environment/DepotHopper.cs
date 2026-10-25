using System;
using System.Collections.Generic;
using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Player;

namespace ElevenHopem.MoonSweepers.Environment
{
    public class DepotHopper : MonoBehaviour
    {
        [Header("Depot Zone")]
        [SerializeField] private float depositRadius = 4.0f;
        [SerializeField] private Light depotBeaconLight;
        [SerializeField] private ParticleSystem unloadParticles;

        private bool playerInRange = false;
        private PlayerInventory nearbyInventory;

        public bool PlayerInRange => playerInRange;

        public event Action<float, long> OnDustDeposited;

        private void Awake()
        {
            gameObject.tag = "Depot";

            SphereCollider col = GetComponent<SphereCollider>();
            if (col == null)
            {
                col = gameObject.AddComponent<SphereCollider>();
            }
            col.isTrigger = true;
            col.radius = depositRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                nearbyInventory = other.GetComponent<PlayerInventory>() ?? other.GetComponentInParent<PlayerInventory>();
                if (depotBeaconLight != null) depotBeaconLight.color = Color.green;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                nearbyInventory = null;
                if (depotBeaconLight != null) depotBeaconLight.color = Color.cyan;
            }
        }

        private void Update()
        {
            // Auto-deposit or E key press
            if (playerInRange && nearbyInventory != null)
            {
                if (Input.GetKeyDown(KeyCode.E) || nearbyInventory.CurrentDustKg > 0)
                {
                    ExecuteDeposit();
                }
            }
        }

        public void ExecuteDeposit()
        {
            if (nearbyInventory == null) return;

            float dustAmount = nearbyInventory.UnloadAllDust();
            List<MineralItem> minerals = nearbyInventory.UnloadAllMinerals();

            if (dustAmount <= 0 && minerals.Count == 0) return;

            float baseRate = GameManager.Instance != null ? GameManager.Instance.BaseDustValuePerKg : 2.5f;
            long dustCredits = (long)Mathf.Round(dustAmount * baseRate);

            long mineralCredits = 0;
            foreach (var m in minerals)
            {
                mineralCredits += (long)(m.count * m.valueMultiplier);
            }

            long totalCredits = dustCredits + mineralCredits;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCredits(totalCredits);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoinChime();
            }

            if (unloadParticles != null)
            {
                unloadParticles.Play();
            }

            OnDustDeposited?.Invoke(dustAmount, totalCredits);
            Debug.Log($"[DepotHopper] Successfully unloaded {dustAmount:F1} kg dust for {totalCredits} LC!");
        }
    }
}
