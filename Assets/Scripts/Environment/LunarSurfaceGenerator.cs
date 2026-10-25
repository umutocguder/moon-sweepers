using UnityEngine;

namespace ElevenHopem.MoonSweepers.Environment
{
    /// <summary>
    /// Generates procedural lunar craters, dust patches, and boulders on the moon surface.
    /// Completely procedural, ensuring zero external copyrighted asset dependency.
    /// </summary>
    public class LunarSurfaceGenerator : MonoBehaviour
    {
        [Header("Surface Generation Settings")]
        [SerializeField] private int dustPatchCount = 60;
        [SerializeField] private float spawnRadiusMin = 5.0f;
        [SerializeField] private float spawnRadiusMax = 65.0f;
        [SerializeField] private Material dustPatchMaterial;
        [SerializeField] private Material craterRockMaterial;

        [Header("Landing Base Anchor")]
        [SerializeField] private Transform landingBaseCenter;

        private void Start()
        {
            GenerateSurfaceFeatures();
        }

        public void GenerateSurfaceFeatures()
        {
            Vector3 center = landingBaseCenter != null ? landingBaseCenter.position : Vector3.zero;

            // Generate interactive dust patches
            for (int i = 0; i < dustPatchCount; i++)
            {
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float distance = UnityEngine.Random.Range(spawnRadiusMin, spawnRadiusMax);

                Vector3 position = center + new Vector3(Mathf.Cos(angle) * distance, 0.05f, Mathf.Sin(angle) * distance);

                CreateDustPatchObject(position, i);
            }

            // Generate ambient lunar boulders
            for (int i = 0; i < 25; i++)
            {
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float distance = UnityEngine.Random.Range(spawnRadiusMin + 4f, spawnRadiusMax + 10f);
                Vector3 position = center + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

                CreateLunarBoulder(position);
            }
        }

        private void CreateDustPatchObject(Vector3 position, int index)
        {
            GameObject patchObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            patchObj.name = $"DustPatch_{index}";
            patchObj.transform.position = position;
            patchObj.transform.SetParent(this.transform);

            // Flatten cylinder into a ground dust disk
            float size = UnityEngine.Random.Range(1.8f, 3.6f);
            patchObj.transform.localScale = new Vector3(size, 0.08f, size);

            Renderer rend = patchObj.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = dustPatchMaterial;
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    mat.color = new Color(0.28f, 0.28f, 0.32f); // Dark lunar regolith gray
                }
                rend.material = mat;
            }

            RegolithDustPatch patchScript = patchObj.AddComponent<RegolithDustPatch>();
            patchObj.tag = "DustPatch";
        }

        private void CreateLunarBoulder(Vector3 position)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "LunarBoulder";
            rock.transform.position = position;
            rock.transform.SetParent(this.transform);

            float s = UnityEngine.Random.Range(0.8f, 2.4f);
            rock.transform.localScale = new Vector3(s, s * 0.7f, s * 1.1f);
            rock.transform.rotation = UnityEngine.Random.rotation;

            Renderer rend = rock.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = craterRockMaterial;
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    mat.color = new Color(0.45f, 0.45f, 0.48f);
                }
                rend.material = mat;
            }
        }
    }
}
