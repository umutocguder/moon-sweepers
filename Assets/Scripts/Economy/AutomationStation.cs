using UnityEngine;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Environment;

namespace ElevenHopem.MoonSweepers.Economy
{
    public class AutomationStation : MonoBehaviour
    {
        public string stationType = "conveyor";
        [SerializeField] private float cleanRadius = 12.0f;
        [SerializeField] private float harvestRateKgPerSec = 4.0f;
        [SerializeField] private float patrolSpeed = 2.5f;

        private float nextTick = 0f;
        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.position;
            CreateVisualRepresentation();
        }

        private void CreateVisualRepresentation()
        {
            if (stationType == "drone")
            {
                // Drone body
                GameObject droneBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                droneBody.transform.SetParent(this.transform);
                droneBody.transform.localPosition = new Vector3(0, 3f, 0);
                droneBody.transform.localScale = new Vector3(1.2f, 0.4f, 1.2f);

                Renderer rend = droneBody.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    rend.material.color = new Color(0.2f, 0.8f, 0.9f);
                }

                Light droneSpot = droneBody.AddComponent<Light>();
                droneSpot.type = LightType.Spot;
                droneSpot.range = 8f;
                droneSpot.spotAngle = 45f;
                droneSpot.color = Color.cyan;
                droneSpot.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            }
            else // conveyor
            {
                GameObject belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
                belt.transform.SetParent(this.transform);
                belt.transform.localPosition = Vector3.zero;
                belt.transform.localScale = new Vector3(1.4f, 0.3f, 6.0f);

                Renderer rend = belt.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    rend.material.color = new Color(0.85f, 0.55f, 0.1f);
                }
            }
        }

        private void Update()
        {
            if (stationType == "drone")
            {
                // Patrol orbit
                float angle = Time.time * patrolSpeed * 0.3f;
                transform.position = initialPosition + new Vector3(Mathf.Cos(angle) * 8f, 0f, Mathf.Sin(angle) * 8f);
            }

            if (Time.time >= nextTick)
            {
                nextTick = Time.time + 1.0f;
                HarvestNearbyDust();
            }
        }

        private void HarvestNearbyDust()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, cleanRadius);
            foreach (var col in hits)
            {
                RegolithDustPatch patch = col.GetComponent<RegolithDustPatch>();
                if (patch != null && !patch.IsFullyCleaned)
                {
                    float harvested = patch.CleanDust(harvestRateKgPerSec);
                    if (harvested > 0)
                    {
                        // Direct transfer into credits at depot rate
                        float rate = GameManager.Instance != null ? GameManager.Instance.BaseDustValuePerKg : 2.5f;
                        long earned = (long)Mathf.Round(harvested * rate);
                        GameManager.Instance?.AddCredits(earned);
                        break;
                    }
                }
            }
        }
    }
}
