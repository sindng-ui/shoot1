using System.Linq;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Evolution;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Skills
{
    [TestFixture]
    public class SkillEvolutionTests
    {
        private EventBus _eventBus;
        private SkillEvolutionManager _evolutionManager;
        private SkillRewardManager _rewardManager;
        private PlayerEntity _player;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _evolutionManager = new SkillEvolutionManager(_eventBus);
            _rewardManager = new SkillRewardManager(_evolutionManager);

            // Create warrior with default "slash" skill
            _player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Warrior, Vector2D.Zero, _eventBus);

            // Register 3 standard Evolution Recipes
            _evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                baseSkillId: "slash",
                requiredPassiveId: "passive_fang",
                evolvedSkillId: "blood_eater",
                evolvedSkillName: "Blood Eater",
                evolvedSkillFactory: () => new CompositeSkill("blood_eater", "Blood Eater", new CooldownTrigger(0.9f), new ClosestEnemyTargeter(), new BloodEaterEffect())
            ));

            _evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                baseSkillId: "bow",
                requiredPassiveId: "passive_feather",
                evolvedSkillId: "storm_bow",
                evolvedSkillName: "Storm Bow",
                evolvedSkillFactory: () => new CompositeSkill("storm_bow", "Storm Bow", new CooldownTrigger(5.0f), new ClosestEnemyTargeter(), new StormArrowEffect())
            ));

            _evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                baseSkillId: "explosion",
                requiredPassiveId: "passive_rune",
                evolvedSkillId: "meteor_strike",
                evolvedSkillName: "Meteor Strike",
                evolvedSkillFactory: () => new CompositeSkill("meteor_strike", "Meteor Strike", new CooldownTrigger(1.2f), new ClosestEnemyTargeter(), new MeteorStrikeEffect())
            ));
        }

        [Test]
        public void GetAvailableEvolutions_ReturnsEmpty_WhenSkillNotMaxLevel()
        {
            _player.AddPassive("passive_fang"); // Has passive, but Slash is Lv 1

            var available = _evolutionManager.GetAvailableEvolutions(_player);
            Assert.That(available.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAvailableEvolutions_ReturnsEmpty_WhenPassiveMissing()
        {
            var slash = _player.Skills.First(s => s.Id == "slash");
            while (!slash.IsMaxLevel) slash.LevelUp(); // Slash max level, but no passive

            var available = _evolutionManager.GetAvailableEvolutions(_player);
            Assert.That(available.Count, Is.EqualTo(0));
        }

        [Test]
        public void EvolveSkill_ReplacesBaseSkillWithEvolvedSkill_WhenConditionsMet()
        {
            var slash = _player.Skills.First(s => s.Id == "slash");
            while (!slash.IsMaxLevel) slash.LevelUp();
            _player.AddPassive("passive_fang");

            var available = _evolutionManager.GetAvailableEvolutions(_player);
            Assert.That(available.Count, Is.EqualTo(1));

            bool evolvedEventFired = false;
            _eventBus.Subscribe<SkillEvolvedEvent>(evt => evolvedEventFired = true);

            bool success = _evolutionManager.EvolveSkill(_player, available[0]);

            Assert.That(success, Is.True);
            Assert.That(evolvedEventFired, Is.True);
            Assert.That(_player.Skills.Any(s => s.Id == "slash"), Is.False);
            Assert.That(_player.Skills.Any(s => s.Id == "blood_eater"), Is.True);
        }

        [Test]
        public void RollRewards_PrioritizesEvolutionOption_WhenConditionSatisfied()
        {
            var slash = _player.Skills.First(s => s.Id == "slash");
            while (!slash.IsMaxLevel) slash.LevelUp();
            _player.AddPassive("passive_fang");

            var rewards = _rewardManager.RollRewards(_player, count: 3);
            var evoReward = rewards.Find(r => r.Category == RewardCategory.EvolveSkill);

            Assert.That(evoReward, Is.Not.Null);
            Assert.That(evoReward.Id, Is.EqualTo("blood_eater"));

            // Apply evolution reward
            _rewardManager.ApplyReward(_player, evoReward);
            Assert.That(_player.Skills.Any(s => s.Id == "blood_eater"), Is.True);
        }

        [Test]
        public void RollRewards_ExcludesBaseSkill_AfterEvolution()
        {
            _rewardManager.RegisterSkill("slash", "Slash", "Base Melee", () => new CompositeSkill("slash", "Slash", new CooldownTrigger(1f), new ClosestEnemyTargeter(), new GreatswordSlashEffect()), new[] { CharacterClassType.Warrior });
            _rewardManager.RegisterSkill("whirlwind", "Whirlwind", "Spin Attack", () => new CompositeSkill("whirlwind", "Whirlwind", new CooldownTrigger(2f), new ClosestEnemyTargeter(), new WhirlwindEffect()), new[] { CharacterClassType.Warrior });

            var slash = _player.Skills.First(s => s.Id == "slash");
            while (!slash.IsMaxLevel) slash.LevelUp();
            _player.AddPassive("passive_fang");

            var rewards = _rewardManager.RollRewards(_player, count: 5);
            var evoReward = rewards.Find(r => r.Category == RewardCategory.EvolveSkill);
            Assert.That(evoReward, Is.Not.Null);

            // Execute evolution -> Slash replaced with Blood Eater
            _rewardManager.ApplyReward(_player, evoReward);
            Assert.That(_player.Skills.Any(s => s.Id == "slash"), Is.False);
            Assert.That(_player.Skills.Any(s => s.Id == "blood_eater"), Is.True);

            // Roll rewards again after evolution -> Base skill "slash" must NOT appear in rewards!
            var postEvoRewards = _rewardManager.RollRewards(_player, count: 10);
            var slashOption = postEvoRewards.Find(r => r.Id == "slash");

            Assert.That(slashOption, Is.Null, "Base skill 'slash' must NOT be offered after evolving into 'blood_eater'!");
        }

        [Test]
        public void AllNineSkillEvolutionRecipes_RegisterAndEvolveCorrectly()
        {
            var evoManager = new SkillEvolutionManager(_eventBus);

            // Register all 9 recipes
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("slash", "passive_fang", "blood_eater", "Blood Eater", () => new CompositeSkill("blood_eater", "Blood Eater", new CooldownTrigger(0.85f), new ClosestEnemyTargeter(), new BloodEaterEffect())));
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("whirlwind", "passive_feather", "tempest_whirlwind", "Tempest Whirlwind", () => new CompositeSkill("tempest_whirlwind", "Tempest Whirlwind", new CooldownTrigger(1.1f), new ClosestEnemyTargeter(), new TempestWhirlwindEffect())));
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("ground_stomp", "passive_armor", "earthshaker", "Earthshaker", () => new CompositeSkill("earthshaker", "Earthshaker", new CooldownTrigger(1.6f), new ClosestEnemyTargeter(), new EarthshakerEffect())));
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("bow", "passive_feather", "storm_bow", "Storm Bow", () => new CompositeSkill("storm_bow", "Storm Bow", new CooldownTrigger(1.6f), new ClosestEnemyTargeter(), new StormArrowEffect())));
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("glaive", "passive_crit", "phantom_glaive", "Phantom Glaive", () => new CompositeSkill("phantom_glaive", "Phantom Glaive", new CooldownTrigger(1.3f), new ClosestEnemyTargeter(), new PhantomGlaiveEffect())));
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("arrow_rain", "passive_ring", "stellar_rain", "Stellar Rain", () => new CompositeSkill("stellar_rain", "Stellar Rain", new CooldownTrigger(2.2f), new ClosestEnemyTargeter(), new StellarRainEffect())));
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("fireball", "passive_rune", "meteor_strike", "Meteor Strike", () => new CompositeSkill("meteor_strike", "Meteor Strike", new CooldownTrigger(1.2f), new ClosestEnemyTargeter(), new MeteorStrikeEffect())));
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("chain_lightning", "passive_overcharge", "gigastorm_lightning", "Gigastorm Lightning", () => new CompositeSkill("gigastorm_lightning", "Gigastorm Lightning", new CooldownTrigger(1.4f), new ClosestEnemyTargeter(), new GigastormLightningEffect())));
            evoManager.RegisterRecipe(new SkillEvolutionRecipe("frost_nova", "passive_heart", "blizzard_nova", "Blizzard Nova", () => new CompositeSkill("blizzard_nova", "Blizzard Nova", new CooldownTrigger(1.8f), new ClosestEnemyTargeter(), new BlizzardNovaEffect())));

            // Test Wizard with Chain Lightning Lv5 + Overcharge Core -> Gigastorm Lightning
            var wizard = PlayerClassFactory.CreatePlayer(2, CharacterClassType.Wizard, Vector2D.Zero, _eventBus);
            var cl = new CompositeSkill("chain_lightning", "Chain Lightning", new CooldownTrigger(2f), new ClosestEnemyTargeter(), new ChainLightningEffect());
            while (!cl.IsMaxLevel) cl.LevelUp();
            wizard.AddSkill(cl);
            wizard.AddPassive("passive_overcharge");

            var wizardEvos = evoManager.GetAvailableEvolutions(wizard);
            Assert.That(wizardEvos.Count, Is.EqualTo(1));
            Assert.That(wizardEvos[0].EvolvedSkillId, Is.EqualTo("gigastorm_lightning"));

            bool success = evoManager.EvolveSkill(wizard, wizardEvos[0]);
            Assert.That(success, Is.True);
            Assert.That(wizard.Skills.Any(s => s.Id == "gigastorm_lightning"), Is.True);
            Assert.That(wizard.Skills.Any(s => s.Id == "chain_lightning"), Is.False);
        }
    }
}
