using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Leveling
{
    [TestFixture]
    public class PassiveItemsTests
    {
        private PlayerEntity _player;
        private SkillRewardManager _rewardManager;

        [SetUp]
        public void SetUp()
        {
            _player = new PlayerEntity(1, CharacterStats.Default, Vector2D.Zero);
            _rewardManager = new SkillRewardManager();

            // Register 6 standard passives
            _rewardManager.RegisterPassive("passive_fang", "Vampire Fang", "+15% Damage", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, 1.0f + 0.15f * lv, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            _rewardManager.RegisterPassive("passive_feather", "Wind Feather", "+12% Speed", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, 5.0f * (1.0f + 0.12f * lv), s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            _rewardManager.RegisterPassive("passive_rune", "Mana Rune", "+10% CDR", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, 0.10f * lv, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            _rewardManager.RegisterPassive("passive_armor", "Iron Armor", "+5 Armor", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, 5f * lv, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            _rewardManager.RegisterPassive("passive_ring", "Golden Ring", "+1.5m Magnet", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, 2.0f + 1.5f * lv);
            });

            _rewardManager.RegisterPassive("passive_heart", "Heart Pendant", "+30 Max HP & +1.5 HP/s Regen", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(100f + 30f * lv, 1.5f * lv, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });
        }

        [Test]
        public void ApplyPassive_IncreasesPlayerPassiveLevelAndAugmentsStats()
        {
            Assert.That(_player.GetPassiveLevel("passive_fang"), Is.EqualTo(0));

            var rewards = _rewardManager.RollRewards(_player, count: 6);
            var fangOption = rewards.Find(r => r.Id == "passive_fang");
            Assert.That(fangOption, Is.Not.Null);

            _rewardManager.ApplyReward(_player, fangOption);

            Assert.That(_player.GetPassiveLevel("passive_fang"), Is.EqualTo(1));
            Assert.That(_player.Stats.AttackPowerMultiplier, Is.EqualTo(1.15f).Within(0.01f));
        }

        [Test]
        public void MaxLevelPassive_IsExcludedFromCandidatePool()
        {
            // Upgrade feather to max level (5)
            for (int i = 0; i < 5; i++)
            {
                var rewards = _rewardManager.RollRewards(_player, count: 10);
                var feather = rewards.Find(r => r.Id == "passive_feather");
                Assert.That(feather, Is.Not.Null);
                _rewardManager.ApplyReward(_player, feather);
            }

            Assert.That(_player.GetPassiveLevel("passive_feather"), Is.EqualTo(5));

            // On next roll, passive_feather must not appear
            var nextRewards = _rewardManager.RollRewards(_player, count: 10);
            var found = nextRewards.Find(r => r.Id == "passive_feather");
            Assert.That(found, Is.Null);
        }
    }
}
