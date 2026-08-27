using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Progression;

namespace HappyShoot.Domain.Tests.Meta
{
    [TestFixture]
    public class SkillTreeManagerTests
    {
        private MemorySkillTreeStorage _storage;
        private SkillTreeManager _mgr;

        [SetUp]
        public void SetUp()
        {
            var initialData = new SkillTreeSaveData
            {
                RubyCount = 50,
                EmeraldCount = 50,
                AmethystCount = 50
            };
            _storage = new MemorySkillTreeStorage(initialData);
            _mgr = new SkillTreeManager(_storage);
            SkillTreeRegistry.RegisterAll(_mgr);
        }

        [Test]
        public void UnlockCoreNode_Succeeds_WhenGemsAreSufficient()
        {
            // w_hp1 costs 3 Rubies
            bool success = _mgr.TryUnlockNode("w_hp1");

            Assert.That(success, Is.True);
            Assert.That(_mgr.GetGemCount(GemType.Ruby), Is.EqualTo(47));
            Assert.That(_mgr.GetNodeLevel("w_hp1"), Is.EqualTo(1));
        }

        [Test]
        public void UnlockPrerequisiteNode_Fails_WhenPrereqNotMet()
        {
            // w_hp2 requires w_hp1
            bool success = _mgr.TryUnlockNode("w_hp2");

            Assert.That(success, Is.False);
            Assert.That(_mgr.GetNodeLevel("w_hp2"), Is.EqualTo(0));

            // Now unlock w_hp1 first
            Assert.That(_mgr.TryUnlockNode("w_hp1"), Is.True);
            // Now w_hp2 should succeed
            Assert.That(_mgr.TryUnlockNode("w_hp2"), Is.True);
            Assert.That(_mgr.GetNodeLevel("w_hp2"), Is.EqualTo(1));
        }

        [Test]
        public void ExclusiveAwakening_BlocksOtherBranches()
        {
            // Warrior tree: Awaken Fire branch by unlocking w_fire1
            bool fireSuccess = _mgr.TryUnlockNode("w_fire1");
            Assert.That(fireSuccess, Is.True);
            Assert.That(_mgr.GetAwakenedBranch(CharacterClassType.Warrior), Is.EqualTo(BranchType.Fire));

            // Attempting to unlock Ice branch (w_ice1) or Lightning branch (w_elec1) MUST fail
            bool iceSuccess = _mgr.TryUnlockNode("w_ice1");
            Assert.That(iceSuccess, Is.False);
            Assert.That(_mgr.GetNodeLevel("w_ice1"), Is.EqualTo(0));

            bool elecSuccess = _mgr.TryUnlockNode("w_elec1");
            Assert.That(elecSuccess, Is.False);
            Assert.That(_mgr.GetNodeLevel("w_elec1"), Is.EqualTo(0));
        }

        [Test]
        public void GemExchange_WorksAtTwoToOneRatio()
        {
            int initialRuby = _mgr.GetGemCount(GemType.Ruby);       // 50
            int initialEmerald = _mgr.GetGemCount(GemType.Emerald); // 50

            // Exchange 2 Rubies for 1 Emerald
            bool success = _mgr.TryExchangeGems(GemType.Ruby, GemType.Emerald);

            Assert.That(success, Is.True);
            Assert.That(_mgr.GetGemCount(GemType.Ruby), Is.EqualTo(initialRuby - 2));
            Assert.That(_mgr.GetGemCount(GemType.Emerald), Is.EqualTo(initialEmerald + 1));
        }

        [Test]
        public void ResetAwakening_RefundsFiftyPercent_AndClearsBranch()
        {
            // Start with 50 rubies
            // Unlock w_fire1 (cost 5) and w_fire2 (cost 8) -> total spent = 13
            Assert.That(_mgr.TryUnlockNode("w_fire1"), Is.True);
            Assert.That(_mgr.TryUnlockNode("w_fire2"), Is.True);
            Assert.That(_mgr.GetGemCount(GemType.Ruby), Is.EqualTo(50 - 13)); // 37
            Assert.That(_mgr.GetAwakenedBranch(CharacterClassType.Warrior), Is.EqualTo(BranchType.Fire));

            // Reset awakening: 13 / 2 = 6 rubies refunded
            int refunded = _mgr.ResetAwakening(CharacterClassType.Warrior);
            Assert.That(refunded, Is.EqualTo(6));
            Assert.That(_mgr.GetGemCount(GemType.Ruby), Is.EqualTo(37 + 6)); // 43
            Assert.That(_mgr.GetAwakenedBranch(CharacterClassType.Warrior), Is.EqualTo(BranchType.None));
            Assert.That(_mgr.GetNodeLevel("w_fire1"), Is.EqualTo(0));
            Assert.That(_mgr.GetNodeLevel("w_fire2"), Is.EqualTo(0));

            // Now Ice branch can be chosen
            Assert.That(_mgr.TryUnlockNode("w_ice1"), Is.True);
            Assert.That(_mgr.GetAwakenedBranch(CharacterClassType.Warrior), Is.EqualTo(BranchType.Ice));
        }

        [Test]
        public void SkillTreeApplier_AppliesStatsAndFlagsCorrectly()
        {
            // Unlock HP and Armor
            _mgr.TryUnlockNode("w_hp1");     // +15 HP
            _mgr.TryUnlockNode("w_armor1");  // +5 Armor
            _mgr.TryUnlockNode("w_fire1");   // Fire Burn

            var baseStats = CharacterStats.Default;
            var newStats = SkillTreeApplier.ApplyStats(baseStats, _mgr, CharacterClassType.Warrior);

            Assert.That(newStats.MaxHealth, Is.EqualTo(baseStats.MaxHealth + 15f));
            Assert.That(newStats.Armor, Is.EqualTo(baseStats.Armor + 5f));

            var flags = SkillTreeApplier.BuildFlags(_mgr, CharacterClassType.Warrior);
            Assert.That(flags.WFireBurnOnHit, Is.True);
            Assert.That(flags.WFireBurnDuration, Is.EqualTo(3f));
            Assert.That(flags.WIceChillOnHit, Is.False); // Not unlocked
        }
    }
}
