/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System.Collections.Generic;
using UnityEngine;

namespace ElevenHopem.MoonSweepers.Core
{
    public static class LunarMaterialLibrary
    {
        private static readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private static Texture2D regolithAlbedo;
        private static Texture2D regolithNormal;
        private static Texture2D dustAlbedo;
        private static Texture2D metalNormal;
        private static Texture2D hazardTex;
        private static Texture2D earthTex;
        private static Texture2D softParticleTex;

        private static Shader GetStandardShader()
        {
            Material template = Resources.Load<Material>("Materials/TemplateStandard");
            if (template != null && template.shader != null)
            {
                return template.shader;
            }
            return Shader.Find("Standard") ?? Shader.Find("Diffuse");
        }

        private static Material CreateBaseStandardMaterial()
        {
            Material template = Resources.Load<Material>("Materials/TemplateStandard");
            if (template != null)
            {
                return new Material(template);
            }
            return new Material(GetStandardShader());
        }

        public static Material GetRegolithTerrainMaterial()
        {
            if (materialCache.TryGetValue("RegolithTerrain", out Material cached) && cached != null)
            {
                return cached;
            }

            Material prebaked = Resources.Load<Material>("Materials/RegolithTerrain");
            if (prebaked != null)
            {
                Material inst = new Material(prebaked);
                materialCache["RegolithTerrain"] = inst;
                return inst;
            }

            EnsureRegolithTextures();
            Material mat = CreateBaseStandardMaterial();
            mat.name = "RegolithTerrainRuntime";
            mat.color = new Color(0.78f, 0.80f, 0.84f, 1f);
            mat.mainTexture = regolithAlbedo;
            mat.mainTextureScale = new Vector2(28f, 28f);
            mat.SetFloat("_Metallic", 0.04f);
            mat.SetFloat("_Glossiness", 0.11f);
            if (regolithNormal != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", regolithNormal);
                mat.SetTextureScale("_BumpMap", new Vector2(28f, 28f));
                mat.SetFloat("_BumpScale", 1.25f);
            }
            materialCache["RegolithTerrain"] = mat;
            return mat;
        }

        public static Material GetDustMoundMaterial()
        {
            if (materialCache.TryGetValue("DustMound", out Material cached) && cached != null)
            {
                return cached;
            }

            Material prebaked = Resources.Load<Material>("Materials/DustMound");
            if (prebaked != null)
            {
                Material inst = new Material(prebaked);
                materialCache["DustMound"] = inst;
                return inst;
            }

            EnsureDustTexture();
            EnsureRegolithTextures();
            Material mat = CreateBaseStandardMaterial();
            mat.name = "DustMoundRuntime";
            mat.color = new Color(0.28f, 0.25f, 0.23f, 1f);
            mat.mainTexture = dustAlbedo;
            mat.mainTextureScale = new Vector2(3f, 3f);
            mat.SetFloat("_Metallic", 0.03f);
            mat.SetFloat("_Glossiness", 0.07f);
            if (regolithNormal != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", regolithNormal);
                mat.SetTextureScale("_BumpMap", new Vector2(4f, 4f));
                mat.SetFloat("_BumpScale", 1.35f);
            }
            materialCache["DustMound"] = mat;
            return mat;
        }

        public static Material GetBoulderMaterial()
        {
            if (materialCache.TryGetValue("Boulder", out Material cached) && cached != null)
            {
                return cached;
            }

            Material prebaked = Resources.Load<Material>("Materials/BoulderRock");
            if (prebaked != null)
            {
                Material inst = new Material(prebaked);
                materialCache["Boulder"] = inst;
                return inst;
            }

            EnsureRegolithTextures();
            Material mat = CreateBaseStandardMaterial();
            mat.name = "BoulderRuntime";
            mat.color = new Color(0.46f, 0.47f, 0.50f, 1f);
            mat.mainTexture = regolithAlbedo;
            mat.mainTextureScale = new Vector2(2f, 2f);
            mat.SetFloat("_Metallic", 0.08f);
            mat.SetFloat("_Glossiness", 0.16f);
            if (regolithNormal != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", regolithNormal);
                mat.SetTextureScale("_BumpMap", new Vector2(2f, 2f));
                mat.SetFloat("_BumpScale", 1.25f);
            }
            materialCache["Boulder"] = mat;
            return mat;
        }

