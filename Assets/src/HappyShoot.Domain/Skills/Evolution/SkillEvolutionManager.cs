using System;
using System.Collections.Generic;
using System.Linq;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;

namespace HappyShoot.Domain.Skills.Evolution
{
    /// <summary>
    /// Evaluates player skills and passives against evolution recipes and handles weapon evolution.
    /// </summary>
    public class SkillEvolutionManager
    {
        private readonly List<SkillEvolutionRecipe> _recipes = new List<SkillEvolutionRecipe>();
        private readonly EventBus _eventBus;

        public IReadOnlyList<SkillEvolutionRecipe> Recipes => _recipes;

        public SkillEvolutionManager(EventBus eventBus = null)
        {
            _eventBus = eventBus;
        }

        public void RegisterRecipe(SkillEvolutionRecipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            _recipes.Add(recipe);
        }

        /// <summary>
        /// Finds all recipes that the player currently satisfies (Max level active + required passive).
        /// </summary>
        public List<SkillEvolutionRecipe> GetAvailableEvolutions(PlayerEntity player)
        {
            if (player == null) return new List<SkillEvolutionRecipe>();

            var readyRecipes = new List<SkillEvolutionRecipe>();
            for (int i = 0; i < _recipes.Count; i++)
            {
                var recipe = _recipes[i];

                var existingSkill = player.Skills.FirstOrDefault(s => s.Id == recipe.BaseSkillId);
                if (existingSkill != null && existingSkill.IsMaxLevel)
                {
                    if (player.HasPassive(recipe.RequiredPassiveId))
                    {
                        readyRecipes.Add(recipe);
                    }
                }
            }

            return readyRecipes;
        }

        /// <summary>
        /// Executes an evolution, replacing the old skill with the newly evolved weapon.
        /// </summary>
        public bool EvolveSkill(PlayerEntity player, SkillEvolutionRecipe recipe)
        {
            if (player == null || recipe == null) return false;

            ISkill evolvedSkill = recipe.EvolvedSkillFactory.Invoke();
            bool replaced = player.ReplaceSkill(recipe.BaseSkillId, evolvedSkill);

            if (replaced)
            {
                _eventBus?.Publish(new SkillEvolvedEvent(recipe.BaseSkillId, recipe.EvolvedSkillId, recipe.EvolvedSkillName));
                return true;
            }

            return false;
        }
    }
}
