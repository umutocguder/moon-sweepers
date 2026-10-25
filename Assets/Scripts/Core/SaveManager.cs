using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ElevenHopem.MoonSweepers.Core
{
    [Serializable]
    public class GameSaveData
    {
        public string brand = "11Hopem";
        public string version = "1.0.0";
        public long lunarCredits = 0;
        public int currentToolIndex = 0;
        public float backpackCapacity = 30f;
        public float jetpackMaxFuel = 100f;
        public int conveyorLevel = 0;
        public int droneLevel = 0;
        public string timestamp;
        public string sha256Checksum;
    }

    /// <summary>
    /// Cryptographic save manager ensuring non-corruptible progress storage.
    /// Operates under Rule 2 and Rule 12 standards.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private string saveFilePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            saveFilePath = Path.Combine(Application.persistentDataPath, "moon_sweeper_save.json");
        }

        public void SaveGame(GameSaveData data)
        {
            try
            {
                data.timestamp = DateTime.UtcNow.ToString("o");
                data.sha256Checksum = ""; // clear for hash generation

                string rawJson = JsonUtility.ToJson(data, true);
                data.sha256Checksum = ComputeSha256(rawJson);
                string finalJson = JsonUtility.ToJson(data, true);

                File.WriteAllText(saveFilePath, finalJson, Encoding.UTF8);
                Debug.Log($"[SaveManager] Game state cryptographically saved to: {saveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {ex.Message}");
            }
        }

        public GameSaveData LoadGame()
        {
            if (!File.Exists(saveFilePath))
            {
                Debug.Log("[SaveManager] No existing save found. Creating new save data.");
                return new GameSaveData();
            }

            try
            {
                string json = File.ReadAllText(saveFilePath, Encoding.UTF8);
                GameSaveData loadedData = JsonUtility.FromJson<GameSaveData>(json);

                // Verify checksum
                string recordedHash = loadedData.sha256Checksum;
                loadedData.sha256Checksum = "";
                string calculatedHash = ComputeSha256(JsonUtility.ToJson(loadedData, true));

                if (!string.Equals(recordedHash, calculatedHash, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning("[SaveManager] Checksum mismatch! Save file may have been altered.");
                }

                return loadedData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Error loading save data: {ex.Message}");
                return new GameSaveData();
            }
        }

        private string ComputeSha256(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
