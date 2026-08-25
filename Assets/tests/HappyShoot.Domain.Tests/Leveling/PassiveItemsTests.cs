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

            _rewardManager.RegisterPassive("passive_crit", "Hawk's Eye", "+8% Crit & +5% Crit Dmg", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, 0.10f + 0.08f * lv, 1.50f + 0.05f * lv, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });
        }

        [Test]
        public void ApplyPassive_IncreasesPlayerPassiveLevelAndAugmentsStats()
        {
            Assert.That(_player.GetPassiveLevel("passive_fang"), Is.EqualTo(0));

            var rewards = _rewardManager.RollRewards(_player, count: 7);
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

        [Test]
        public void ApplyPassiveCrit_IncreasesCritChanceAndMultiplierCorrectly()
        {
            Assert.That(_player.GetPassiveLevel("passive_crit"), Is.EqualTo(0));
            Assert.That(_player.Stats.CritChance, Is.EqualTo(0.10f).Within(0.001f));
            Assert.That(_player.Stats.CritDamageMultiplier, Is.EqualTo(1.50f).Within(0.001f));

            var rewards = _rewardManager.RollRewards(_player, count: 10);
            var critOption = rewards.Find(r => r.Id == "passive_crit");
            Assert.That(critOption, Is.Not.Null);

            _rewardManager.ApplyReward(_player, critOption);

            Assert.That(_player.GetPassiveLevel("passive_crit"), Is.EqualTo(1));
            Assert.That(_player.Stats.CritChance, Is.EqualTo(0.18f).Within(0.001f)); // 0.10 + 0.08
            Assert.That(_player.Stats.CritDamageMultiplier, Is.EqualTo(1.55f).Within(0.001f)); // 1.50 + 0.05
        }

        [Test]
        public void ApplyPassiveFang_IncreasesSlashSkillDamage_OnPlayer()
        {
            var grid = new SpatialGrid2D<ISpatialEntity>(cellSize: 2.0f);
            var eventBus = new EventBus();
            var warrior = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Warrior, Vector2D.Zero, eventBus);

            // Base warrior has 1.1x AttackPowerMultiplier -> BaseDamage = 11f (Slash: 35 * 1.1 = 38.5)
            var monster1 = new MonsterEntity();
            monster1.Initialize(10, "Slime", 100f, 2f, 10f, 1, 1, new Vector2D(1.5f, 0f), eventBus);
            grid.Register(monster1);

            // Execute 1 tick with Slash skill
            warrior.Update(deltaTime: 1.5f, grid);

            // Monster takes 38.5 damage (100 - 38.5 = 61.5)
            float hpAfterFirstHit = monster1.CurrentHealth;
            Assert.That(hpAfterFirstHit, Is.EqualTo(61.5f).Within(0.01f));

            // Grant Vampire Fang (+15% AttackPowerMultiplier -> 1.1 + 0.15 = 1.25x)
            _rewardManager.GrantOrUpgradePassiveDirectly(warrior, "passive_fang");
            Assert.That(warrior.Stats.AttackPowerMultiplier, Is.EqualTo(1.25f).Within(0.01f));

            // Reset slash cooldown to test second strike
            var slashSkill = warrior.GetSkill("slash") as CompositeSkill;
            (slashSkill.Trigger as HappyShoot.Domain.Skills.Triggers.CooldownTrigger).Reset();

            var monster2 = new MonsterEntity();
            monster2.Initialize(20, "Slime", 100f, 2f, 10f, 1, 1, new Vector2D(1.5f, 0f), eventBus);
            grid.Register(monster2);

            // Execute next tick -> Slash deals 35 * 1.25 = 43.75 damage!
            warrior.Update(deltaTime: 1.5f, grid);

            Assert.That(monster2.CurrentHealth, Is.EqualTo(56.25f).Within(0.01f));
        }
    }
}
