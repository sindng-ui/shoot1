namespace HappyShoot.Domain.Events
{
    public enum SoundEffectType
    {
        SlashAttack,
        BowShoot,
        MagicExplosion,
        MonsterHit,
        MonsterDeath,
        GemCollect,
        LevelUp,
        WeaponEvolve,
        PlayerHurt
    }

    public readonly struct PlaySoundEvent : IDomainEvent
    {
        public readonly SoundEffectType SoundType;

        public PlaySoundEvent(SoundEffectType soundType)
        {
            SoundType = soundType;
        }
    }
}
