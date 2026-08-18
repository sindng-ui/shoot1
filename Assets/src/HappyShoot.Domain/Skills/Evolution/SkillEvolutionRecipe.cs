using System;
using HappyShoot.Domain.Skills;

namespace HappyShoot.Domain.Skills.Evolution
{
    /// <summary>
    /// Definition of a skill synthesis / evolution recipe.
    /// </summary>
    public class SkillEvolutionRecipe
    {
        public string BaseSkillId { get; }
        public string RequiredPassiveId { get; }
        public string EvolvedSkillId { get; }
        public string EvolvedSkillName { get; }
        public Func<ISkill> EvolvedSkillFactory { get; }

        public SkillEvolutionRecipe(
            string baseSkillId,
            string requiredPassiveId,
            string evolvedSkillId,
            string evolvedSkillName,
            Func<ISkill> evolvedSkillFactory)
        {
            BaseSkillId = baseSkillId ?? throw new ArgumentNullException(nameof(baseSkillId));
            RequiredPassiveId = requiredPassiveId ?? throw new ArgumentNullException(nameof(requiredPassiveId));
            EvolvedSkillId = evolvedSkillId ?? throw new ArgumentNullException(nameof(evolvedSkillId));
            EvolvedSkillName = evolvedSkillName ?? throw new ArgumentNullException(nameof(evolvedSkillName));
            EvolvedSkillFactory = evolvedSkillFactory ?? throw new ArgumentNullException(nameof(evolvedSkillFactory));
        }
    }
}
