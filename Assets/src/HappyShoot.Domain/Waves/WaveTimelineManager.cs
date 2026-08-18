using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Waves
{
    /// <summary>
    /// Manages the 15-minute pacing timeline, dynamic difficulty, and boss spawn triggers.
    /// </summary>
    public class WaveTimelineManager
    {
        private readonly List<WaveStep> _timeline = new List<WaveStep>();
        private float _elapsedTime;
        private int _currentStepIndex;
        private float _spawnTimer;

        public float ElapsedTime => _elapsedTime;
        public int CurrentStepIndex => _currentStepIndex;
        public bool IsRunComplete => _elapsedTime >= 900f; // 15 minutes = 900s

        public event Action<WaveBossType, Vector2D> OnBossSpawnTriggered;
        public event Action<WaveStep> OnWaveChanged;

        public WaveTimelineManager()
        {
            SetupStandard15MinuteTimeline();
        }

        private void SetupStandard15MinuteTimeline()
        {
            // 00:00 ~ 03:00 (0 - 180s)
            _timeline.Add(new WaveStep(0f, 180f, "Slime", spawnInterval: 0.8f, monsterHp: 20f, monsterSpeed: 2.2f));

            // 03:00 (180s) First Elite
            _timeline.Add(new WaveStep(180f, 181f, "EliteGoblin", spawnInterval: 999f, monsterHp: 300f, monsterSpeed: 3.0f, bossEvent: WaveBossType.Elite));

            // 03:00 ~ 08:00 (181 - 480s)
            _timeline.Add(new WaveStep(181f, 480f, "FastBat", spawnInterval: 0.5f, monsterHp: 45f, monsterSpeed: 3.5f));

            // 08:00 (480s) Mid-Boss + Swarm
            _timeline.Add(new WaveStep(480f, 481f, "MidBossGolem", spawnInterval: 999f, monsterHp: 1200f, monsterSpeed: 2.0f, bossEvent: WaveBossType.MidBoss));

            // 08:00 ~ 14:00 (481 - 840s)
            _timeline.Add(new WaveStep(481f, 840f, "SkeletonKnight", spawnInterval: 0.3f, monsterHp: 100f, monsterSpeed: 3.0f));

            // 14:00 ~ 15:00 (840 - 900s) Late Pre-Boss Swarm
            _timeline.Add(new WaveStep(840f, 900f, "DemonMinion", spawnInterval: 0.15f, monsterHp: 180f, monsterSpeed: 4.0f));

            // 15:00 (900s) Final Boss
            _timeline.Add(new WaveStep(900f, 9999f, "DeathLord", spawnInterval: 999f, monsterHp: 5000f, monsterSpeed: 3.2f, bossEvent: WaveBossType.FinalBoss));
        }

        /// <summary>
        /// Updates the timeline, spawning regular wave monsters and triggering boss encounters.
        /// </summary>
        public void Update(float deltaTime, MonsterSpawner spawner, Vector2D playerPosition)
        {
            if (deltaTime <= 0f) return;

            _elapsedTime += deltaTime;

            // Find current active step
            for (int i = 0; i < _timeline.Count; i++)
            {
                var step = _timeline[i];
                if (_elapsedTime >= step.StartTimeSeconds && _elapsedTime < step.EndTimeSeconds)
                {
                    if (_currentStepIndex != i)
                    {
                        _currentStepIndex = i;
                        OnWaveChanged?.Invoke(step);
                    }

                    // Handle boss spawn
                    if (step.BossEvent != WaveBossType.None && !step.IsBossSpawned)
                    {
                        step.IsBossSpawned = true;
                        OnBossSpawnTriggered?.Invoke(step.BossEvent, playerPosition);

                        if (spawner != null)
                        {
                            spawner.SpawnAroundPlayer(playerPosition, spawnRadius: 10f, angleRadians: 0f,
                                typeName: step.PrimaryMonsterType, maxHealth: step.MonsterHp, moveSpeed: step.MonsterSpeed,
                                contactDamage: 25f, expValue: 50, goldValue: 100);
                        }
                    }

                    // Handle regular wave spawning
                    if (spawner != null && step.SpawnInterval < 100f)
                    {
                        _spawnTimer += deltaTime;
                        if (_spawnTimer >= step.SpawnInterval)
                        {
                            _spawnTimer = 0f;
                            float randAngle = (float)(new Random().NextDouble() * Math.PI * 2.0);
                            spawner.SpawnAroundPlayer(playerPosition, spawnRadius: 12f, angleRadians: randAngle,
                                typeName: step.PrimaryMonsterType, maxHealth: step.MonsterHp, moveSpeed: step.MonsterSpeed);
                        }
                    }

                    break;
                }
            }
        }
    }
}