        public static Material GetSurfaceMaterial(string key, Color color, float metallic = 0.25f, float smoothness = 0.55f, bool panelNormal = false)
        {
            if (materialCache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Material mat = CreateBaseStandardMaterial();
            mat.name = key;
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Glossiness", smoothness);

            if (panelNormal)
            {
                EnsureMetalNormal();
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", metalNormal);
                mat.SetTextureScale("_BumpMap", new Vector2(2f, 2f));
                mat.SetFloat("_BumpScale", 0.65f);
            }

            materialCache[key] = mat;
            return mat;
        }

        public static Material GetHazardMaterial()
        {
            if (materialCache.TryGetValue("HazardStripe", out Material cached) && cached != null)
            {
                return cached;
            }

            EnsureHazardTexture();
            Material mat = CreateBaseStandardMaterial();
            mat.name = "HazardStripe";
            mat.color = Color.white;
            mat.mainTexture = hazardTex;
            mat.mainTextureScale = new Vector2(6f, 1f);
            mat.SetFloat("_Metallic", 0.35f);
            mat.SetFloat("_Glossiness", 0.55f);
            materialCache["HazardStripe"] = mat;
            return mat;
        }

        public static Material GetEmissiveMaterial(string key, Color baseColor, Color emissionColor)
        {
            if (materialCache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Material template = Resources.Load<Material>("Materials/TemplateEmissive");
            Material mat = template != null ? new Material(template) : CreateBaseStandardMaterial();
            mat.name = key;
            mat.color = baseColor;
            mat.SetFloat("_Metallic", 0.15f);
            mat.SetFloat("_Glossiness", 0.75f);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", emissionColor);
            materialCache[key] = mat;
            return mat;
        }

        public static Material GetTransparentMaterial(string key, Color color, Color emissionColor)
        {
            if (materialCache.TryGetValue(key, out Material cached) && cached != null)
            {
                return cached;
            }

            Material template = Resources.Load<Material>("Materials/TemplateTransparent");
            Material mat = template != null ? new Material(template) : CreateBaseStandardMaterial();
            mat.name = key;
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.color = color;
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Glossiness", 0.88f);
            if (emissionColor.maxColorComponent > 0.001f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor);
            }
            materialCache[key] = mat;
            return mat;
        }

        public static Material GetEarthMaterial()
        {
            if (materialCache.TryGetValue("PlanetEarth", out Material cached) && cached != null)
            {
                return cached;
            }

            EnsureEarthTexture();
            Material mat = GetEmissiveMaterial("PlanetEarthMat", Color.white, new Color(0.55f, 0.65f, 0.85f));
            mat.mainTexture = earthTex;
            mat.SetTexture("_EmissionMap", earthTex);
            mat.SetFloat("_Metallic", 0.0f);
            mat.SetFloat("_Glossiness", 0.35f);
            materialCache["PlanetEarth"] = mat;
            return mat;
        }

        public static Material GetParticleMaterial()
        {
            if (materialCache.TryGetValue("SoftParticle", out Material cached) && cached != null)
            {
                return cached;
            }

            EnsureParticleTexture();
            Material template = Resources.Load<Material>("Materials/SoftParticle");
            Shader pShader = template != null ? template.shader : (Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default"));
            Material mat = template != null ? new Material(template) : new Material(pShader);
            mat.mainTexture = softParticleTex;
            materialCache["SoftParticle"] = mat;
            return mat;
        }

        public static float Fbm(float x, float y, int octaves)
        {
            float sum = 0f;
            float amp = 0.5f;
            float freq = 1f;
            for (int i = 0; i < octaves; i++)
            {
                sum += amp * Mathf.PerlinNoise(x * freq, y * freq);
                freq *= 2.03f;
                amp *= 0.5f;
            }
            return sum;
        }

        private static void EnsureRegolithTextures()
        {
            if (regolithAlbedo != null && regolithNormal != null) return;

            int size = 256;
            float[,] heights = new float[size, size];
            regolithAlbedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            regolithAlbedo.wrapMode = TextureWrapMode.Repeat;
            regolithAlbedo.filterMode = FilterMode.Trilinear;
            regolithAlbedo.anisoLevel = 8;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    float n1 = TileableNoise(u, v, 6f);
                    float n2 = TileableNoise(u, v, 18f);
                    float n3 = TileableNoise(u, v, 48f);
                    float microPits = Mathf.Pow(TileableNoise(u + 0.31f, v + 0.73f, 28f), 3f) * 0.35f;
                    float h = n1 * 0.52f + n2 * 0.30f + n3 * 0.18f - microPits;
                    heights[x, y] = h;

                    float shade = Mathf.Clamp01(0.56f + h * 0.36f);
                    Color c = new Color(shade * 0.98f, shade * 0.99f, shade * 1.03f, 1f);
                    regolithAlbedo.SetPixel(x, y, c);
                }
            }
            regolithAlbedo.Apply(true);

            regolithNormal = new Texture2D(size, size, TextureFormat.RGBA32, true, true);
            regolithNormal.wrapMode = TextureWrapMode.Repeat;
            regolithNormal.filterMode = FilterMode.Trilinear;
            regolithNormal.anisoLevel = 8;

            float strength = 3.2f;
            for (int y = 0; y < size; y++)
            {
                int ym = (y - 1 + size) % size;
                int yp = (y + 1) % size;
                for (int x = 0; x < size; x++)
                {
                    int xm = (x - 1 + size) % size;
                    int xp = (x + 1) % size;
                    float dx = (heights[xp, y] - heights[xm, y]) * strength;
                    float dy = (heights[x, yp] - heights[x, ym]) * strength;
                    Vector3 norm = new Vector3(-dx, -dy, 1f).normalized;
                    float nx = norm.x * 0.5f + 0.5f;
                    float ny = norm.y * 0.5f + 0.5f;
                    regolithNormal.SetPixel(x, y, new Color(1f, ny, 1f, nx));
                }
            }
            regolithNormal.Apply(true);
        }

