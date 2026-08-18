namespace HappyShoot.Domain.Meta
{
    /// <summary>
    /// Abstract storage provider for saving and loading player meta progression data.
    /// </summary>
    public interface ISaveStorage
    {
        MetaUpgradeSaveData Load();
        void Save(MetaUpgradeSaveData data);
    }

    /// <summary>
    /// In-memory storage implementation for deterministic TDD testing.
    /// </summary>
    public class MemorySaveStorage : ISaveStorage
    {
        private MetaUpgradeSaveData _cachedData;

        public MemorySaveStorage(MetaUpgradeSaveData initialData = null)
        {
            _cachedData = initialData ?? new MetaUpgradeSaveData();
        }

        public MetaUpgradeSaveData Load()
        {
            return _cachedData;
        }

        public void Save(MetaUpgradeSaveData data)
        {
            _cachedData = data;
        }
    }
}
