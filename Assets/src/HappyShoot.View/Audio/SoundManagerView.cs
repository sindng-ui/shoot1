using System;
using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Session;

namespace HappyShoot.View.Audio
{
    /// <summary>
    /// Master Zero-GC Sound Manager with 16-channel AudioSource pooling, procedural audio clips,
    /// BGM loop playback, and smart sound debouncing for high-frequency combat events.
    /// </summary>
    public class SoundManagerView : MonoBehaviour
    {
        private const int SfxPoolSize = 32; // Expanded for mass-piercing simultaneous multi-hits
        private const int MaxHitsPerFrame = 4; // 한 프레임당 최대 피격음 개수 캡핑 (100마리 피격 시 과밀/소리 찢어짐 원천 차단)
        private const float DefaultDebounceTime = 0.02f;

        private readonly AudioSource[] _sfxSources = new AudioSource[SfxPoolSize];
        private int _currentSourceIndex = 0;
        private int _lastHitFrame = -1;
        private int _currentFrameHitCount = 0;

        private AudioSource _bgmSource;
        private EventBus _eventBus;

        private readonly Dictionary<SoundEffectType, AudioClip> _clipCache = new Dictionary<SoundEffectType, AudioClip>();
        private readonly Dictionary<SoundEffectType, float> _lastPlayTimes = new Dictionary<SoundEffectType, float>();

        public void Initialize(EventBus eventBus)
        {
            EnsureAudioSources();
            PreloadProceduralClips();
            BindEventBus(eventBus);
            PlayBgm();
        }

        private void EnsureAudioSources()
        {
            // Create BGM source
            if (_bgmSource == null)
            {
                var bgmGo = new GameObject("BgmSource");
                bgmGo.transform.SetParent(transform, false);
                _bgmSource = bgmGo.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.playOnAwake = false;
                _bgmSource.volume = 0.35f;
            }

            // Create SFX pool (32 channels)
            var sfxRoot = new GameObject("SfxPool");
            sfxRoot.transform.SetParent(transform, false);
            for (int i = 0; i < SfxPoolSize; i++)
            {
                var srcGo = new GameObject($"SfxSource_{i}");
                srcGo.transform.SetParent(sfxRoot.transform, false);
                var src = srcGo.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxSources[i] = src;
            }
        }

        private void PreloadProceduralClips()
        {
            foreach (SoundEffectType type in Enum.GetValues(typeof(SoundEffectType)))
            {
                if (!_clipCache.ContainsKey(type))
                {
                    _clipCache[type] = ProceduralAudioHelper.CreateSoundEffect(type);
                }
            }
        }

        public void BindEventBus(EventBus eventBus)
        {
            _eventBus = eventBus;
            if (_eventBus == null) return;

            _eventBus.Subscribe<PlaySoundEvent>(OnPlaySound);
            _eventBus.Subscribe<PlayBgmEvent>(OnPlayBgm);
            _eventBus.Subscribe<StopBgmEvent>(OnStopBgm);

            // Gameplay reactive audio hooks: Smart Throttling for mass combat (crisp punch without ear fatigue)
            _eventBus.Subscribe<MonsterDamagedEvent>(evt => HandleMonsterDamagedAudio(evt.IsCritical));
            _eventBus.Subscribe<MonsterDiedEvent>(evt => PlaySfx(SoundEffectType.MonsterDeath, 0.50f, 0.03f));
            _eventBus.Subscribe<ExpGainedEvent>(evt => PlaySfx(SoundEffectType.GemCollect, 0.35f, 0.02f));
            _eventBus.Subscribe<PlayerLevelUpEvent>(evt => PlaySfx(SoundEffectType.LevelUp, 0.9f, 0.2f));
            _eventBus.Subscribe<SkillEvolvedEvent>(evt => PlaySfx(SoundEffectType.WeaponEvolve, 1.0f, 0.3f));
            _eventBus.Subscribe<BossSpawnedEvent>(evt => PlaySfx(SoundEffectType.BossSpawn, 1.0f, 0.5f));
            _eventBus.Subscribe<GroundStompExecutedEvent>(evt => PlaySfx(SoundEffectType.GroundStomp, 0.95f, 0.08f));
            _eventBus.Subscribe<EarthshakerExecutedEvent>(evt => PlaySfx(SoundEffectType.GroundStomp, 1.0f, 0.08f));
            _eventBus.Subscribe<TreasureChestOpenedEvent>(evt => PlaySfx(SoundEffectType.ChestOpen, 0.85f, 0.2f));
            _eventBus.Subscribe<PlayerDamagedEvent>(evt => PlaySfx(SoundEffectType.PlayerHurt, 0.8f, 0.1f));
            _eventBus.Subscribe<PlayerDiedEvent>(evt => {
                PlaySfx(SoundEffectType.GameOver, 1.0f, 0.5f);
                if (_bgmSource != null) _bgmSource.volume = 0.1f;
            });
            _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

            Domain.Settings.GameSettings.OnSettingsChanged += ApplySettingsVolume;
        }

