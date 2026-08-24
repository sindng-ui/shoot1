using System;
using UnityEngine;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.Audio
{
    /// <summary>
    /// Generates nostalgic 8-bit / arcade sound effects and chiptune BGM loops procedurally using mathematical waveforms.
    /// Eliminates external asset dependencies and loads instantly.
    /// </summary>
    public static class ProceduralAudioHelper
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateSoundEffect(SoundEffectType type)
        {
            switch (type)
            {
                case SoundEffectType.SlashAttack:
                    return GenerateNoiseSweep(0.12f, 800f, 200f);
                case SoundEffectType.BowShoot:
                    return GenerateBowArrowWhoosh(0.22f);
                case SoundEffectType.MagicExplosion:
                    return GenerateExplosion(0.35f);
                case SoundEffectType.Fireball:
                    return GenerateFireball(0.28f);
                case SoundEffectType.BladeOrbit:
                    return GenerateMetallicPing(0.15f, 1200f);
                case SoundEffectType.MonsterHit:
                    return GeneratePunchHit(0.08f);
                case SoundEffectType.MonsterDeath:
                    return GeneratePitchDrop(0.20f, 400f, 80f);
                case SoundEffectType.GemCollect:
                    return GenerateChime(0.12f, 1500f, 2000f);
                case SoundEffectType.LevelUp:
                    return GenerateFanfare(0.5f, new[] { 440f, 554f, 659f, 880f });
                case SoundEffectType.WeaponEvolve:
                    return GenerateArpeggio(0.7f, new[] { 523f, 659f, 784f, 1046f, 1318f });
                case SoundEffectType.BossSpawn:
                    return GenerateSiren(0.8f, 180f, 320f);
                case SoundEffectType.ChestOpen:
                    return GenerateArpeggio(0.6f, new[] { 440f, 554f, 659f, 880f, 1108f });
                case SoundEffectType.PlayerHurt:
                    return GeneratePunchHit(0.15f);
                case SoundEffectType.GameOver:
                    return GenerateFanfare(0.8f, new[] { 440f, 415f, 392f, 330f });
                case SoundEffectType.Victory:
                    return GenerateFanfare(0.8f, new[] { 523f, 659f, 784f, 1046f });
                default:
                    return GeneratePunchHit(0.08f);
            }
        }

        public static AudioClip CreateRetroBgmTrack()
        {
            float duration = 8.0f; // 8-second loop
            int sampleCount = (int)(SampleRate * duration);
            float[] samples = new float[sampleCount];

            // Bass note frequencies (Am - F - C - G chord progression)
            float[] bassNotes = { 110f, 87.31f, 130.81f, 98f };
            float noteDuration = 2.0f; // 2 seconds per bar

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                int bar = (int)(t / noteDuration) % 4;
                float bassFreq = bassNotes[bar];

                // 16th note rhythm pulse (8 Hz)
                float pulse = Mathf.Sin(t * Mathf.PI * 16f) > 0 ? 1f : 0.2f;

                // Bass square wave + harmonic
                float bassSample = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * bassFreq * t)) * 0.15f * pulse;

                // Fast arpeggiated lead (16th notes)
                int arpIndex = (int)(t * 8f) % 4;
                float[] arpFreqs = { bassFreq * 2f, bassFreq * 2.5f, bassFreq * 3f, bassFreq * 4f };
                float leadSample = (Mathf.PingPong(t * arpFreqs[arpIndex] * 2f, 1f) - 0.5f) * 0.10f;

                samples[i] = Mathf.Clamp(bassSample + leadSample, -1f, 1f);
            }

            var clip = AudioClip.Create("Procedural_RetroBgm", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GeneratePunchHit(float duration = 0.085f)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;

                // Punchy Attack (0.008s) + Rich Juicy Sustain + Smooth Decay
                float env = Mathf.Sin(Mathf.Clamp01(t * 6.0f) * Mathf.PI * 0.5f) * Mathf.Pow(1f - t, 1.2f);

                // 1. Ultra-Juicy Slap Snap / Pop Crack (1450Hz down to 320Hz)
                float snapEnv = Mathf.Exp(-t * 32f);
                float snapFreq = Mathf.Lerp(1450f, 320f, t * 5f);
                float snap = Mathf.Sin(2f * Mathf.PI * snapFreq * (i / (float)SampleRate)) * snapEnv;
                float snapHarmonic = Mathf.Sin(4f * Mathf.PI * snapFreq * (i / (float)SampleRate)) * snapEnv * 0.40f;

                // 2. Solid Meat Thud Body (240Hz down to 60Hz)
                float thudFreq = Mathf.Lerp(240f, 60f, t);
                float thud = Mathf.Sin(2f * Mathf.PI * thudFreq * (i / (float)SampleRate));

                // 3. Crispy Flesh Texture Noise (0.025s initial crunch)
                float noiseEnv = Mathf.Exp(-t * 26f);
                float noise = (UnityEngine.Random.value * 2f - 1f) * noiseEnv * 0.45f;

                // Master saturated mix with maximum punch & juicy bite
                float mixed = (snap * 0.50f + snapHarmonic + thud * 0.45f + noise) * env * 1.35f;
                samples[i] = Mathf.Clamp(mixed, -1f, 1f);
            }

            var clip = AudioClip.Create("Hit", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateNoiseSweep(float duration, float startFreq, float endFreq)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Sin(t * Mathf.PI);
                float noise = UnityEngine.Random.value * 2f - 1f;
                float tone = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(startFreq, endFreq, t) * (i / (float)SampleRate));
                samples[i] = (noise * 0.6f + tone * 0.4f) * env * 0.6f;
            }
            var clip = AudioClip.Create("Slash", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GeneratePitchDrop(float duration, float startFreq, float endFreq)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = 1f - t;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate)) * env * 0.6f;
            }
            var clip = AudioClip.Create("PitchDrop", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateExplosion(float duration)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Pow(1f - t, 2f);
                float noise = UnityEngine.Random.value * 2f - 1f;
                float rumble = Mathf.Sin(2f * Mathf.PI * 65f * (i / (float)SampleRate));
                samples[i] = (noise * 0.7f + rumble * 0.3f) * env * 0.8f;
            }
            var clip = AudioClip.Create("Explosion", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateChime(float duration, float freq1, float freq2)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = 1f - t;
                float s1 = Mathf.Sin(2f * Mathf.PI * freq1 * (i / (float)SampleRate));
                float s2 = Mathf.Sin(2f * Mathf.PI * freq2 * (i / (float)SampleRate));
                samples[i] = (s1 * 0.5f + s2 * 0.5f) * env * 0.5f;
            }
            var clip = AudioClip.Create("Chime", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateMetallicPing(float duration, float freq)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Exp(-t * 8f);
                float s = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate));
                samples[i] = s * env * 0.4f;
            }
            var clip = AudioClip.Create("Ping", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateFanfare(float duration, float[] notes)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            float noteDur = duration / notes.Length;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                int noteIndex = Mathf.Min(notes.Length - 1, (int)(t / noteDur));
                float noteLocalT = (t % noteDur) / noteDur;
                float env = 1f - noteLocalT * 0.3f;
                float freq = notes[noteIndex];
                float s = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.3f;
                samples[i] = s * env * 0.6f;
            }
            var clip = AudioClip.Create("Fanfare", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateArpeggio(float duration, float[] notes)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            float noteDur = duration / notes.Length;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SampleRate;
                int noteIdx = Mathf.Min(notes.Length - 1, (int)(t / noteDur));
                float freq = notes[noteIdx];
                float s = (Mathf.PingPong(t * freq * 2f, 1f) - 0.5f) * 0.5f;
                samples[i] = s * (1f - (float)i / count * 0.5f);
            }
            var clip = AudioClip.Create("Arpeggio", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateSiren(float duration, float minFreq, float maxFreq)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float freq = Mathf.PingPong(t * 8f, 1f) * (maxFreq - minFreq) + minFreq;
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate)) * 0.6f;
            }
            var clip = AudioClip.Create("Siren", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateFireball(float duration)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Sin(Mathf.Clamp01(t * 2.0f) * Mathf.PI * 0.5f) * Mathf.Pow(1f - t, 1.4f);

                // 1. Fiery Whoosh Pitch Sweep (520Hz down to 100Hz)
                float sweepFreq = Mathf.Lerp(520f, 100f, t);
                float whoosh = Mathf.Sin(2f * Mathf.PI * sweepFreq * (i / (float)SampleRate));

                // 2. Combustion Low Rumble (85Hz down to 40Hz)
                float rumbleFreq = Mathf.Lerp(85f, 40f, t);
                float rumble = Mathf.Sin(2f * Mathf.PI * rumbleFreq * (i / (float)SampleRate));

                // 3. Fiery Flame Noise & Flame Crackle
                float noise = UnityEngine.Random.value * 2f - 1f;
                float crackle = (UnityEngine.Random.value > 0.88f) ? (UnityEngine.Random.value * 1.5f - 0.75f) : 0f;

                samples[i] = (whoosh * 0.35f + rumble * 0.35f + noise * 0.20f + crackle * 0.10f) * env * 0.85f;
            }
            var clip = AudioClip.Create("Fireball", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip GenerateBowArrowWhoosh(float duration = 0.24f)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count; // 0.0 to 1.0

                // Fast punchy attack (0.01s), strong sustain, crisp tail
                float env = Mathf.Sin(Mathf.Clamp01(t * 4.0f) * Mathf.PI * 0.5f) * Mathf.Pow(1f - t, 1.1f);

                // 1. Bowstring Twang Snap (Crisp elastic release tone)
                float twangEnv = Mathf.Exp(-t * 22f);
                float twangFreq = Mathf.Lerp(850f, 320f, t * 4f);
                float twang = Mathf.Sin(2f * Mathf.PI * twangFreq * (i / (float)SampleRate)) * twangEnv;

                // 2. High-speed Piercing Wind Whistle ("휘이잉~" 1650Hz down to 260Hz)
                float whistleFreq = 260f + 1390f * Mathf.Pow(1f - t, 1.6f);
                float whistle = Mathf.Sin(2f * Mathf.PI * whistleFreq * (i / (float)SampleRate));
                float harmonic = Mathf.Sin(4f * Mathf.PI * whistleFreq * (i / (float)SampleRate)) * 0.40f;

                // 3. Fletching Flutter (24Hz aerodynamic vibration)
                float flutter = 1.0f + 0.35f * Mathf.Sin(2f * Mathf.PI * 24f * (i / (float)SampleRate));

                // 4. White/Pink Wind Turbulence Noise
                float noise = (UnityEngine.Random.value * 2f - 1f) * (0.9f - t * 0.6f);

                // Full mix with punchy amplification
                float mixed = (twang * 0.55f + (whistle + harmonic) * flutter * 0.60f + noise * 0.25f) * env;
                samples[i] = Mathf.Clamp(mixed * 1.15f, -1f, 1f);
            }

            var clip = AudioClip.Create("BowArrowWhoosh", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
