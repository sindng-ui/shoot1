namespace HappyShoot.Domain.Events
{
    public enum SoundEffectType
    {
        SlashAttack,
        BowShoot,
        MagicExplosion,
        BladeOrbit,
        MonsterHit,
        MonsterDeath,
        GemCollect,
        LevelUp,
        WeaponEvolve,
        BossSpawn,
        ChestOpen,
        PlayerHurt,
        GameOver,
        Victory
    }

    public readonly struct PlaySoundEvent : IDomainEvent
    {
        public readonly SoundEffectType SoundType;
        public readonly float Volume;

        public PlaySoundEvent(SoundEffectType soundType, float volume = 1.0f)
        {
            SoundType = soundType;
            Volume = volume;
        }
    }

    public readonly struct PlayBgmEvent : IDomainEvent
    {
        public readonly string BgmTrackName;
        public readonly float Volume;

        public PlayBgmEvent(string bgmTrackName, float volume = 0.5f)
        {
            BgmTrackName = bgmTrackName;
            Volume = volume;
        }
    }

    public readonly struct StopBgmEvent : IDomainEvent
    {
    }
}