        private static float TileableNoise(float u, float v, float scale)
        {
            float sx = u * scale;
            float sy = v * scale;
            float n00 = Mathf.PerlinNoise(sx, sy);
            float n10 = Mathf.PerlinNoise(sx - scale, sy);
            float n01 = Mathf.PerlinNoise(sx, sy - scale);
            float n11 = Mathf.PerlinNoise(sx - scale, sy - scale);
            float nx0 = Mathf.Lerp(n00, n10, u);
            float nx1 = Mathf.Lerp(n01, n11, u);
            return Mathf.Lerp(nx0, nx1, v);
        }

        private static void EnsureDustTexture()
        {
            if (dustAlbedo != null) return;
            int size = 128;
            dustAlbedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            dustAlbedo.wrapMode = TextureWrapMode.Repeat;
            dustAlbedo.filterMode = FilterMode.Trilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    float grain = TileableNoise(u + 1.7f, v + 2.3f, 24f) * 0.6f + TileableNoise(u, v, 64f) * 0.4f;
                    float val = Mathf.Lerp(0.55f, 1.0f, grain);
                    dustAlbedo.SetPixel(x, y, new Color(val * 0.95f, val * 0.96f, val * 1.05f, 1f));
                }
            }
            dustAlbedo.Apply(true);
        }

        private static void EnsureMetalNormal()
        {
            if (metalNormal != null) return;
            int size = 128;
            metalNormal = new Texture2D(size, size, TextureFormat.RGBA32, true, true);
            metalNormal.wrapMode = TextureWrapMode.Repeat;
            metalNormal.filterMode = FilterMode.Trilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool seamX = (x % 32 == 0) || (x % 32 == 1);
                    bool seamY = (y % 32 == 0) || (y % 32 == 1);
                    float nx = seamX ? ((x % 32 == 0) ? 0.3f : 0.7f) : 0.5f;
                    float ny = seamY ? ((y % 32 == 0) ? 0.3f : 0.7f) : 0.5f;
                    metalNormal.SetPixel(x, y, new Color(1f, ny, 1f, nx));
                }
            }
            metalNormal.Apply(true);
        }

        private static void EnsureHazardTexture()
        {
            if (hazardTex != null) return;
            int size = 128;
            hazardTex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            hazardTex.wrapMode = TextureWrapMode.Repeat;
            hazardTex.filterMode = FilterMode.Bilinear;

            Color yellow = new Color(0.95f, 0.72f, 0.10f, 1f);
            Color dark = new Color(0.12f, 0.13f, 0.15f, 1f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int stripe = ((x + y) / 16) % 2;
                    hazardTex.SetPixel(x, y, stripe == 0 ? yellow : dark);
                }
            }
            hazardTex.Apply(true);
        }

        private static void EnsureEarthTexture()
        {
            if (earthTex != null) return;
            int w = 256;
            int h = 128;
            earthTex = new Texture2D(w, h, TextureFormat.RGBA32, true);
            earthTex.wrapMode = TextureWrapMode.Repeat;
            earthTex.filterMode = FilterMode.Bilinear;

            Color deepOcean = new Color(0.05f, 0.22f, 0.58f, 1f);
            Color shallowOcean = new Color(0.10f, 0.45f, 0.80f, 1f);
            Color lushLand = new Color(0.18f, 0.48f, 0.24f, 1f);
            Color aridLand = new Color(0.68f, 0.58f, 0.36f, 1f);

            for (int y = 0; y < h; y++)
            {
                float lat = Mathf.Abs(((float)y / h) * 2f - 1f);
                for (int x = 0; x < w; x++)
                {
                    float u = (float)x / w;
                    float v = (float)y / h;
                    float cont = Fbm(u * 4.2f + 3.1f, v * 2.6f + 1.4f, 4);
                    float climate = Fbm(u * 7.5f + 9.2f, v * 5.0f + 4.1f, 3);
                    float clouds = Fbm(u * 6.5f + 15.3f + Mathf.Sin(v * 8f) * 0.2f, v * 4.5f + 7.7f, 4);

                    Color col;
                    if (lat > 0.84f - cont * 0.1f)
                    {
                        col = new Color(0.92f, 0.95f, 0.99f, 1f);
                    }
                    else if (cont > 0.51f)
                    {
                        col = Color.Lerp(lushLand, aridLand, Mathf.Clamp01((climate - 0.35f) * 2.2f));
                    }
                    else
                    {
                        col = Color.Lerp(deepOcean, shallowOcean, Mathf.Clamp01(cont / 0.51f));
                    }

                    float cloudAlpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.49f, 0.72f, clouds));
                    col = Color.Lerp(col, Color.white, cloudAlpha * 0.88f);
                    earthTex.SetPixel(x, y, col);
                }
            }
            earthTex.Apply(true);
        }

        private static void EnsureParticleTexture()
        {
            if (softParticleTex != null) return;
            int size = 64;
            softParticleTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            softParticleTex.wrapMode = TextureWrapMode.Clamp;
            softParticleTex.filterMode = FilterMode.Bilinear;

            float center = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Pow(Mathf.Clamp01(1f - dist), 2.1f);
                    softParticleTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            softParticleTex.Apply(false);
        }

        public static Sprite GetRoundedSprite(string key, int size, int cornerRadius, int borderWidth, Color fill, Color border)
        {
            if (spriteCache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float half = size * 0.5f;
            float innerHalf = half - cornerRadius - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float qx = Mathf.Abs(x + 0.5f - half) - innerHalf;
                    float qy = Mathf.Abs(y + 0.5f - half) - innerHalf;
                    float dist = Vector2.Max(new Vector2(qx, qy), Vector2.zero).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0f) - cornerRadius;

                    float outerAlpha = Mathf.Clamp01(0.5f - dist);
                    if (outerAlpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    Color pixel = fill;
                    if (borderWidth > 0)
                    {
                        float borderBlend = Mathf.Clamp01(dist + borderWidth + 0.5f);
                        pixel = Color.Lerp(fill, border, borderBlend);
                    }
                    pixel.a *= outerAlpha;
                    tex.SetPixel(x, y, pixel);
                }
            }
            tex.Apply(false);

            int slice = Mathf.Max(cornerRadius + borderWidth + 2, 4);
            Sprite sp = Sprite.Create(
                tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(slice, slice, slice, slice)
            );
            spriteCache[key] = sp;
            return sp;
        }

        public static Mesh CreateDustMoundMesh(int seed)
        {
            int radial = 22;
            int rings = 8;
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float offsetX = (seed * 17.3f) % 100f;
            float offsetZ = (seed * 31.7f) % 100f;

            verts.Add(new Vector3(0f, 0.34f + Mathf.PerlinNoise(offsetX, offsetZ) * 0.08f, 0f));
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int r = 1; r <= rings; r++)
            {
                float t = (float)r / rings;
                for (int s = 0; s < radial; s++)
                {
                    float ang = (s / (float)radial) * Mathf.PI * 2f;
                    float dirX = Mathf.Cos(ang);
                    float dirZ = Mathf.Sin(ang);
                    float lump = 0.82f + 0.34f * Mathf.PerlinNoise(offsetX + dirX * 1.6f * t + 2f, offsetZ + dirZ * 1.6f * t + 2f);
                    float radius = t * lump;
                    float falloff = Mathf.Pow(Mathf.Clamp01(1f - t * t), 1.65f);
                    float y = (r == rings) ? -0.008f : (falloff * 0.34f * (0.88f + 0.24f * lump));

                    verts.Add(new Vector3(dirX * radius, y, dirZ * radius));
                    uvs.Add(new Vector2(0.5f + dirX * t * 0.5f, 0.5f + dirZ * t * 0.5f));
                }
            }

            for (int s = 0; s < radial; s++)
            {
                int next = (s + 1) % radial;
                tris.Add(0);
                tris.Add(1 + next);
                tris.Add(1 + s);
            }

            for (int r = 0; r < rings - 1; r++)
            {
                int rowA = 1 + r * radial;
                int rowB = 1 + (r + 1) * radial;
                for (int s = 0; s < radial; s++)
                {
                    int next = (s + 1) % radial;
                    tris.Add(rowA + s);
                    tris.Add(rowA + next);
                    tris.Add(rowB + s);

                    tris.Add(rowA + next);
                    tris.Add(rowB + next);
                    tris.Add(rowB + s);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = $"DustMound_{seed}";
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateBoulderMesh(int seed)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh src = temp.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] baseVerts = src.vertices;
            Vector2[] baseUvs = src.uv;
            int[] baseTris = src.triangles;
            Object.DestroyImmediate(temp);

            float ox = (seed * 13.7f) % 80f;
            float oy = (seed * 29.1f) % 80f;
            float oz = (seed * 43.9f) % 80f;

            Vector3[] displaced = new Vector3[baseVerts.Length];
            for (int i = 0; i < baseVerts.Length; i++)
            {
                Vector3 v = baseVerts[i];
                float n1 = Mathf.PerlinNoise(ox + v.x * 2.1f + 3f, oy + v.y * 2.1f + v.z * 1.7f + 3f);
                float n2 = Mathf.PerlinNoise(oz + v.z * 4.5f + 5f, ox + v.x * 4.5f + 5f);
                float scale = 0.72f + n1 * 0.48f + (n2 - 0.5f) * 0.16f;
                Vector3 p = v * scale;
                if (p.y < -0.18f) p.y *= 0.55f;
                displaced[i] = p;
            }

            Mesh rock = new Mesh();
            rock.name = $"Boulder_{seed}";
            rock.vertices = displaced;
            rock.uv = baseUvs;
            rock.triangles = baseTris;
            rock.RecalculateNormals();
            rock.RecalculateTangents();
            rock.RecalculateBounds();
            return rock;
        }
    }
}
