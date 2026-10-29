/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using UnityEngine;

namespace ElevenHopem.MoonSweepers.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource loopSource;
        private AudioSource thrusterLoopSource;
        private AudioSource ambienceSource;

        private AudioClip brushSoundClip;
        private AudioClip coinSoundClip;
        private AudioClip mineralSoundClip;
        private AudioClip upgradeSoundClip;
        private AudioClip jumpSoundClip;
        private AudioClip vacuumLoopClip;
        private AudioClip thrusterLoopClip;
        private AudioClip stationHumClip;

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
            sfxSource.spatialBlend = 0f;

            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;
            loopSource.spatialBlend = 0f;

            thrusterLoopSource = gameObject.AddComponent<AudioSource>();
            thrusterLoopSource.playOnAwake = false;
            thrusterLoopSource.loop = true;
            thrusterLoopSource.spatialBlend = 0f;

            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = false;
            ambienceSource.loop = true;
            ambienceSource.spatialBlend = 0f;
            ambienceSource.volume = 0.055f;

            GenerateProceduralAudioClips();
            ambienceSource.clip = stationHumClip;
            ambienceSource.Play();
        }

        private void GenerateProceduralAudioClips()
        {
            brushSoundClip = CreateFilteredSwishClip("BrushSwish", 0.095f);
            coinSoundClip = CreateChimeClip("CoinChime", 0.34f, 880f, 1318.5f, 1760f);
            mineralSoundClip = CreateChimeClip("MineralSparkle", 0.42f, 1174.6f, 1567.9f, 2349.3f);
            upgradeSoundClip = CreateFanfareClip("UpgradeFanfare", 0.52f);
            jumpSoundClip = CreateJumpWhooshClip("LunarJump", 0.18f);
            vacuumLoopClip = CreateVacuumLoopClip("VacuumTurbine", 1.2f);
            thrusterLoopClip = CreateThrusterLoopClip("ThrusterLoop", 1.0f);
            stationHumClip = CreateStationAmbienceClip("LunarAmbience", 2.0f);
        }

        public void PlayBrushScrape()
        {
            if (brushSoundClip != null)
            {
                sfxSource.pitch = Random.Range(0.92f, 1.16f);
                sfxSource.PlayOneShot(brushSoundClip, 0.42f);
            }
        }

        public void PlayCoinChime()
        {
            if (coinSoundClip != null)
            {
                sfxSource.pitch = Random.Range(0.98f, 1.04f);
                sfxSource.PlayOneShot(coinSoundClip, 0.72f);
            }
        }

        public void PlayMineralPickup()
        {
            if (mineralSoundClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(mineralSoundClip, 0.78f);
            }
        }

        public void PlayJump()
        {
            if (jumpSoundClip != null)
            {
                sfxSource.pitch = Random.Range(0.96f, 1.06f);
                sfxSource.PlayOneShot(jumpSoundClip, 0.35f);
            }
        }

        public void PlayThruster()
        {
            SetThrusterActive(true);
        }

        public void SetThrusterActive(bool active)
        {
            if (thrusterLoopSource == null || thrusterLoopClip == null) return;

            if (active)
            {
                if (!thrusterLoopSource.isPlaying)
                {
                    thrusterLoopSource.clip = thrusterLoopClip;
                    thrusterLoopSource.volume = 0.34f;
                    thrusterLoopSource.Play();
                }
            }
            else if (thrusterLoopSource.isPlaying)
            {
                thrusterLoopSource.Stop();
            }
        }

        public void PlayUpgradeFanfare()
        {
            if (upgradeSoundClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(upgradeSoundClip, 0.85f);
            }
        }

        public void SetVacuumActive(bool active, float pitch = 1.0f)
        {
            if (loopSource == null || vacuumLoopClip == null) return;

            if (active)
            {
                loopSource.pitch = pitch;
                if (!loopSource.isPlaying)
                {
                    loopSource.clip = vacuumLoopClip;
                    loopSource.volume = 0.38f;
                    loopSource.Play();
                }
            }
            else if (loopSource.isPlaying)
            {
                loopSource.Stop();
            }
        }

        private AudioClip CreateFilteredSwishClip(string clipName, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            float prev = 0f;
            float prev2 = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float env = Mathf.Sin(t * Mathf.PI) * (1f - t * 0.4f);
                float white = Random.value * 2f - 1f;
                prev = prev + 0.24f * (white - prev);
                prev2 = prev2 + 0.08f * (prev - prev2);
                float band = prev - prev2;
                data[i] = band * env * 0.55f;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateJumpWhooshClip(string clipName, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            float lp = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float norm = (float)i / samples;
                float env = Mathf.Sin(norm * Mathf.PI) * Mathf.Exp(-5f * t);
                float freq = Mathf.Lerp(110f, 240f, norm);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f;
                lp += 0.12f * ((Random.value * 2f - 1f) - lp);
                data[i] = (tone + lp * 0.5f) * env * 0.45f;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateChimeClip(string clipName, float duration, float f1, float f2, float f3)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env1 = Mathf.Exp(-8f * t);
                float env2 = t > 0.055f ? Mathf.Exp(-7f * (t - 0.055f)) : 0f;
                float env3 = t > 0.11f ? Mathf.Exp(-6f * (t - 0.11f)) : 0f;
                float wave = Mathf.Sin(2f * Mathf.PI * f1 * t) * env1 * 0.45f
                           + Mathf.Sin(2f * Mathf.PI * f2 * t) * env2 * 0.38f
                           + Mathf.Sin(2f * Mathf.PI * f3 * t) * env3 * 0.32f;
                data[i] = wave * 0.45f;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateFanfareClip(string clipName, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            float[] notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };
            float step = duration / notes.Length;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                int idx = Mathf.Clamp(Mathf.FloorToInt(t / step), 0, notes.Length - 1);
                float localT = t - idx * step;
                float env = Mathf.Exp(-4.5f * localT);
                float f = notes[idx];
                float wave = Mathf.Sin(2f * Mathf.PI * f * localT) + 0.35f * Mathf.Sin(4f * Mathf.PI * f * localT);
                data[i] = wave * env * 0.3f;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateVacuumLoopClip(string clipName, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            float lp = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float turbine = Mathf.Sin(2f * Mathf.PI * 145f * t) * 0.35f
                              + Mathf.Sin(2f * Mathf.PI * 290f * t) * 0.22f
                              + Mathf.Sin(2f * Mathf.PI * 580f * t) * 0.08f;
                lp += 0.14f * ((Random.value * 2f - 1f) - lp);
                data[i] = (turbine + lp * 0.45f) * 0.32f;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateThrusterLoopClip(string clipName, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            float lp = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                lp += 0.06f * ((Random.value * 2f - 1f) - lp);
                float rumble = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.35f + lp * 0.85f;
                data[i] = rumble * 0.42f;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip CreateStationAmbienceClip(string clipName, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float drone = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.5f
                            + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * 0.3f
                            + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.2f;
                data[i] = drone * 0.25f;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
