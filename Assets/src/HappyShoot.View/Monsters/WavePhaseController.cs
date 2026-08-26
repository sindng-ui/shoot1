using HappyShoot.Domain.Entities;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Controls the multi-phase wave progression after boss defeats.
    /// Phase 1: Classic slimes/bats/skeletons/golems -> Boss 1
    /// Phase 2: 3 new monster types introduced sequentially -> Boss 2
    /// </summary>
    public class WavePhaseController
    {
        public enum Phase { Phase1, Phase2Wave1, Phase2Wave2, Phase2Wave3, Boss2Spawned }

        public Phase CurrentPhase { get; private set; } = Phase.Phase1;
        public bool Boss1Defeated { get; private set; }
        public bool Boss2Defeated { get; private set; }

        // How long after boss defeat each new wave type introduces
        private const float Wave2IntroTime = 30f;
        private const float Wave3IntroTime = 70f;
        private const float Wave4IntroTime = 110f;

        private float _phaseTimer;

        public void OnBossDefeated(bool isBoss2)
        {
            if (isBoss2)
            {
                Boss2Defeated = true;
            }
            else if (!Boss1Defeated)
            {
                Boss1Defeated = true;
                CurrentPhase = Phase.Phase2Wave1;
                _phaseTimer = 0f;
            }
        }

        public void Update(float deltaTime)
        {
            if (!Boss1Defeated) return;
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
                    // Wave 1: Introduce FireImp with veteran skeletons, bats & slimes
                    if (r < 0.45f) return MonsterType.FireImp;
                    if (r < 0.70f) return MonsterType.Skeleton;
                    if (r < 0.85f) return MonsterType.Bat;
                    return MonsterType.Slime;

                case Phase.Phase2Wave2:
                    // Wave 2: Toxic Spiders swarm with FireImps and Golems
                    if (r < 0.35f) return MonsterType.ToxicSpider;
                    if (r < 0.65f) return MonsterType.FireImp;
                    if (r < 0.85f) return MonsterType.Golem;
                    return MonsterType.Skeleton;

                case Phase.Phase2Wave3:
                case Phase.Boss2Spawned:
                    // Wave 3: Dark Knights lead the invasion with Spiders, Imps & Golems
                    if (r < 0.30f) return MonsterType.DarkKnight;
                    if (r < 0.55f) return MonsterType.ToxicSpider;
                    if (r < 0.80f) return MonsterType.FireImp;
                    return MonsterType.Golem;

                default:
                    return MonsterType.Slime;
            }
        }

        public MonsterDefinition RollPhase1Archetype(float gameTime) => RollPhase1Archetype(gameTime, null);
        public MonsterDefinition RollPhase1Archetype(float gameTime, MonsterTuningConfigData cfg)
        {
            return MonsterDefinition.FromConfig(RollPhase1Type(gameTime), cfg);
        }

        public MonsterDefinition RollPhase2Archetype() => RollPhase2Archetype(null);
        public MonsterDefinition RollPhase2Archetype(MonsterTuningConfigData cfg)
        {
            return MonsterDefinition.FromConfig(RollPhase2Type(), cfg);
        }
    }
}
