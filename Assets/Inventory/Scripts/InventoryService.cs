using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    /*
    Interface for managing items and inventories.

    ItemData and Catalogs are static data that can be freely referenced and shared.
    ModifiedItems and Inventories are stateful, so are tracked by the InventoryService
    and referenced via IDs. This ensures the (de)serialization is centralised to avoid
    data becoming duplicated.
    */
    [Serializable]
    public class InventoryService : GameServices.IGameService, ISerializationCallbackReceiver
    {
        public string saveName = "InventoryService";
        public Catalog catalog;
        public List<ModifiedItem> modifiedItems = new List<ModifiedItem>();
        public List<Inventory> inventories = new List<Inventory>();

        private Dictionary<string, ModifiedItem> modifiedItemMapping = new Dictionary<string, ModifiedItem>();
        private Dictionary<string, Inventory> inventoryMapping = new Dictionary<string, Inventory>();

        public InventoryService() { }
        public InventoryService(Catalog catalog) { this.catalog = catalog; }

        private ModifiedItem CreateModifiedItem(string itemID)
        {
            ModifiedItem modifiedItem = new ModifiedItem(itemID);
            modifiedItems.Add(modifiedItem);
            modifiedItemMapping.Add(modifiedItem.Id(), modifiedItem);
            return modifiedItem;
        }

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
        public string CreateModifiedItemID(string itemID)
        {
            return CreateModifiedItem(itemID).Id();
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
        public bool IsModifiedItemID(string id) { return modifiedItemMapping.ContainsKey(id); }

        // Inventories
        public Inventory CreateInventory(int size)
        {
            Inventory inventory = new Inventory(size);
            inventories.Add(inventory);
            inventoryMapping.Add(inventory.Id(), inventory);
            return inventory;
        }
        public Inventory GetInventory(string inventoryID) { return inventoryMapping[inventoryID]; }

        // Serialization
        public bool Save()
        {
            // TODO: This will save the catalog into the JSON? Uneeded though.
            //       Could remove from the before serialisation?
            string json = JsonUtility.ToJson(this);
            return SaveManager.SaveJSON(saveName, json);
        }
        public bool Load()
        {
            if (!SaveManager.SaveExists(saveName))
            {
                return false;
            }
            string json;
            if (!SaveManager.LoadJSON(saveName, out json))
            {
                return false;
            }
            JsonUtility.FromJsonOverwrite(json, this);
            return true;
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            modifiedItemMapping = new Dictionary<string, ModifiedItem>();
            foreach (ModifiedItem modifiedItem in modifiedItems)
            {
                modifiedItemMapping.Add(modifiedItem.Id(), modifiedItem);
            }

            inventoryMapping = new Dictionary<string, Inventory>();
            foreach (Inventory inventory in inventories)
            {
                inventoryMapping.Add(inventory.Id(), inventory);
            }
        }
    }
}