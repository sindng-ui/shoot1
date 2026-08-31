using NUnit.Framework;
using HappyShoot.Domain.Forge;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Effects;

namespace HappyShoot.Domain.Tests.Forge
{
    [TestFixture]
    public class RuneSystemTests
    {
        private RuneManager _runeManager;
        private ForgeSaveData _saveData;
        private SkillTreeSaveData _wallet;

        [SetUp]
        public void SetUp()
        {
            _saveData = new ForgeSaveData();
            _runeManager = new RuneManager(_saveData);
            RuneRegistry.RegisterAll(_runeManager);

            _wallet = new SkillTreeSaveData();
            _wallet.AddGems(GemType.Ruby, 100);
            _wallet.AddGems(GemType.Emerald, 100);
            _wallet.AddGems(GemType.Amethyst, 100);
        }

        [Test]
        public void RegisterAll_Registers12Runes()
        {
            Assert.AreEqual(12, _runeManager.Definitions.Count);
            Assert.IsNotNull(_runeManager.GetDefinition("rune_rapid"));
            Assert.IsNotNull(_runeManager.GetDefinition("rune_split"));
            Assert.IsNotNull(_runeManager.GetDefinition("rune_chaos"));
        }

        [Test]
        public void TryUnlockRune_DeductsGemsAndSetsLevel1()
        {
            int rubyBefore = _wallet.GetGems(GemType.Ruby);
            bool success = _runeManager.TryUnlockRune("rune_rapid", _wallet);

            Assert.IsTrue(success);
            Assert.IsTrue(_runeManager.IsUnlocked("rune_rapid"));
            Assert.AreEqual(1, _runeManager.GetLevel("rune_rapid"));
            Assert.AreEqual(rubyBefore - 3, _wallet.GetGems(GemType.Ruby));
        }

        [Test]
        public void TryUpgradeRune_UpgradesLevelAndScalesModifiers()
        {
            _runeManager.TryUnlockRune("rune_rapid", _wallet);
            int levelBefore = _runeManager.GetLevel("rune_rapid");

            bool upgraded = _runeManager.TryUpgradeRune("rune_rapid", _wallet);
            Assert.IsTrue(upgraded);
            Assert.AreEqual(levelBefore + 1, _runeManager.GetLevel("rune_rapid"));

            var mods = _runeManager.GetDefinition("rune_rapid").CalculateModifiers(_runeManager.GetLevel("rune_rapid"));
            Assert.Less(mods.CooldownMultiplier, 0.85f, "Cooldown multiplier should shrink with level");
        }

        [Test]
        public void EquipRune_BindsToSkillSlotAndAppliesModifiers()
        {
            _runeManager.TryUnlockRune("rune_power", _wallet);
            bool equipped = _runeManager.EquipRune("fireball", "rune_power");

            Assert.IsTrue(equipped);
            Assert.AreEqual("rune_power", _runeManager.GetEquippedRuneId("fireball"));

            var mods = _runeManager.GetModifiersForSkill("fireball");
            Assert.IsTrue(mods.IsActive);
            Assert.Greater(mods.DamageMultiplier, 1.0f);
        }

        [Test]
        public void UnequipRune_ClearsModifiersForSkill()
        {
            _runeManager.TryUnlockRune("rune_power", _wallet);
            _runeManager.EquipRune("fireball", "rune_power");
            _runeManager.UnequipRune("fireball");

            var mods = _runeManager.GetModifiersForSkill("fireball");
            Assert.IsFalse(mods.IsActive);
            Assert.AreEqual(1.0f, mods.DamageMultiplier);
        }

        [Test]
        public void CompositeSkill_IntegratesRuneModifiersInUpdate()
        {
            _runeManager.TryUnlockRune("rune_rapid", _wallet);
            var mods = _runeManager.GetDefinition("rune_rapid").CalculateModifiers(1);

            var trigger = new CooldownTrigger(2.0f);
            var skill = new CompositeSkill("fireball", "화염구", trigger, new ClosestEnemyTargeter(), new FireballEffect());
            skill.Rune = mods;

            var context = new SkillContext();
            skill.Update(0.1f, context);

            Assert.AreEqual("fireball", context.SkillId);
            Assert.IsTrue(context.ActiveRune.IsActive);
            Assert.AreEqual("rune_rapid", context.ActiveRune.RuneId);
        }
    }
}
