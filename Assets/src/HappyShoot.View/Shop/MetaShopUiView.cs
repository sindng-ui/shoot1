using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Meta;

namespace HappyShoot.View.Shop
{
    /// <summary>
    /// PlayerPrefs storage implementation for saving meta progression data in Unity.
    /// </summary>
    public class JsonPlayerPrefsStorage : ISaveStorage
    {
        private const string SaveKey = "HappyShoot_MetaSave";

        public MetaUpgradeSaveData Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                try
                {
                    return JsonUtility.FromJson<MetaUpgradeSaveData>(json);
                }
                catch
                {
                    // Fallback to fresh data if corrupted
                }
            }
            return new MetaUpgradeSaveData();
        }

        public void Save(MetaUpgradeSaveData data)
        {
            if (data == null) return;
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Unity UI View for the Lobby Meta Shop (Upgrades, Gold Display, Refunds).
    /// </summary>
    public class MetaShopUiView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text _totalGoldText;
        [SerializeField] private Button _refundAllButton;

        private MetaShopManager _shopManager;

        public MetaShopManager ShopManager => _shopManager;

        private void Awake()
        {
            var storage = new JsonPlayerPrefsStorage();
            _shopManager = new MetaShopManager(storage);
            _shopManager.OnShopStateChanged += RefreshUI;

            if (_refundAllButton != null)
            {
                _refundAllButton.onClick.AddListener(OnRefundAllClicked);
            }

            RefreshUI();
        }

        public void RefreshUI()
        {
            if (_totalGoldText != null && _shopManager != null)
            {
                _totalGoldText.text = $"Gold: {_shopManager.TotalGold:N0}";
            }
        }

        public void TryBuyUpgrade(string upgradeId)
        {
            if (_shopManager != null)
            {
                bool success = _shopManager.TryPurchaseUpgrade(upgradeId);
                Debug.Log($"[MetaShopUiView] Purchase {upgradeId}: {success}");
            }
        }

        private void OnRefundAllClicked()
        {
            if (_shopManager != null)
            {
                _shopManager.RefundAll();
                Debug.Log("[MetaShopUiView] All upgrades refunded 100%!");
            }
        }

        private void OnDestroy()
        {
            if (_shopManager != null)
            {
                _shopManager.OnShopStateChanged -= RefreshUI;
            }
        }
    }
}
