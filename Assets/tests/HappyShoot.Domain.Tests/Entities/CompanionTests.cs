using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Entities
{
    [TestFixture]
    public class CompanionTests
    {
        [Test]
        public void ClearCount_UnlocksCompanions_Progressively()
        {
            var data = new SkillTreeSaveData();
            Assert.That(data.ClearCount, Is.EqualTo(0));
            Assert.That(data.IsWarriorUnlocked, Is.False);
            Assert.That(data.IsRangerUnlocked, Is.False);

            // 1st Clear: Warrior Unlocks
            data.IncrementClearCount();
            Assert.That(data.ClearCount, Is.EqualTo(1));
            Assert.That(data.IsWarriorUnlocked, Is.True);
            Assert.That(data.IsRangerUnlocked, Is.False);

            // 2nd Clear: Ranger Unlocks
            data.IncrementClearCount();
            Assert.That(data.ClearCount, Is.EqualTo(2));
            Assert.That(data.IsWarriorUnlocked, Is.True);
            Assert.That(data.IsRangerUnlocked, Is.True);
        }

        [Test]
        public void CompanionEntity_ScalesDamageAtOneThirdMultiplier()
        {
            var player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero);
            var companion = new CompanionEntity(CompanionType.Warrior, player, Vector2D.Zero);

            float baseDamage = 30f;
            float expectedDamage = baseDamage * player.Stats.AttackPowerMultiplier * CompanionEntity.DamageMultiplier;
            float actualDamage = companion.CalculateDamage(baseDamage);

            Assert.That(actualDamage, Is.EqualTo(expectedDamage).Within(0.001f));
        }

        [Test]
        public void CompanionEntity_Cooldown_SyncsWithPlayerCdr()
        {
            var player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero);
            var companion = new CompanionEntity(CompanionType.Ranger, player, Vector2D.Zero);

            Assert.That(companion.CanAttack, Is.True);
            companion.TriggerAttack();
            Assert.That(companion.CanAttack, Is.False);

            // Cooldown ticks down
            companion.Update(companion.BaseCooldown + 0.1f);
            Assert.That(companion.CanAttack, Is.True);
        }
    }
}
