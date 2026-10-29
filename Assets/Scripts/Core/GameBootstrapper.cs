/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using UnityEngine;
using ElevenHopem.MoonSweepers.Player;
using ElevenHopem.MoonSweepers.Tools;
using ElevenHopem.MoonSweepers.Environment;
using ElevenHopem.MoonSweepers.Economy;
using ElevenHopem.MoonSweepers.UI;

namespace ElevenHopem.MoonSweepers.Core
{
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureWorldInitialized()
        {
            if (Object.FindFirstObjectByType<GameManager>() == null)
            {
                GameObject core = new GameObject("CoreSystems");
                core.AddComponent<GameManager>();
                core.AddComponent<AudioManager>();
                core.AddComponent<SaveManager>();
                core.AddComponent<ShopManager>();
                core.AddComponent<HUDController>();
                core.AddComponent<ShopUIController>();
            }

            if (Object.FindFirstObjectByType<Light>() == null)
            {
                GameObject sunObj = new GameObject("Directional Light (Sun)");
                sunObj.transform.rotation = Quaternion.Euler(26f, -36f, 0f);
                Light sun = sunObj.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.45f;
                sun.shadows = LightShadows.Soft;
            }

            if (Object.FindFirstObjectByType<DepotHopper>() == null)
            {
                GameObject depot = new GameObject("DepotHopper");
                depot.tag = "Depot";
                depot.transform.position = new Vector3(0f, 0f, 8.0f);
                depot.AddComponent<DepotHopper>();
            }

            if (Object.FindFirstObjectByType<LunarSurfaceGenerator>() == null)
            {
                GameObject gen = new GameObject("SurfaceGenerator");
                gen.AddComponent<LunarSurfaceGenerator>();
            }

            if (Object.FindFirstObjectByType<PlayerController>() == null)
            {
                GameObject player = new GameObject("AstronautPlayer");
                player.tag = "Player";
                player.transform.position = new Vector3(0f, 1.05f, -5.5f);

                CharacterController cc = player.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.42f;
                cc.stepOffset = 0.42f;
                cc.slopeLimit = 52f;

                GameObject camObj = new GameObject("FirstPersonCamera");
                camObj.tag = "MainCamera";
                camObj.transform.SetParent(player.transform, false);
                camObj.transform.localPosition = new Vector3(0f, 0.72f, 0.06f);
                camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();

                GameObject th = new GameObject("ToolHolder");
                th.transform.SetParent(camObj.transform, false);

                player.AddComponent<PlayerInventory>();
                player.AddComponent<PlayerController>();
                player.AddComponent<ToolController>();
            }
        }
    }
}
