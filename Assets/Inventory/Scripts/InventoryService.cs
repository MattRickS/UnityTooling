namespace Inventory
{
    public class InventoryService : GameServices.IGameService
    {
        public Catalog catalog { get; private set; }
        // public List<ItemDelta> deltas;
        // public InventoryFactory factory { get; private set; }

        // TODO: Could be deltaID or itemID
        public ItemData GetItemData(string id)
        {
            return catalog.GetItem(id);
        }
        public int GetItemStatistic(string id, Statistic stat)
        {
            ItemData data = GetItemData(id);
            // TODO: Apply any delta
            return data.GetStat(stat);
        }

        public void Save(string fileName) { }
        public void Load(string fileName) { }
    }
}