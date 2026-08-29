using HappyShoot.Domain.Entities;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Controls the multi-phase wave progression across all 3 major boss acts.
    /// Phase 1: Classic slimes/bats/skeletons/golems -> Boss 1 (Goblin King)
    /// Phase 2: FireImps, ToxicSpiders, DarkKnights -> Boss 2 (Dragon Fiend)
    /// Phase 3 (Fast Paced 60s): Wraiths, Necromancers, Abominations, Reapers -> Boss 3 (Arch-Lich King)
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class WavePhaseController
    {
        public enum Phase
        {
            Phase1,
            Phase2Wave1,
            Phase2Wave2,
            Phase2Wave3,
            Boss2Spawned,
            Phase3Wave1, // 0s ~ 15s: Wraith joins
            Phase3Wave2, // 15s ~ 30s: Necromancer joins
            Phase3Wave3, // 30s ~ 45s: Abomination joins
            Phase3Wave4, // 45s ~ 60s: Reaper joins
            Boss3Spawned,// 60s: Arch-Lich King
            StageClear   // Victory!
        }

        public Phase CurrentPhase { get; private set; } = Phase.Phase1;
        public bool Boss1Defeated { get; private set; }
        public bool Boss2Defeated { get; private set; }
        public bool Boss3Defeated { get; private set; }

        // Phase 2 intervals - Snappy 15s pacing (Boss 2 in 45s!)
        private const float Wave2IntroTime = 15f; // Toxic Spider at 15s
        private const float Wave3IntroTime = 30f; // Dark Knight at 30s
        private const float Wave4IntroTime = 45f; // Boss 2 (Dragon Fiend) at 45s!

        // Phase 3 speed: 15s per new monster, 60s to final boss!
        private const float P3Wave2IntroTime = 15f;
        private const float P3Wave3IntroTime = 30f;
        private const float P3Wave4IntroTime = 45f;
        private const float P3Boss3IntroTime = 60f;

        private float _phaseTimer;

        public void OnBossDefeated(int bossIndex)
        {
            if (bossIndex == 1 && !Boss1Defeated)
            {
                Boss1Defeated = true;
                CurrentPhase = Phase.Phase2Wave1;
                _phaseTimer = 0f;
            }
            else if (bossIndex == 2 && !Boss2Defeated)
            {
                Boss2Defeated = true;
                CurrentPhase = Phase.Phase3Wave1;
                _phaseTimer = 0f;
            }
            else if (bossIndex >= 3)
            {
                Boss3Defeated = true;
                CurrentPhase = Phase.StageClear;
            }
        }

        public void OnBossDefeated(bool isBoss2)
        {
            if (isBoss2) OnBossDefeated(2);
            else OnBossDefeated(1);
        }

        public void JumpToPhase(int phaseNumber)
        {
            _phaseTimer = 0f;
            if (phaseNumber <= 1)
            {
                Boss1Defeated = false;
                Boss2Defeated = false;
                Boss3Defeated = false;
                CurrentPhase = Phase.Phase1;
            }
            else if (phaseNumber == 2)
            {
                Boss1Defeated = true;
                Boss2Defeated = false;
                Boss3Defeated = false;
                CurrentPhase = Phase.Phase2Wave1;
            }
            else
            {
                Boss1Defeated = true;
                Boss2Defeated = true;
                Boss3Defeated = false;
                CurrentPhase = Phase.Phase3Wave1;
            }
        }

        public void Update(float deltaTime)
        {
            if (!Boss1Defeated) return;

            // Phase 2 progression
            if (!Boss2Defeated)
            {
                if (CurrentPhase == Phase.Boss2Spawned) return;

                _phaseTimer += deltaTime;

                if (CurrentPhase == Phase.Phase2Wave1 && _phaseTimer >= Wave2IntroTime)
                {
                    CurrentPhase = Phase.Phase2Wave2;
                }
                else if (CurrentPhase == Phase.Phase2Wave2 && _phaseTimer >= Wave3IntroTime)
                {
                    CurrentPhase = Phase.Phase2Wave3;
                }
                else if (CurrentPhase == Phase.Phase2Wave3 && _phaseTimer >= Wave4IntroTime)
                {
                    CurrentPhase = Phase.Boss2Spawned;
                }
                return;
            }

            // Phase 3 progression (Snappy 60s sequence)
            if (!Boss3Defeated)
            {
                if (CurrentPhase == Phase.Boss3Spawned) return;

                _phaseTimer += deltaTime;

                if (CurrentPhase == Phase.Phase3Wave1 && _phaseTimer >= P3Wave2IntroTime)
                {
                    CurrentPhase = Phase.Phase3Wave2;
                }
                else if (CurrentPhase == Phase.Phase3Wave2 && _phaseTimer >= P3Wave3IntroTime)
                {
                    CurrentPhase = Phase.Phase3Wave3;
                }
                else if (CurrentPhase == Phase.Phase3Wave3 && _phaseTimer >= P3Wave4IntroTime)
                {
                    CurrentPhase = Phase.Phase3Wave4;
                }
                else if (CurrentPhase == Phase.Phase3Wave4 && _phaseTimer >= P3Boss3IntroTime)
                {
                    CurrentPhase = Phase.Boss3Spawned;
                }
            }
        }

        public MonsterType RollPhase1Type(float gameTime)
        {
            if (gameTime < 45f)
                return UnityEngine.Random.value < 0.7f ? MonsterType.Slime : MonsterType.Bat;

            if (gameTime < 150f)
            {
                float r = UnityEngine.Random.value;
                if (r < 0.4f) return MonsterType.Slime;
                if (r < 0.75f) return MonsterType.Bat;
                return MonsterType.Skeleton;
            }

            float roll = UnityEngine.Random.value;
            if (roll < 0.3f) return MonsterType.Slime;
            if (roll < 0.55f) return MonsterType.Bat;
            if (roll < 0.8f) return MonsterType.Skeleton;
            return MonsterType.Golem;
        }

        public MonsterType RollPhase2Type()
        {
            float r = UnityEngine.Random.value;
            switch (CurrentPhase)
            {
                case Phase.Phase2Wave1:
                    if (r < 0.45f) return MonsterType.FireImp;
                    if (r < 0.70f) return MonsterType.Skeleton;
                    if (r < 0.85f) return MonsterType.Bat;
                    return MonsterType.Slime;

                case Phase.Phase2Wave2:
                    if (r < 0.35f) return MonsterType.ToxicSpider;
                    if (r < 0.65f) return MonsterType.FireImp;
                    if (r < 0.85f) return MonsterType.Golem;
                    return MonsterType.Skeleton;

                case Phase.Phase2Wave3:
                case Phase.Boss2Spawned:
                    if (r < 0.30f) return MonsterType.DarkKnight;
                    if (r < 0.55f) return MonsterType.ToxicSpider;
                    if (r < 0.80f) return MonsterType.FireImp;
                    return MonsterType.Golem;

                default:
                    return MonsterType.Slime;
            }
        }

        public MonsterType RollPhase3Type()
        {
            float r = UnityEngine.Random.value;
            switch (CurrentPhase)
            {
                case Phase.Phase3Wave1: // 0~15s: Wraith joins
                    if (r < 0.50f) return MonsterType.Wraith;
                    if (r < 0.75f) return MonsterType.DarkKnight;
                    if (r < 0.90f) return MonsterType.FireImp;
                    return MonsterType.ToxicSpider;

                case Phase.Phase3Wave2: // 15~30s: Necromancer joins
                    if (r < 0.40f) return MonsterType.Necromancer;
                    if (r < 0.70f) return MonsterType.Wraith;
                    if (r < 0.85f) return MonsterType.DarkKnight;
                    return MonsterType.FireImp;

                case Phase.Phase3Wave3: // 30~45s: Abomination joins
                    if (r < 0.35f) return MonsterType.Abomination;
                    if (r < 0.60f) return MonsterType.Necromancer;
                    if (r < 0.85f) return MonsterType.Wraith;
                    return MonsterType.DarkKnight;

                case Phase.Phase3Wave4: // 45~60s: Reaper joins
                case Phase.Boss3Spawned:
                    if (r < 0.35f) return MonsterType.Reaper;
                    if (r < 0.60f) return MonsterType.Abomination;
                    if (r < 0.80f) return MonsterType.Necromancer;
                    return MonsterType.Wraith;

                default:
                    return MonsterType.Wraith;
            }
        }

        public MonsterDefinition RollPhase1Archetype(float gameTime, MonsterTuningConfigData cfg = null)
        {
            return MonsterDefinition.FromConfig(RollPhase1Type(gameTime), cfg);
        }

        public MonsterDefinition RollPhase2Archetype(MonsterTuningConfigData cfg = null)
        {
            return MonsterDefinition.FromConfig(RollPhase2Type(), cfg);
        }

        public MonsterDefinition RollPhase3Archetype(MonsterTuningConfigData cfg = null)
        {
            return MonsterDefinition.FromConfig(RollPhase3Type(), cfg);
        }
    }
}
