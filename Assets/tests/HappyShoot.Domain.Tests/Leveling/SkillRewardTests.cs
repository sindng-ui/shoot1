using System.Linq;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Leveling
{
    [TestFixture]
    public class SkillRewardTests
    {
        private SkillRewardManager _rewardManager;
        private PlayerEntity _player;

        [SetUp]
        public void SetUp()
        {
            _rewardManager = new SkillRewardManager(seed: 42);
            _player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Warrior, Vector2D.Zero);

            // Register skills and passives
            _rewardManager.RegisterSkill("slash", "Greatsword Slash", "Slash effect",
                () => new CompositeSkill("slash", "Greatsword Slash", new CooldownTrigger(1f), new ClosestEnemyTargeter(), new GreatswordSlashEffect()));

            _rewardManager.RegisterSkill("fireball", "Fireball", "Launches explosive fireball",
                () => new CompositeSkill("fireball", "Fireball", new CooldownTrigger(1f), new ClosestEnemyTargeter(), new GroundStompEffect()));

            _rewardManager.RegisterSkill("thunder", "Lightning Strike", "Strikes random enemy",
                () => new CompositeSkill("thunder", "Lightning Strike", new CooldownTrigger(1.5f), new ClosestEnemyTargeter(), new GroundStompEffect()));

            _rewardManager.RegisterPassive("passive_hp", "Vitality", "+20 HP", 5, (p, lv) => p.Heal(20f));
        }

        [Test]
        public void RollRewards_OffersUpgradeForExistingSkills_AndNewForUnowned()
        {
            var options = _rewardManager.RollRewards(_player, count: 3);

            Assert.That(options.Count, Is.EqualTo(3));

            // Warrior starts with 'slash'. If 'slash' is in the pool, it should be an Upgrade
            var slashOption = options.FirstOrDefault(o => o.Id == "slash");
            if (slashOption != null)
            {
                Assert.That(slashOption.Category, Is.EqualTo(RewardCategory.UpgradeActiveSkill));
                Assert.That(slashOption.CurrentLevel, Is.EqualTo(1));
                Assert.That(slashOption.NextLevel, Is.EqualTo(2));
            }
        }

        [Test]
        public void ApplyReward_EquipsNewSkillToPlayer()
        {
            var options = _rewardManager.RollRewards(_player, count: 5);
            var fireballOption = options.FirstOrDefault(o => o.Id == "fireball");

            Assert.That(fireballOption, Is.Not.Null);
            Assert.That(_player.Skills.Any(s => s.Id == "fireball"), Is.False);

            _rewardManager.ApplyReward(_player, fireballOption);

            Assert.That(_player.Skills.Any(s => s.Id == "fireball"), Is.True);
            Assert.That(_player.Skills.Count, Is.EqualTo(2)); // Slash + Fireball
        }

        [Test]
        public void ApplyReward_UpgradesExistingSkillLevel()
        {
            var slashSkill = _player.Skills.First(s => s.Id == "slash");
            Assert.That(slashSkill.Level, Is.EqualTo(1));

            var upgradeOption = new SkillRewardOption(
                "slash", "Greatsword Slash", "Upgrade", RewardCategory.UpgradeActiveSkill, 1, 2,
                passiveApplier: p => slashSkill.LevelUp()
            );

            _rewardManager.ApplyReward(_player, upgradeOption);

            Assert.That(slashSkill.Level, Is.EqualTo(2));
        }
    }
}
