using System;


namespace Inventory
{
    /*
    Represents an item with modifications. Modifications are to be applied on
    top of the ItemData, eg:
        data = ItemData(itemID, statistics={Attack: 10})
        modifiedItem = ModifiedItem(itemID, statistics={Attack: -1})
        value = data.GetStat(Attack) + modifiedItem.GetStatDelta(Attack)
    This is the same value retrieved from:
        InventoryService.GetItemStatistic(modifiedItemID)
    */
    [Serializable]
    public class ModifiedItem
    {
        public string modifiedItemID = System.Guid.NewGuid().ToString();
        public string itemID;
        public SerializableDictionary<Statistic, int> statistics = new SerializableDictionary<Statistic, int>();

        public ModifiedItem(string itemID)
        {
            this.itemID = itemID;
            // For debugging purposes, use a unique readable ID
            this.modifiedItemID = $"{this.itemID}.{System.Guid.NewGuid().ToString()}";
        }
        public string Id() { return modifiedItemID; }

        public int GetStatDelta(Statistic stat)
        {
            int value;
            if (statistics.TryGetValue(stat, out value))
            {
                return value;
            }
            return 0;
        }
        public void SetStatDelta(Statistic stat, int value)
        {
            statistics[stat] = value;
        }
    }
}
