using System.Collections.Generic;


namespace Inventory
{
    public class Item
    {
        string itemID;
        Dictionary<Statistic, int> modifiedAttrs = null;

        public Item(string itemID, Dictionary<Statistic, int> modifiedAttrs = null)
        {
            this.itemID = itemID;
            this.modifiedAttrs = modifiedAttrs;
        }
        // Could itemData generate an ID on creation and store it in the data?
        // public string Id() { return itemID; }
        public ItemData GetItemData()
        {
            return GameServices.ServiceLocator.Instance.Get<InventoryService>().GetItemData(itemID);
        }
        public bool IsModified() { return modifiedAttrs != null && !modifiedAttrs.Equals(null); }
        public bool IsModifiedStat(Statistic stat) { return IsModified() && modifiedAttrs.ContainsKey(stat); }
        public int GetStat(Statistic stat)
        {
            int value;
            if (modifiedAttrs.TryGetValue(stat, out value)) { return value; }
            return GetItemData().GetStat(stat);
        }
        public void SetStat(Statistic stat, int value)
        {
            if (modifiedAttrs == null)
            {
                modifiedAttrs = new Dictionary<Statistic, int>();
            }
            modifiedAttrs[stat] = value;
        }
        public Item copy()
        {
            Item item = new Item(itemID);
            foreach (KeyValuePair<Statistic, int> kvp in modifiedAttrs)
            {
                item.SetStat(kvp.Key, kvp.Value);
            }
            return item;
        }
    }
}
