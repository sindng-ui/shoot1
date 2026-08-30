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
                GoldCount = 5000,
                RubyCount = 50,
                EmeraldCount = 50,
                AmethystCount = 50
            };
            _storage = new MemorySkillTreeStorage(initialData);
            _mgr = new SkillTreeManager(_storage);
            SkillTreeRegistry.RegisterAll(_mgr);
        }

        [Test]
        public void UnlockCoreNode_Succeeds_WhenGoldIsSufficient()
        {
            // m_cdr1 costs 150 Gold
            bool success = _mgr.TryUnlockNode("m_cdr1");

            Assert.That(success, Is.True);
            Assert.That(_mgr.GetGoldCount(), Is.EqualTo(5000 - 150));
            Assert.That(_mgr.GetNodeLevel("m_cdr1"), Is.EqualTo(1));
        }

        [Test]
        public void UnlockCoreNode_Fails_WhenGoldIsInsufficient()
        {
            _mgr.SaveData.GoldCount = 50; // Less than 150G

            bool success = _mgr.TryUnlockNode("m_cdr1");

            Assert.That(success, Is.False);
            Assert.That(_mgr.GetNodeLevel("m_cdr1"), Is.EqualTo(0));
        }

        [Test]
        public void UnlockPrerequisiteNode_Fails_WhenPrereqNotMet()
        {
            // m_cdr2 requires m_cdr1
            bool success = _mgr.TryUnlockNode("m_cdr2");

            Assert.That(success, Is.False);
            Assert.That(_mgr.GetNodeLevel("m_cdr2"), Is.EqualTo(0));

            // Unlock m_cdr1 first
            Assert.That(_mgr.TryUnlockNode("m_cdr1"), Is.True);
            // Now m_cdr2 should succeed
            Assert.That(_mgr.TryUnlockNode("m_cdr2"), Is.True);
            Assert.That(_mgr.GetNodeLevel("m_cdr2"), Is.EqualTo(1));
        }

        [Test]
        public void ExclusiveAwakening_BlocksOtherBranches()
        {
            // Wizard tree: Awaken Fire branch by unlocking m_fire1
            bool fireSuccess = _mgr.TryUnlockNode("m_fire1");
            Assert.That(fireSuccess, Is.True);
            Assert.That(_mgr.GetAwakenedBranch(CharacterClassType.Wizard), Is.EqualTo(BranchType.Fire));

            // Attempting to unlock Ice branch (m_ice1) or Lightning branch (m_elec1) MUST fail
            bool iceSuccess = _mgr.TryUnlockNode("m_ice1");
            Assert.That(iceSuccess, Is.False);
            Assert.That(_mgr.GetNodeLevel("m_ice1"), Is.EqualTo(0));

            bool elecSuccess = _mgr.TryUnlockNode("m_elec1");
            Assert.That(elecSuccess, Is.False);
            Assert.That(_mgr.GetNodeLevel("m_elec1"), Is.EqualTo(0));
        }

        [Test]
        public void ResetAwakening_RefundsFiftyPercentGold_AndClearsBranch()
        {
            // Start with 5000 gold
            // Unlock m_fire1 (cost 300G) and m_fire2 (cost 600G) -> total spent = 900G
            Assert.That(_mgr.TryUnlockNode("m_fire1"), Is.True);
            Assert.That(_mgr.TryUnlockNode("m_fire2"), Is.True);
            Assert.That(_mgr.GetGoldCount(), Is.EqualTo(5000 - 900)); // 4100
            Assert.That(_mgr.GetAwakenedBranch(CharacterClassType.Wizard), Is.EqualTo(BranchType.Fire));

            // Reset awakening: 900 / 2 = 450 gold refunded
            int refunded = _mgr.ResetAwakening(CharacterClassType.Wizard);
            Assert.That(refunded, Is.EqualTo(450));
            Assert.That(_mgr.GetGoldCount(), Is.EqualTo(4100 + 450)); // 4550
            Assert.That(_mgr.GetAwakenedBranch(CharacterClassType.Wizard), Is.EqualTo(BranchType.None));
            Assert.That(_mgr.GetNodeLevel("m_fire1"), Is.EqualTo(0));
            Assert.That(_mgr.GetNodeLevel("m_fire2"), Is.EqualTo(0));

            // Now Ice branch can be chosen
            Assert.That(_mgr.TryUnlockNode("m_ice1"), Is.True);
            Assert.That(_mgr.GetAwakenedBranch(CharacterClassType.Wizard), Is.EqualTo(BranchType.Ice));
        }

        [Test]
        public void SkillTreeApplier_AppliesWizardStatsAndFlagsCorrectly()
        {
            // Unlock CDR and Area
            _mgr.TryUnlockNode("m_cdr1");    // +0.05 CDR
            _mgr.TryUnlockNode("m_area1");   // +0.10 Area
            _mgr.TryUnlockNode("m_fire1");   // Fireball Dot Boost

            var baseStats = CharacterStats.Default;
            var newStats = SkillTreeApplier.ApplyStats(baseStats, _mgr, CharacterClassType.Wizard);

            Assert.That(newStats.CooldownReduction, Is.EqualTo(baseStats.CooldownReduction + 0.05f));
            Assert.That(newStats.AreaMultiplier, Is.EqualTo(baseStats.AreaMultiplier * 1.10f));

            var flags = SkillTreeApplier.BuildFlags(_mgr, CharacterClassType.Wizard);
            Assert.That(flags.MFireDotBoost, Is.True);
            Assert.That(flags.MIceSlowBoost, Is.False); // Not unlocked
        }
    }
}
