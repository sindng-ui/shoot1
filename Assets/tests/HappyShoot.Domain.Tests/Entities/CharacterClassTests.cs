using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Entities
{
    [TestFixture]
    public class CharacterClassTests
    {
        [Test]
        public void CreateWarrior_HasBonusHpAndArmorAndMeleeSkill()
        {
            var warrior = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Warrior, Vector2D.Zero);

            Assert.That(warrior.Stats.MaxHealth, Is.EqualTo(125f));
            Assert.That(warrior.Stats.Armor, Is.EqualTo(15f));
            Assert.That(warrior.Stats.CritChance, Is.EqualTo(0.10f));
            Assert.That(warrior.Skills.Count, Is.EqualTo(1));
            Assert.That(warrior.Skills[0].Id, Is.EqualTo("slash"));
        }

        [Test]
        public void CreateRanger_HasBonusSpeedAndCritAndBowSkill()
        {
            var ranger = PlayerClassFactory.CreatePlayer(2, CharacterClassType.Ranger, Vector2D.Zero);

            Assert.That(ranger.Stats.MoveSpeed, Is.EqualTo(6.0f));
            Assert.That(ranger.Stats.CritChance, Is.EqualTo(0.20f));
            Assert.That(ranger.Skills.Count, Is.EqualTo(1));
            Assert.That(ranger.Skills[0].Id, Is.EqualTo("bow"));
        }

        [Test]
        public void CreateWizard_HasCooldownReductionAndExplosionSkill()
        {
            var wizard = PlayerClassFactory.CreatePlayer(3, CharacterClassType.Wizard, Vector2D.Zero);

            Assert.That(wizard.Stats.CooldownReduction, Is.EqualTo(0.15f));
            Assert.That(wizard.Stats.AreaMultiplier, Is.EqualTo(1.2f));
            Assert.That(wizard.Stats.CritChance, Is.EqualTo(0.10f));
            Assert.That(wizard.Skills.Count, Is.EqualTo(1));
            Assert.That(wizard.Skills[0].Id, Is.EqualTo("fireball"));
        }

        [Test]
        public void CreateWizard_WithFrostNova_HasFrostNovaSkill()
        {
            var wizard = PlayerClassFactory.CreatePlayer(3, CharacterClassType.Wizard, Vector2D.Zero, startSkillId: "frost_nova");

            Assert.That(wizard.Skills.Count, Is.EqualTo(1));
            Assert.That(wizard.Skills[0].Id, Is.EqualTo("frost_nova"));
            Assert.That(wizard.Skills[0].Name, Is.EqualTo("서리 폭발"));
        }

        [Test]
        public void CreateWizard_WithChainLightning_HasChainLightningSkill()
        {
            var wizard = PlayerClassFactory.CreatePlayer(3, CharacterClassType.Wizard, Vector2D.Zero, startSkillId: "chain_lightning");

            Assert.That(wizard.Skills.Count, Is.EqualTo(1));
            Assert.That(wizard.Skills[0].Id, Is.EqualTo("chain_lightning"));
            Assert.That(wizard.Skills[0].Name, Is.EqualTo("연쇄 번개"));
        }
    }
}
