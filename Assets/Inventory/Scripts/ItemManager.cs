using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    /// <summary>Interface for managing items. ItemData and Catalogs are static data
    /// that can be freely referenced and shared. ModifiedItems are stateful, so are
    /// tracked by the ItemManager and referenced via IDs. This ensures the
    /// (de)serialization is centralised to avoid data becoming duplicated.</summary>
    [Serializable]
    public class ItemManager : ISerializationCallbackReceiver
    {
        public Catalog catalog;
        public List<ModifiedItem> modifiedItems = new List<ModifiedItem>();

        private Dictionary<string, ModifiedItem> modifiedItemMapping = new Dictionary<string, ModifiedItem>();

        public ItemManager() { }
        public ItemManager(Catalog catalog) { this.catalog = catalog; }

        private ModifiedItem CreateModifiedItem(string itemID, string newItemID = null)
        {
            if (!IsStaticItemID(itemID)) { throw new KeyNotFoundException($"{itemID} is not a static item"); }
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

        public int NumModifiedItems() { return modifiedItems.Count; }
        public int NumStaticItems() { return catalog.NumItems(); }
        public bool IsValidID(string itemID) { return IsStaticItemID(itemID) || IsModifiedItemID(itemID); }
        public bool IsModifiedItemID(string itemID) { return !string.IsNullOrEmpty(itemID) && modifiedItemMapping.ContainsKey(itemID); }
        public bool IsStaticItemID(string itemID) { return catalog.IsValidID(itemID); }

        // Item Data
        /// <summary>Retrieves the <see cref="ItemData"/> for a given ID, which may be
        /// an itemID or modifiedItemID</summary>
        public ItemData GetItemData(string itemID)
        {
            if (string.IsNullOrEmpty(itemID)) { return null; }
            ModifiedItem modifiedItem;
            if (modifiedItemMapping.TryGetValue(itemID, out modifiedItem))
            {
                itemID = modifiedItem.itemID;
            }
            return catalog.GetItemData(itemID);
        }
        /// <summary>Retrieves the statistic value for the ID. If the ID belongs to a
        /// <see cref="ModifiedItem"/>, it combines the data and delta value.</summary>
        public int GetItemStatisticValue(string itemID, Statistic stat)
        {
            if (!IsValidID(itemID)) { throw new KeyNotFoundException($"{itemID} is not a an existing id"); }
            ModifiedItem modItem;
            int value = 0;
            if (modifiedItemMapping.TryGetValue(itemID, out modItem))
            {
                itemID = modItem.itemID;
                value += modItem.GetStatDelta(stat);
            }
            ItemData data = GetItemData(itemID);
            value += data.GetStat(stat);
            return value;
        }
        /// <summary>Retrieves the statistic's delta value for the <see cref="ModifiedItem"/>.
        /// Defaults to 0 if delta is not set. Throws KeyNotFoundException if not a modified ID.</summary>
        public int GetItemStatisticDeltaValue(string itemID, Statistic stat)
        {
            if (!IsModifiedItemID(itemID)) { throw new KeyNotFoundException($"{itemID} is not a modified id"); }
            ModifiedItem modItem;
            if (modifiedItemMapping.TryGetValue(itemID, out modItem))
            {
                itemID = modItem.itemID;
                return modItem.GetStatDelta(stat);
            }
            return 0;
        }

        /// <summary>Creates a new <see cref="ModifiedItem"/> instance from a static itemID.
        /// Throws a KeyNotFoundException if the itemID is not an existing static ID.
        /// The returned <see cref="ModifiedItem"/> ID has no delta values when returned.</summary>
        public string CreateModifiedItemID(string itemID, string newItemID = null)
        {
            return CreateModifiedItem(itemID, newItemID).Id();
        }
        /// <summary>Destroys an existing <see cref="ModifiedItem"/> and returns true. Returns
        /// false for any other itemID.</summary>
        public bool DestroyModifiedItemID(string itemID)
        {
            if (string.IsNullOrEmpty(itemID)) { return false; }
            ModifiedItem modifiedItem;
            if (!modifiedItemMapping.TryGetValue(itemID, out modifiedItem))
                return false;
            modifiedItemMapping.Remove(itemID);
            modifiedItems.Remove(modifiedItem);
            return true;
        }
        /// <summary>Sets the delta value for a <see cref="ModifiedItem"/>. If the given
        /// itemID is a static itemID, a new <see cref="ModifiedItem"/> instance is created.
        /// Returns the itemID of the <see cref="ModifiedItem"/> instance.<summary/>
        public string SetItemStatisticDeltaValue(string itemID, Statistic statistic, int value)
        {
            if (string.IsNullOrEmpty(itemID)) { throw new KeyNotFoundException($"{itemID} is not a valid ID"); }
            ModifiedItem modItem;
            if (!modifiedItemMapping.TryGetValue(itemID, out modItem))
            {
                // Throws if not a valid static ID
                modItem = CreateModifiedItem(itemID);
            }
            modItem.SetStatDelta(statistic, value);
            return modItem.Id();
        }
        /// <summary>Increments/Decrements the delta value for an existing modified item.
        /// If the modified item exists but no value is set, the modifier is set as the
        /// value. Throws a KeyNotFoundException if no modified item exists for the
        /// given ID. Returns the resulting delta value.</summary>
        public int ModifyItemStatisticDeltaValue(string itemID, Statistic statistic, int modifier)
        {
            if (!IsModifiedItemID(itemID)) { throw new KeyNotFoundException($"{itemID} is not a modified id"); }
            ModifiedItem modItem = modifiedItemMapping[itemID];
            return modItem.ModifyStatDelta(statistic, modifier);
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