using System;
using UnityEngine;
using ElevenHopem.MoonSweepers.Player;

namespace ElevenHopem.MoonSweepers.Environment
{
    public class RegolithDustPatch : MonoBehaviour
    {
        [Header("Dust Configuration")]
        [SerializeField] private float maxDustMassKg = 15f;
        [SerializeField] private float currentDustMassKg = 15f;
        [SerializeField] private bool canDropMinerals = true;
        [SerializeField] private float mineralDropChance = 0.15f;

        [Header("Respawn")]
        [SerializeField] private bool autoRespawn = true;
        [SerializeField] private float respawnCooldownSeconds = 45f;

        private Vector3 initialScale;
        private float respawnTimer = 0f;
        private MeshRenderer meshRenderer;
        private Collider patchCollider;

        public float CurrentDustMassKg => currentDustMassKg;
        public bool IsFullyCleaned => currentDustMassKg <= 0.05f;

        public event Action<float> OnDustCleaned;

        private void Awake()
        {
            initialScale = transform.localScale;
            currentDustMassKg = maxDustMassKg;
            meshRenderer = GetComponent<MeshRenderer>();
            patchCollider = GetComponent<Collider>();
            gameObject.tag = "DustPatch";
        }

        private void Update()
        {
            if (IsFullyCleaned && autoRespawn)
            {
                respawnTimer += Time.deltaTime;
                if (respawnTimer >= respawnCooldownSeconds)
                {
                    Respawn();
                }
            }
        }

        public float CleanDust(float amountKg)
        {
            if (IsFullyCleaned) return 0f;

            float cleaned = Mathf.Min(amountKg, currentDustMassKg);
            currentDustMassKg -= cleaned;

            // Visual scale shrink
            float ratio = Mathf.Clamp01(currentDustMassKg / maxDustMassKg);
            transform.localScale = new Vector3(
                initialScale.x * (0.2f + 0.8f * ratio),
                initialScale.y * ratio,
                initialScale.z * (0.2f + 0.8f * ratio)
            );

            OnDustCleaned?.Invoke(cleaned);

            if (IsFullyCleaned)
            {
                OnFullyCleaned();
            }

            return cleaned;
        }

        private void OnFullyCleaned()
        {
            if (canDropMinerals && UnityEngine.Random.value < mineralDropChance)
            {
                AwardRareMineral();
            }

            if (meshRenderer != null) meshRenderer.enabled = false;
            if (patchCollider != null) patchCollider.enabled = false;
            respawnTimer = 0f;
        }

        private void AwardRareMineral()
        {
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory == null) return;

            float roll = UnityEngine.Random.value;
            if (roll < 0.60f)
            {
                inventory.AddMineral("Titanium Chunk", 1, 15f);
            }
            else if (roll < 0.90f)
            {
                inventory.AddMineral("Helium-3 Canister", 1, 45f);
            }
            else
            {
                inventory.AddMineral("Ancient Alien Screw", 1, 120f);
            }
        }

        public void Respawn()
        {
            currentDustMassKg = maxDustMassKg;
            transform.localScale = initialScale;
            if (meshRenderer != null) meshRenderer.enabled = true;
            if (patchCollider != null) patchCollider.enabled = true;
            respawnTimer = 0f;
        }
    }
}
