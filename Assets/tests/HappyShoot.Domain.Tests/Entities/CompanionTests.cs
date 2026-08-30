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
        public void CompanionEntity_InitializesWithClassDefaultSkill()
        {
            var player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero);
            var warrior = new CompanionEntity(CompanionType.Warrior, player, Vector2D.Zero);
            var ranger = new CompanionEntity(CompanionType.Ranger, player, Vector2D.Zero);

            Assert.That(warrior.Skills.Count, Is.EqualTo(1));
            Assert.That(warrior.Skills[0].SkillId, Is.EqualTo("slash"));
            Assert.That(warrior.Skills[0].Level, Is.EqualTo(1));

            Assert.That(ranger.Skills.Count, Is.EqualTo(1));
            Assert.That(ranger.Skills[0].SkillId, Is.EqualTo("bow"));
            Assert.That(ranger.Skills[0].Level, Is.EqualTo(1));
        }

        [Test]
        public void CompanionEntity_LearnsNewSkillRandomly_UntilMaxActivePool()
        {
            var player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero);
            var warrior = new CompanionEntity(CompanionType.Warrior, player, Vector2D.Zero);

            // Warrior starts with slash (1 skill)
            Assert.That(warrior.Skills.Count, Is.EqualTo(1));

            // Learns 2nd skill
            bool learned2 = warrior.LearnNewSkillRandomly();
            Assert.That(learned2, Is.True);
            Assert.That(warrior.Skills.Count, Is.EqualTo(2));

            // Learns 3rd skill
            bool learned3 = warrior.LearnNewSkillRandomly();
            Assert.That(learned3, Is.True);
            Assert.That(warrior.Skills.Count, Is.EqualTo(3));

            // Pool is full (slash, ground_stomp, whirlwind) -> Learning again upgrades a skill
            bool upgradedInstead = warrior.LearnNewSkillRandomly();
            Assert.That(upgradedInstead, Is.True);
            Assert.That(warrior.Skills.Count, Is.EqualTo(3)); // count doesn't increase beyond 3
        }

        [Test]
        public void CompanionEntity_LevelsUpRandomSkill_Correctly()
        {
            var player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero);
            var ranger = new CompanionEntity(CompanionType.Ranger, player, Vector2D.Zero);

            Assert.That(ranger.Skills[0].Level, Is.EqualTo(1));

            bool leveledUp = ranger.LevelUpRandomSkill();
            Assert.That(leveledUp, Is.True);
            Assert.That(ranger.Skills[0].Level, Is.EqualTo(2));
        }

        [Test]
        public void CompanionEntity_ReceivesOneThirdOfPlayerPassiveBonuses()
        {
            var player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero);

            // Give player +30% Attack Power (Total 1.30 AP)
            player.Stats = new CharacterStats(
                player.Stats.MaxHealth,
                player.Stats.HealthRegen,
                player.Stats.MoveSpeed,
                attackPowerMultiplier: 1.30f,
                player.Stats.Armor,
                player.Stats.CritChance,
                player.Stats.CritDamageMultiplier,
                cooldownReduction: 0.30f, // 30% CDR
                areaMultiplier: 1.30f,    // 30% Area
                player.Stats.ProjectileSpeedMultiplier,
                player.Stats.ExtraProjectiles,
                player.Stats.PickupRadius
            );

            var companion = new CompanionEntity(CompanionType.Warrior, player, Vector2D.Zero);

            // Companion receives 1/3 of player's +30% bonus -> +10% bonus (Total AP 1.10)
            float expectedAp = 1.0f + (0.30f * (1f / 3f));
            Assert.That(companion.GetEffectiveAttackPowerMultiplier(), Is.EqualTo(expectedAp).Within(0.001f));

            // CDR 1/3 of 30% -> 10% CDR
            float expectedCdr = 0.30f * (1f / 3f);
            Assert.That(companion.GetEffectiveCooldownReduction(), Is.EqualTo(expectedCdr).Within(0.001f));

            // Area 1/3 of 30% -> +10% Area (Total 1.10)
            float expectedArea = 1.0f + (0.30f * (1f / 3f));
            Assert.That(companion.GetEffectiveAreaMultiplier(), Is.EqualTo(expectedArea).Within(0.001f));
        }

        [Test]
        public void CompanionEntity_FinalDamage_AppliesOneThirdMultiplierOnEffectiveAP()
        {
            var player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero);
            player.Stats = new CharacterStats(
                player.Stats.MaxHealth, player.Stats.HealthRegen, player.Stats.MoveSpeed,
                attackPowerMultiplier: 1.30f, // +30% AP
                player.Stats.Armor, player.Stats.CritChance, player.Stats.CritDamageMultiplier,
                player.Stats.CooldownReduction, player.Stats.AreaMultiplier,
                player.Stats.ProjectileSpeedMultiplier, player.Stats.ExtraProjectiles, player.Stats.PickupRadius
            );

            var companion = new CompanionEntity(CompanionType.Ranger, player, Vector2D.Zero);

            float baseDamage = 60f;
            // Effective AP = 1.0 + 0.10 = 1.10
            // Final Damage = 60 * 1.10 * (1/3) = 22
            float expectedDamage = 60f * 1.10f * (1f / 3f);
            float actualDamage = companion.CalculateFinalDamage(baseDamage);

            Assert.That(actualDamage, Is.EqualTo(expectedDamage).Within(0.001f));
        }
    }
}
