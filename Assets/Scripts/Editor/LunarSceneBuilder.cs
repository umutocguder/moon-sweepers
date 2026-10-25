#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ElevenHopem.MoonSweepers.Core;
using ElevenHopem.MoonSweepers.Player;
using ElevenHopem.MoonSweepers.Tools;
using ElevenHopem.MoonSweepers.Environment;
using ElevenHopem.MoonSweepers.Economy;
using ElevenHopem.MoonSweepers.UI;

namespace ElevenHopem.EditorScripts
{
    public static class LunarSceneBuilder
    {
        [MenuItem("11Hopem/Setup Lunar World Scene (One-Click)")]
        public static void SetupLunarScene()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            Debug.Log($"[11Hopem] Constructing Lunar World Scene in: {activeScene.name}");

            // 1. Setup Lighting (Harsh Lunar Sun & Space Sky)
            GameObject sunLightObj = GameObject.Find("Directional Light") ?? new GameObject("Directional Light");
            Light sunLight = sunLightObj.GetComponent<Light>() ?? sunLightObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.98f, 0.92f);
            sunLight.intensity = 1.35f;
            sunLight.shadows = LightShadows.Hard;
            sunLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.08f, 0.08f, 0.12f); // Deep space shadow ambient

            // 2. Setup Lunar Surface Ground
            GameObject moonGround = GameObject.Find("LunarSurface") ?? GameObject.CreatePrimitive(PrimitiveType.Plane);
            moonGround.name = "LunarSurface";
            moonGround.transform.position = Vector3.zero;
            moonGround.transform.localScale = new Vector3(20f, 1f, 20f); // 200m x 200m playfield
            Renderer groundRend = moonGround.GetComponent<Renderer>();
            if (groundRend != null)
            {
                Material moonMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                moonMat.color = new Color(0.35f, 0.35f, 0.38f);
                groundRend.material = moonMat;
            }

            // 3. Setup Core Systems Container
            GameObject coreSystems = GameObject.Find("CoreSystems") ?? new GameObject("CoreSystems");
            if (!coreSystems.GetComponent<GameManager>()) coreSystems.AddComponent<GameManager>();
            if (!coreSystems.GetComponent<AudioManager>()) coreSystems.AddComponent<AudioManager>();
            if (!coreSystems.GetComponent<SaveManager>()) coreSystems.AddComponent<SaveManager>();
            if (!coreSystems.GetComponent<ShopManager>()) coreSystems.AddComponent<ShopManager>();
            if (!coreSystems.GetComponent<HUDController>()) coreSystems.AddComponent<HUDController>();
            if (!coreSystems.GetComponent<ShopUIController>()) coreSystems.AddComponent<ShopUIController>();

            // 4. Setup Depot Hopper Base
            GameObject depotObj = GameObject.Find("DepotHopper") ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            depotObj.name = "DepotHopper";
            depotObj.tag = "Depot";
            depotObj.transform.position = new Vector3(0f, 0.75f, 0f);
            depotObj.transform.localScale = new Vector3(4f, 0.75f, 4f);

            Renderer depotRend = depotObj.GetComponent<Renderer>();
            if (depotRend != null)
            {
                Material depotMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                depotMat.color = new Color(0.9f, 0.8f, 0.2f); // Industrial gold/yellow
                depotRend.material = depotMat;
            }

            // Add Depot Beacon Light
            GameObject beaconObj = new GameObject("BeaconLight");
            beaconObj.transform.SetParent(depotObj.transform, false);
            beaconObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            Light beaconLight = beaconObj.AddComponent<Light>();
            beaconLight.type = LightType.Point;
            beaconLight.color = Color.cyan;
            beaconLight.range = 15f;
            beaconLight.intensity = 2f;

            if (!depotObj.GetComponent<DepotHopper>())
            {
                DepotHopper hopper = depotObj.AddComponent<DepotHopper>();
            }

            // 5. Setup Procedural Surface Generator
            GameObject surfaceGenObj = GameObject.Find("SurfaceGenerator") ?? new GameObject("SurfaceGenerator");
            LunarSurfaceGenerator gen = surfaceGenObj.GetComponent<LunarSurfaceGenerator>() ?? surfaceGenObj.AddComponent<LunarSurfaceGenerator>();

            // 6. Setup Astronaut Player
            GameObject playerObj = GameObject.Find("AstronautPlayer") ?? GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "AstronautPlayer";
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0f, 1.2f, -6f);
            playerObj.transform.localScale = new Vector3(1f, 1.8f, 1f);

            // Setup Character Controller
            CharacterController cc = playerObj.GetComponent<CharacterController>() ?? playerObj.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.45f;
            cc.stepOffset = 0.35f;

            // Setup Camera
            Camera cam = playerObj.GetComponentInChildren<Camera>();
            if (cam == null)
            {
                GameObject camObj = new GameObject("FirstPersonCamera");
                camObj.transform.SetParent(playerObj.transform, false);
                camObj.transform.localPosition = new Vector3(0f, 0.7f, 0.2f);
                cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            // Setup Tool Holder
            Transform toolHolder = playerObj.transform.Find("ToolHolder");
            if (toolHolder == null)
            {
                GameObject thObj = new GameObject("ToolHolder");
                thObj.transform.SetParent(cam.transform, false);
                thObj.transform.localPosition = new Vector3(0.2f, -0.2f, 0.4f);
                toolHolder = thObj.transform;
            }

            // Attach Player Scripts
            if (!playerObj.GetComponent<PlayerInventory>()) playerObj.AddComponent<PlayerInventory>();
            if (!playerObj.GetComponent<PlayerController>()) playerObj.AddComponent<PlayerController>();
            if (!playerObj.GetComponent<ToolController>()) playerObj.AddComponent<ToolController>();

            // Save Scene
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene, "Assets/Scenes/MainLunarBase.unity");

            Debug.Log("[11Hopem] Lunar World Scene successfully configured and saved to Assets/Scenes/MainLunarBase.unity!");
        }
    }
}
#endif
