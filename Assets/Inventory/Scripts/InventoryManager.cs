using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    /*
    Interface for managing items and inventories.

    Inventories are stateful, so are tracked by the InventoryManager and referenced via
    IDs. This ensures the (de)serialization is centralised to avoid data becoming
    duplicated.

    TODO: Consider using GetInstanceID() for all IDs. Integers would be much more
    efficient at the cost of readibility.
    */
    [Serializable]
    public class InventoryManager : ISerializationCallbackReceiver
    {
        [SerializeField] private ItemManager itemManager;
        [SerializeField] private List<Inventory> inventories = new List<Inventory>();
        private Dictionary<string, Inventory> inventoryMapping = new Dictionary<string, Inventory>();

        public InventoryManager() { }
        public InventoryManager(ItemManager itemManager) { this.itemManager = itemManager; }

        public ItemManager ItemManager { get { return itemManager; } }

        private Inventory GetInventoryByID(string inventoryID)
        {
            return inventoryMapping[inventoryID];
        }

        // Inventories
        public string CreateInventory(uint size)
        {
            Inventory inventory = new Inventory(itemManager, size);
            inventories.Add(inventory);
            inventoryMapping.Add(inventory.Id(), inventory);
            return inventory.Id();
        }
        public bool IsValidInventoryID(string inventoryID)
        {
            return !string.IsNullOrEmpty(inventoryID) && inventoryMapping.ContainsKey(inventoryID);
        }
        public int NumInventories() { return inventories.Count; }

        // Item Manipulation
        public bool HasCapacity(string inventoryID, string itemID, int quantity = 1)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.HasCapacity(itemID, quantity);
        }
        public bool HasCapacity(string inventoryID, Dictionary<string, int> itemQuantities)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.HasCapacity(itemQuantities);
        }
        public int AddItemToInventory(string inventoryID, string itemID, int quantity = 1)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.AddItem(itemID, quantity: quantity);
        }

        // Serialization
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            inventoryMapping = new Dictionary<string, Inventory>();
            foreach (Inventory inventory in inventories)
            {
                inventory.SetItemManager(itemManager);
                inventoryMapping.Add(inventory.Id(), inventory);
            }
        }
    }
}