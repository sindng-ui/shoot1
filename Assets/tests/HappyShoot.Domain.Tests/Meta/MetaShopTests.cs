using NUnit.Framework;
using HappyShoot.Domain.Meta;

namespace HappyShoot.Domain.Tests.Meta
{
    [TestFixture]
    public class MetaShopTests
    {
        private MemorySaveStorage _storage;
        private MetaShopManager _shop;

        [SetUp]
        public void SetUp()
        {
            var initialData = new MetaUpgradeSaveData { TotalGold = 500 };
            _storage = new MemorySaveStorage(initialData);
            _shop = new MetaShopManager(_storage);
        }

        [Test]
        public void PurchaseUpgrade_Succeeds_WhenGoldIsSufficient()
        {
            // Health base cost = 100
            bool success = _shop.TryPurchaseUpgrade(MetaUpgradeApplier.UpgradeHealth);

            Assert.That(success, Is.True);
            Assert.That(_shop.TotalGold, Is.EqualTo(400));
            Assert.That(_shop.SaveData.GetLevel(MetaUpgradeApplier.UpgradeHealth), Is.EqualTo(1));
        }

        [Test]
        public void PurchaseUpgrade_Fails_WhenGoldIsInsufficient()
        {
            // Extra projectile cost = 1000, player has 500
            bool success = _shop.TryPurchaseUpgrade(MetaUpgradeApplier.UpgradeExtraProjectile);

            Assert.That(success, Is.False);
            Assert.That(_shop.TotalGold, Is.EqualTo(500));
            Assert.That(_shop.SaveData.GetLevel(MetaUpgradeApplier.UpgradeExtraProjectile), Is.EqualTo(0));
        }

        [Test]
        public void CannotPurchase_BeyondMaxLevel()
        {
            _shop.AddGold(10000); // Plenty of gold

            // Amount max level is 2
            Assert.That(_shop.TryPurchaseUpgrade(MetaUpgradeApplier.UpgradeExtraProjectile), Is.True);
            Assert.That(_shop.TryPurchaseUpgrade(MetaUpgradeApplier.UpgradeExtraProjectile), Is.True);
            
            // 3rd attempt should fail
            Assert.That(_shop.TryPurchaseUpgrade(MetaUpgradeApplier.UpgradeExtraProjectile), Is.False);
            Assert.That(_shop.SaveData.GetLevel(MetaUpgradeApplier.UpgradeExtraProjectile), Is.EqualTo(2));
        }

        [Test]
        public void RefundAll_RestoresTotalInvestedGold_AndResetsLevels()
        {
            // Start with 1000 gold
            _shop.AddGold(500); // 1000 total

            _shop.TryPurchaseUpgrade(MetaUpgradeApplier.UpgradeHealth); // cost 100 -> rem 900
            _shop.TryPurchaseUpgrade(MetaUpgradeApplier.UpgradeArmor);  // cost 150 -> rem 750

            Assert.That(_shop.TotalGold, Is.EqualTo(750));
            Assert.That(_shop.SaveData.GetLevel(MetaUpgradeApplier.UpgradeHealth), Is.EqualTo(1));
            Assert.That(_shop.SaveData.GetLevel(MetaUpgradeApplier.UpgradeArmor), Is.EqualTo(1));

            // Refund
            _shop.RefundAll();

            Assert.That(_shop.TotalGold, Is.EqualTo(1000));
            Assert.That(_shop.SaveData.GetLevel(MetaUpgradeApplier.UpgradeHealth), Is.EqualTo(0));
            Assert.That(_shop.SaveData.GetLevel(MetaUpgradeApplier.UpgradeArmor), Is.EqualTo(0));
        }

        [Test]
        public void AddGold_PersistsCorrectly()
        {
            _shop.AddGold(250);
            Assert.That(_shop.TotalGold, Is.EqualTo(750));

            // Reload from storage
            var newShop = new MetaShopManager(_storage);
            Assert.That(newShop.TotalGold, Is.EqualTo(750));
        }
    }
}
