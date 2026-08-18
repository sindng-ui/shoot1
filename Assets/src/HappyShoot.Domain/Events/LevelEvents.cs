namespace HappyShoot.Domain.Events
{
    public readonly struct ExpGainedEvent : IDomainEvent
    {
        public readonly int Amount;
        public readonly int CurrentExp;
        public readonly int RequiredExp;

        public ExpGainedEvent(int amount, int currentExp, int requiredExp)
        {
            Amount = amount;
            CurrentExp = currentExp;
            RequiredExp = requiredExp;
        }
    }

    public readonly struct PlayerLevelUpEvent : IDomainEvent
    {
        public readonly int NewLevel;

        public PlayerLevelUpEvent(int newLevel)
        {
            NewLevel = newLevel;
        }
    }
}
