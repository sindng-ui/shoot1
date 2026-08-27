using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Events
{
    /// <summary>
    /// Categorizes the damage source to allow distinct audio and visual responses per skill and dot effect.
    /// </summary>
    public enum DamageType
    {
        Default,      // Melee punch/slash/stomp
        Arrow,        // Pierce Arrow, Storm Bow (crisp sharp pierce thwip)
        WindGlaive,   // Wind Glaive, Phantom Glaive (razor whirlwind slice)
        StellarRain,  // Stellar Rain, Arrow Rain (ethereal crystal drops)
        Fireball,     // Fireball, Inferno Fireball (fiery boom)
        BurnDot,      // Fire burn tick (sizzling crackle)
        ShockDot      // Lightning shock tick (zapping electric spark)
    }

    public readonly struct MonsterDamagedEvent : IDomainEvent
    {
        public readonly int MonsterId;
        public readonly float Damage;
        public readonly float RemainingHealth;
        public readonly float MaxHealth;
        public readonly Vector2D Position;
        public readonly bool IsCritical;
        public readonly DamageType DamageType;

        public MonsterDamagedEvent(
            int monsterId,
            float damage,
            float remainingHealth,
            float maxHealth,
            Vector2D position,
            bool isCritical = false,
            DamageType damageType = DamageType.Default)
        {
            MonsterId = monsterId;
            Damage = damage;
            RemainingHealth = remainingHealth;
            MaxHealth = maxHealth;
            Position = position;
            IsCritical = isCritical;
            DamageType = damageType;
        }
    }

    public readonly struct MonsterDiedEvent : IDomainEvent
    {
        public readonly int MonsterId;
        public readonly Vector2D Position;
        public readonly int ExpValue;
        public readonly int GoldValue;
        public readonly Entities.MonsterType MonsterType;
        public readonly bool IsBoss;

        public MonsterDiedEvent(int monsterId, Vector2D position, int expValue, int goldValue, Entities.MonsterType monsterType = Entities.MonsterType.Slime, bool isBoss = false)
        {
            MonsterId = monsterId;
            Position = position;
            ExpValue = expValue;
            GoldValue = goldValue;
            MonsterType = monsterType;
            IsBoss = isBoss;
        }
    }
}
