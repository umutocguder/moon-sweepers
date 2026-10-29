/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

#if UNITY_EDITOR
using System.IO;
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
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EnsureShaderTemplatesAndGraphicsSettings();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject sunLightObj = new GameObject("Directional Light (Sun)");
            Light sunLight = sunLightObj.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.97f, 0.92f);
            sunLight.intensity = 1.45f;
            sunLight.shadows = LightShadows.Soft;
            sunLightObj.transform.rotation = Quaternion.Euler(26f, -36f, 0f);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.11f, 0.14f, 0.22f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.09f, 0.13f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.05f, 0.07f);

            GameObject coreSystems = new GameObject("CoreSystems");
            coreSystems.AddComponent<GameManager>();
            coreSystems.AddComponent<AudioManager>();
            coreSystems.AddComponent<SaveManager>();
            coreSystems.AddComponent<ShopManager>();
            coreSystems.AddComponent<HUDController>();
            coreSystems.AddComponent<ShopUIController>();

            GameObject depotObj = new GameObject("DepotHopper");
            depotObj.tag = "Depot";
            depotObj.transform.position = new Vector3(0f, 0f, 8.0f);
            depotObj.AddComponent<DepotHopper>();

            GameObject surfaceGenObj = new GameObject("SurfaceGenerator");
            surfaceGenObj.AddComponent<LunarSurfaceGenerator>();

            GameObject playerObj = new GameObject("AstronautPlayer");
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0f, 1.05f, -5.5f);
            playerObj.transform.rotation = Quaternion.identity;

            CharacterController cc = playerObj.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.42f;
            cc.stepOffset = 0.42f;
            cc.slopeLimit = 52f;

            GameObject camObj = new GameObject("FirstPersonCamera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(playerObj.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 0.72f, 0.06f);
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.006f, 0.009f, 0.018f, 1f);
            cam.nearClipPlane = 0.06f;
            cam.farClipPlane = 900f;
            cam.fieldOfView = 68f;
            camObj.AddComponent<AudioListener>();

            GameObject thObj = new GameObject("ToolHolder");
            thObj.transform.SetParent(camObj.transform, false);
            thObj.transform.localPosition = Vector3.zero;

            playerObj.AddComponent<PlayerInventory>();
            playerObj.AddComponent<PlayerController>();
            playerObj.AddComponent<ToolController>();

            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }

            string scenePath = "Assets/Scenes/MainLunarBase.unity";
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);

            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[11Hopem] Lunar World Scene generated and saved to Assets/Scenes/MainLunarBase.unity");
        }

        private static void EnsureShaderTemplatesAndGraphicsSettings()
        {
            string resMatDir = "Assets/Resources/Materials";
            if (!Directory.Exists(resMatDir))
            {
                Directory.CreateDirectory(resMatDir);
            }

            Shader std = Shader.Find("Standard");
            if (std != null)
            {
                Texture2D dummyBump = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        dummyBump.SetPixel(x, y, new Color(1f, 0.5f, 1f, 0.5f));
                    }
                }
                dummyBump.Apply();
                string bumpPath = "Assets/Resources/Materials/DefaultFlatNormal.png";
                File.WriteAllBytes(bumpPath, dummyBump.EncodeToPNG());
                AssetDatabase.ImportAsset(bumpPath, ImportAssetOptions.ForceSynchronousImport);

                Texture2D bumpAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(bumpPath);

                Material stdTemplate = new Material(std);
                stdTemplate.name = "TemplateStandard";
                stdTemplate.EnableKeyword("_NORMALMAP");
                stdTemplate.SetTexture("_BumpMap", bumpAsset);
                SaveOrReplaceMaterial(stdTemplate, $"{resMatDir}/TemplateStandard.mat");

                Texture2D dummyWhite = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        dummyWhite.SetPixel(x, y, Color.white);
                    }
                }
                dummyWhite.Apply();
                string whitePath = "Assets/Resources/Materials/DefaultWhite.png";
                File.WriteAllBytes(whitePath, dummyWhite.EncodeToPNG());
                AssetDatabase.ImportAsset(whitePath, ImportAssetOptions.ForceSynchronousImport);
                Texture2D whiteAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(whitePath);

                Material emiTemplate = new Material(std);
                emiTemplate.name = "TemplateEmissive";
                emiTemplate.EnableKeyword("_EMISSION");
                emiTemplate.SetColor("_EmissionColor", Color.cyan);
                emiTemplate.SetTexture("_EmissionMap", whiteAsset);
                SaveOrReplaceMaterial(emiTemplate, $"{resMatDir}/TemplateEmissive.mat");

                Material transTemplate = new Material(std);
                transTemplate.name = "TemplateTransparent";
                transTemplate.SetFloat("_Mode", 3f);
                transTemplate.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                transTemplate.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                transTemplate.SetInt("_ZWrite", 0);
                transTemplate.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                transTemplate.EnableKeyword("_EMISSION");
                transTemplate.SetColor("_EmissionColor", Color.cyan);
                transTemplate.renderQueue = 3000;
                SaveOrReplaceMaterial(transTemplate, $"{resMatDir}/TemplateTransparent.mat");
            }

            Shader partShader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            if (partShader != null)
            {
                Material partMat = new Material(partShader);
                partMat.name = "SoftParticle";
                partMat.SetFloat("_Mode", 2f);
                partMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                partMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                partMat.SetInt("_ZWrite", 0);
                partMat.EnableKeyword("_ALPHABLEND_ON");
                partMat.renderQueue = 3000;
                SaveOrReplaceMaterial(partMat, $"{resMatDir}/SoftParticle.mat");
            }

            Object[] gfxAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (gfxAssets != null && gfxAssets.Length > 0)
            {
                SerializedObject gfxSo = new SerializedObject(gfxAssets[0]);
                SerializedProperty alwaysShaders = gfxSo.FindProperty("m_AlwaysIncludedShaders");
                if (alwaysShaders != null)
                {
                    string[] requiredShaders = new string[]
                    {
                        "Standard",
                        "Particles/Standard Unlit",
                        "Sprites/Default",
                        "UI/Default",
                        "Unlit/Color"
                    };

                    foreach (string sName in requiredShaders)
                    {
                        Shader sh = Shader.Find(sName);
                        if (sh == null) continue;
                        bool exists = false;
                        for (int i = 0; i < alwaysShaders.arraySize; i++)
                        {
                            if (alwaysShaders.GetArrayElementAtIndex(i).objectReferenceValue == sh)
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists)
                        {
                            int idx = alwaysShaders.arraySize;
                            alwaysShaders.InsertArrayElementAtIndex(idx);
                            alwaysShaders.GetArrayElementAtIndex(idx).objectReferenceValue = sh;
                        }
                    }
                    gfxSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static void SaveOrReplaceMaterial(Material mat, string assetPath)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mat, existing);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(mat, assetPath);
            }
        }
    }
}
#endif
