namespace HappyShoot.Domain.Waves
{
    public enum WaveBossType
    {
        None,
        Elite,
        MidBoss,
        FinalBoss
    }

    public class WaveStep
    {
        public float StartTimeSeconds { get; }
        public float EndTimeSeconds { get; }
        public string PrimaryMonsterType { get; }
        public float SpawnInterval { get; }
        public float MonsterHp { get; }
        public float MonsterSpeed { get; }
        public WaveBossType BossEvent { get; }
        public bool IsBossSpawned { get; set; }

        public WaveStep(
            float startTime,
            float endTime,
            string primaryMonster,
            float spawnInterval,
            float monsterHp,
            float monsterSpeed,
            WaveBossType bossEvent = WaveBossType.None)
        {
            StartTimeSeconds = startTime;
            EndTimeSeconds = endTime;
            PrimaryMonsterType = primaryMonster;
            SpawnInterval = spawnInterval;
            MonsterHp = monsterHp;
            MonsterSpeed = monsterSpeed;
            BossEvent = bossEvent;
            IsBossSpawned = false;
        }
    }
}
