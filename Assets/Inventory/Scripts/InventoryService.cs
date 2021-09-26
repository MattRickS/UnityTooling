using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    [Serializable]
    public class InventoryService : GameServices.IGameService, ISerializationCallbackReceiver
    {
        public Catalog catalog;
        public List<ItemDelta> deltas = new List<ItemDelta>();
        public List<Inventory> inventories = new List<Inventory>();

        private Dictionary<string, ItemDelta> deltaMapping = new Dictionary<string, ItemDelta>();
        private Dictionary<string, Inventory> inventoryMapping = new Dictionary<string, Inventory>();

        public InventoryService() { }
        public InventoryService(Catalog catalog) { this.catalog = catalog; }

        // Item Data
        /*
        Retrieves the ItemData for a given ID, which may be an itemID or deltaID
        */
        public ItemData GetItemData(string id)
        {
            ItemDelta delta;
            if (deltaMapping.TryGetValue(id, out delta))
            {
                id = delta.itemID;
            }
            return catalog.GetItem(id);
        }
        /*
        Retrieves the statistic value for the ID. If the ID belongs to a delta,
        it combines the data and delta value.
        */
        public int GetItemStatistic(string id, Statistic stat)
        {
            ItemDelta delta;
            int value = 0;
            if (deltaMapping.TryGetValue(id, out delta))
            {
                id = delta.itemID;
                value += delta.GetStat(stat);
            }
            ItemData data = GetItemData(id);
            value += data.GetStat(stat);
            return value;
        }
        public ItemDelta CreateDelta(string itemID)
        {
            ItemDelta delta = new ItemDelta(itemID);
            deltas.Add(delta);
            deltaMapping.Add(delta.Id(), delta);
            return delta;
        }
        public bool HasDelta(string id) { return deltaMapping.ContainsKey(id); }

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
        public void Save(string fileName)
        {
            // TODO: This will save the catalog into the JSON? Uneeded though.
            //       Could remove from the before serialisation?
            string json = JsonUtility.ToJson(this);
            FileManager.WriteFile(fileName, json);
        }
        public void Load(string fileName)
        {
            string json;
            FileManager.ReadFile(fileName, out json);
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            deltaMapping = new Dictionary<string, ItemDelta>();
            foreach (ItemDelta delta in deltas)
            {
                deltaMapping.Add(delta.Id(), delta);
            }

            inventoryMapping = new Dictionary<string, Inventory>();
            foreach (Inventory inventory in inventories)
            {
                inventoryMapping.Add(inventory.Id(), inventory);
            }
        }
    }
}