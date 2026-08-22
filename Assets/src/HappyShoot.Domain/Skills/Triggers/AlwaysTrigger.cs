namespace HappyShoot.Domain.Skills.Triggers
{
    /// <summary>
    /// Skill trigger that executes every frame for continuous/aura/orbiting skills.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class AlwaysTrigger : ISkillTrigger
    {
        public bool CanTrigger(float deltaTime)
        {
            return true;
        }

        public void OnTriggered()
        {
            // No cooldown reset needed for continuous every-frame trigger
        }

        public void Reset()
        {
        }
    }
}
