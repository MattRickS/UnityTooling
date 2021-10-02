using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    /*
    Interface for managing items.

    ItemData and Catalogs are static data that can be freely referenced and shared.
    ModifiedItems are stateful, so are tracked by the ItemManager and referenced
    via IDs. This ensures the (de)serialization is centralised to avoid data becoming
    duplicated.
    */
    [Serializable]
    public class ItemManager : ISerializationCallbackReceiver
    {
        public Catalog catalog;
        public List<ModifiedItem> modifiedItems = new List<ModifiedItem>();

        private Dictionary<string, ModifiedItem> modifiedItemMapping = new Dictionary<string, ModifiedItem>();

        public ItemManager() { }
        public ItemManager(Catalog catalog) { this.catalog = catalog; }

        private void ValidateID(string itemID)
        {
            if (!IsValidID(itemID)) { throw new KeyNotFoundException(itemID); }
        }
        private ModifiedItem CreateModifiedItem(string itemID, string newItemID = null)
        {
            ValidateID(itemID);
            ModifiedItem modifiedItem;
            if (string.IsNullOrEmpty(newItemID))
            {
                modifiedItem = new ModifiedItem(itemID);
            }
            else
            {
                // If the ID is already in use, throw an error
                if (IsValidID(newItemID)) { throw new SystemException(); }
                modifiedItem = new ModifiedItem(itemID, newItemID);
            }
            modifiedItems.Add(modifiedItem);
            modifiedItemMapping.Add(modifiedItem.Id(), modifiedItem);
            return modifiedItem;
        }

        public int NumModifiedIDs() { return modifiedItems.Count; }
        public int NumStaticItems() { return catalog.NumItems(); }
        public bool IsValidID(string id)
        {
            return IsStaticItemID(id) || IsModifiedItemID(id);
        }
        public bool IsModifiedItemID(string id) { return !string.IsNullOrEmpty(id) && modifiedItemMapping.ContainsKey(id); }
        public bool IsStaticItemID(string id) { return catalog.IsValidID(id); }

        // Item Data
        /*
        Retrieves the ItemData for a given ID, which may be an itemID or modifiedItemID
        */
        public ItemData GetItemData(string id)
        {
            ModifiedItem modifiedItem;
            if (modifiedItemMapping.TryGetValue(id, out modifiedItem))
            {
                id = modifiedItem.itemID;
            }
            return catalog.GetItemData(id);
        }
        /*
        Retrieves the statistic value for the ID. If the ID belongs to a modified item,
        it combines the data and delta value.
        */
        public int GetItemStatisticValue(string id, Statistic stat)
        {
            ModifiedItem modItem;
            int value = 0;
            if (modifiedItemMapping.TryGetValue(id, out modItem))
            {
                id = modItem.itemID;
                value += modItem.GetStatDelta(stat);
            }
            ItemData data = GetItemData(id);
            value += data.GetStat(stat);
            return value;
        }
        /*
        Retrieves the statistic's modified value for the ID. Defaults to 0.
        */
        public int GetItemStatisticDeltaValue(string id, Statistic stat)
        {
            ModifiedItem modItem;
            if (modifiedItemMapping.TryGetValue(id, out modItem))
            {
                id = modItem.itemID;
                return modItem.GetStatDelta(stat);
            }
            return 0;
        }

        /*
        Creates a new modified item instance to be tracked by the inventory.
        The returned ModifiedItem has no stat deltas when returned.
        */
        public string CreateModifiedItemID(string itemID, string newItemID = null)
        {
            return CreateModifiedItem(itemID, newItemID).Id();
        }
        /*
        Modifies the delta for an itemID. If the given itemID was not currently a
        tracked instance, a new instance is created and the ID returned.
        */
        public string ModifyItemDelta(string itemID, Statistic statistic, int value)
        {
            ModifiedItem modItem;
            if (!modifiedItemMapping.TryGetValue(itemID, out modItem))
            {
                modItem = CreateModifiedItem(itemID);
            }
            modItem.SetStatDelta(statistic, value);
            return modItem.Id();
        }

        // Serialization
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            modifiedItemMapping = new Dictionary<string, ModifiedItem>();
            foreach (ModifiedItem modifiedItem in modifiedItems)
            {
                modifiedItemMapping.Add(modifiedItem.Id(), modifiedItem);
            }
        }
    }
}