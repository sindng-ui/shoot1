namespace HappyShoot.Domain.Events
{
    public readonly struct SkillEvolvedEvent : IDomainEvent
    {
        public readonly string OldSkillId;
        public readonly string EvolvedSkillId;
        public readonly string EvolvedSkillName;

        public SkillEvolvedEvent(string oldSkillId, string evolvedSkillId, string evolvedSkillName)
        {
            OldSkillId = oldSkillId;
            EvolvedSkillId = evolvedSkillId;
            EvolvedSkillName = evolvedSkillName;
        }
    }
}
