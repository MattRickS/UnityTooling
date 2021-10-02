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
        private Dictionary<string, Inventory> inventoryMapping = new Dictionary<string, Inventory>();

        public List<Inventory> inventories = new List<Inventory>();

        // Inventories
        public Inventory CreateInventory(int size)
        {
            Inventory inventory = new Inventory(itemManager, size);
            inventories.Add(inventory);
            inventoryMapping.Add(inventory.Id(), inventory);
            return inventory;
        }
        public Inventory GetInventory(string inventoryID) { return inventoryMapping[inventoryID]; }

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