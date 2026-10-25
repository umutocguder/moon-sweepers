using UnityEngine;

namespace ElevenHopem.MoonSweepers.Core
{
    /// <summary>
    /// Procedural audio synthesizer for 11Hopem Moon Sweepers.
    /// Synthesizes all sound effects via code math, ensuring 100% zero external audio file
    /// dependency and absolute compliance with Rule 11 (Zero Copyright Infringement).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource loopSource;

        private AudioClip brushSoundClip;
        private AudioClip coinSoundClip;
        private AudioClip thrusterSoundClip;
        private AudioClip upgradeSoundClip;
        private AudioClip vacuumLoopClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;

            GenerateProceduralAudioClips();
        }

        private void GenerateProceduralAudioClips()
        {
            brushSoundClip = CreateNoiseBurstClip("BrushScrape", 0.12f, 1200f);
            coinSoundClip = CreateChimeClip("CoinChime", 0.25f, 1046.5f, 1318.5f);
            thrusterSoundClip = CreateNoiseBurstClip("ThrusterRumble", 0.35f, 320f);
            upgradeSoundClip = CreateFanfareClip("UpgradeFanfare", 0.45f);
            vacuumLoopClip = CreateVacuumLoopClip("VacuumHum", 1.0f);
        }

        public void PlayBrushScrape()
        {
            if (brushSoundClip != null)
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.9f, 1.15f);
                sfxSource.PlayOneShot(brushSoundClip, 0.6f);
            }
        }

        public void PlayCoinChime()
        {
            if (coinSoundClip != null)
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.98f, 1.05f);
                sfxSource.PlayOneShot(coinSoundClip, 0.8f);
            }
        }

        public void PlayThruster()
        {
            if (thrusterSoundClip != null && !sfxSource.isPlaying)
            {
                sfxSource.PlayOneShot(thrusterSoundClip, 0.4f);
            }
        }

        public void PlayUpgradeFanfare()
        {
            if (upgradeSoundClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(upgradeSoundClip, 0.9f);
            }
        }

        public void SetVacuumActive(bool active)
        {
            if (active)
            {
                if (!loopSource.isPlaying)
                {
                    loopSource.clip = vacuumLoopClip;
                    loopSource.volume = 0.5f;
                    loopSource.Play();
                }
            }
            else
            {
                if (loopSource.isPlaying)
                {
                    loopSource.Stop();
                }
            }
        }

        private AudioClip CreateNoiseBurstClip(string name, float duration, float centerFreq)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // Linear fade out
                float noise = (UnityEngine.Random.value * 2f - 1f) * envelope;
                data[i] = noise * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateChimeClip(string name, float duration, float freq1, float freq2)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-7f * t); // Exponential decay
                float wave = Mathf.Sin(2f * Mathf.PI * freq1 * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * freq2 * t);
                data[i] = wave * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateFanfareClip(string name, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            float[] notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            float noteDuration = duration / notes.Length;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t / noteDuration), 0, notes.Length - 1);
                float noteFreq = notes[noteIndex];
                float noteT = t - (noteIndex * noteDuration);
                float env = Mathf.Exp(-4f * noteT);
                float wave = Mathf.Sin(2f * Mathf.PI * noteFreq * noteT);
                data[i] = wave * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateVacuumLoopClip(string name, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float motor = Mathf.Sin(2f * Mathf.PI * 130f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * 260f * t);
                float hiss = (UnityEngine.Random.value * 2f - 1f) * 0.2f;
                data[i] = (motor * 0.5f + hiss) * 0.3f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
