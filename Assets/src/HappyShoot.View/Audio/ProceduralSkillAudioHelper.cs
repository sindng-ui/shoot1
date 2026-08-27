using System;
using UnityEngine;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.Audio
{
    /// <summary>
    /// Procedurally synthesizes distinctive, juicy sound effects for specific weapon hits and elemental dots:
    /// - Piercing Arrow / Storm Bow: Sharp piercing thwip snap
    /// - Wind Glaive: Razor whirlwind metallic slice
    /// - Stellar Rain: Ethereal celestial crystal chime drop
    /// - Inferno Fireball: Fiery magma explosion boom
    /// - Burn Dot: Sizzling flame crackle ember
    /// - Shock Dot: Zapping electric plasma spark
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class ProceduralSkillAudioHelper
    {
        private const int SampleRate = 44100;

        public static AudioClip GenerateArrowPierceHit(float duration = 0.055f)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                // Fast sharp transient envelope
                float env = Mathf.Exp(-t * 28f);

                // 1. Sharp arrowhead piercing snap (1150Hz -> 380Hz)
                float freq = Mathf.Lerp(1150f, 380f, t * 4f);
                float snap = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)SampleRate));

                // 2. High-speed projectile whistle / whip hiss
                float whistle = Mathf.Sin(2f * Mathf.PI * 2400f * (i / (float)SampleRate)) * 0.25f;

                // 3. Crisp impact click
                float click = (UnityEngine.Random.value * 2f - 1f) * Mathf.Exp(-t * 45f) * 0.40f;

                samples[i] = Mathf.Clamp((snap * 0.65f + whistle + click) * env * 1.30f, -1f, 1f);
            }

            var clip = AudioClip.Create("SFX_ArrowHit", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateWindGlaiveHit(float duration = 0.065f)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Exp(-t * 22f);

                // Spinning razor blade resonance (FM modulation)
                float mod = Mathf.Sin(2f * Mathf.PI * 180f * (i / (float)SampleRate)) * 120f;
                float carrierFreq = Mathf.Lerp(980f, 260f, t) + mod;
                float blade = Mathf.Sin(2f * Mathf.PI * carrierFreq * (i / (float)SampleRate));

                // Wind whoosh noise
                float windNoise = (UnityEngine.Random.value * 2f - 1f) * Mathf.Exp(-t * 18f) * 0.35f;

                samples[i] = Mathf.Clamp((blade * 0.70f + windNoise) * env * 1.25f, -1f, 1f);
            }

            var clip = AudioClip.Create("SFX_WindGlaiveHit", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateStellarRainHit(float duration = 0.075f)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Exp(-t * 16f);

                // Sparkling celestial crystal bells (harmonic partials: 1480Hz, 2220Hz, 2960Hz)
                float bell1 = Mathf.Sin(2f * Mathf.PI * 1480f * (i / (float)SampleRate));
                float bell2 = Mathf.Sin(2f * Mathf.PI * 2220f * (i / (float)SampleRate)) * 0.45f;
                float bell3 = Mathf.Sin(2f * Mathf.PI * 2960f * (i / (float)SampleRate)) * 0.20f;

                samples[i] = Mathf.Clamp((bell1 + bell2 + bell3) * env * 1.15f, -1f, 1f);
            }

            var clip = AudioClip.Create("SFX_StellarRainHit", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateFireballExplosionHit(float duration = 0.12f)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Pow(1f - t, 1.4f);

                // Deep fiery combustion bass (180Hz -> 50Hz)
                float bassFreq = Mathf.Lerp(180f, 50f, t);
                float boom = Mathf.Sin(2f * Mathf.PI * bassFreq * (i / (float)SampleRate));

                // Magma roar sizzling noise
                float roar = (UnityEngine.Random.value * 2f - 1f) * Mathf.Exp(-t * 12f) * 0.50f;

                samples[i] = Mathf.Clamp((boom * 0.70f + roar) * env * 1.35f, -1f, 1f);
            }

            var clip = AudioClip.Create("SFX_FireballHit", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateBurnTick(float duration = 0.045f)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Exp(-t * 30f);

                // Sizzling flame ember pop (bandpassed flame noise)
                float hiss = (UnityEngine.Random.value * 2f - 1f) * 0.60f;
                float pop = Mathf.Sin(2f * Mathf.PI * 1600f * (i / (float)SampleRate)) * 0.40f;

                samples[i] = Mathf.Clamp((hiss + pop) * env * 1.10f, -1f, 1f);
            }

            var clip = AudioClip.Create("SFX_BurnTick", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip GenerateShockTick(float duration = 0.040f)
        {
            int count = (int)(SampleRate * duration);
            float[] samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float env = Mathf.Exp(-t * 35f);

                // Zapping electric arc (jagged triangle wave with high-frequency buzz)
                float tCycle = Mathf.Repeat((i / (float)SampleRate) * 880f, 1f);
                float tri = (tCycle < 0.5f ? (tCycle * 4f - 1f) : (3f - tCycle * 4f));

                // Electric spark burst
                float spark = Mathf.Sin(2f * Mathf.PI * 2200f * (i / (float)SampleRate)) * 0.50f;

                samples[i] = Mathf.Clamp((tri * 0.65f + spark) * env * 1.20f, -1f, 1f);
            }

            var clip = AudioClip.Create("SFX_ShockTick", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