        private void HandleMonsterDamagedAudio(bool isCritical)
        {
            int currentFrame = Time.frameCount;
            if (_lastHitFrame != currentFrame)
            {
                _lastHitFrame = currentFrame;
                _currentFrameHitCount = 0;
            }

            // Frame-level cap: max 4 hits per frame to prevent audio clipping when hitting 50-100 mobs
            if (_currentFrameHitCount >= MaxHitsPerFrame)
            {
                return;
            }

            _currentFrameHitCount++;

            // Rich & audible punchy volume with slight side-chain compression
            float baseVol = isCritical ? 1.0f : 0.85f;
            float volume = baseVol * (1.0f - (_currentFrameHitCount - 1) * 0.04f);
            PlaySfx(SoundEffectType.MonsterHit, volume, 0.004f);
        }

        private void OnDestroy()
        {
            Domain.Settings.GameSettings.OnSettingsChanged -= ApplySettingsVolume;
        }

        private void ApplySettingsVolume()
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = Domain.Settings.GameSettings.IsMuted 
                    ? 0f 
                    : (Domain.Settings.GameSettings.BgmVolume * 0.35f);
            }
        }

        public void PlaySfx(SoundEffectType type, float volume = 1.0f, float debounceSeconds = DefaultDebounceTime)
        {
            if (Domain.Settings.GameSettings.IsMuted || Domain.Settings.GameSettings.SfxVolume <= 0.001f)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (_lastPlayTimes.TryGetValue(type, out float lastTime) && (now - lastTime < debounceSeconds))
            {
                return; // Debounce rapid triggers
            }
            _lastPlayTimes[type] = now;

            if (_clipCache.TryGetValue(type, out var clip) && clip != null)
            {
                var src = _sfxSources[_currentSourceIndex];
                _currentSourceIndex = (_currentSourceIndex + 1) % SfxPoolSize;

                src.clip = clip;
                src.volume = Mathf.Clamp01(volume * Domain.Settings.GameSettings.SfxVolume);

                // Add subtle pitch variation for high-frequency hit juice (eliminates robotic sound)
                if (type == SoundEffectType.MonsterHit || type == SoundEffectType.MonsterDeath)
                {
                    src.pitch = UnityEngine.Random.Range(0.90f, 1.15f);
                }
                else
                {
                    src.pitch = 1.0f;
                }

                src.Play();
            }
        }

        public void PlayBgm()
        {
            if (_bgmSource == null) return;

            var bgmClip = ProceduralAudioHelper.CreateRetroBgmTrack();
            _bgmSource.clip = bgmClip;
            ApplySettingsVolume();
            _bgmSource.Play();
        }

        private void OnPlaySound(PlaySoundEvent evt)
        {
            PlaySfx(evt.SoundType, evt.Volume);
        }

        private void OnPlayBgm(PlayBgmEvent evt)
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = evt.Volume;
                if (!_bgmSource.isPlaying) _bgmSource.Play();
            }
        }

        private void OnStopBgm(StopBgmEvent evt)
        {
            if (_bgmSource != null)
            {
                _bgmSource.Stop();
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            if (_bgmSource == null) return;

            if (evt.NewState == GameState.Paused)
            {
                _bgmSource.pitch = 0.8f;
                _bgmSource.volume = 0.15f;
            }
            else if (evt.NewState == GameState.Playing)
            {
                _bgmSource.pitch = 1.0f;
                _bgmSource.volume = 0.30f;
            }
        }
    }
}
