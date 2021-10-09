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

        // Read State
        public Dictionary<string, int> GetInventoryItems(string inventoryID)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.Items();
        }
        public bool FindItem(string inventoryID, string itemID, out int index, int startIndex = 0)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.FindItem(itemID, out index, startIndex: startIndex);
        }
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
        /// <summary>Modified item IDs will not include inherited item IDs. Use FindItem
        /// if looking for one instance.</summary>
        public bool HasItem(string inventoryID, string itemID, int quantity)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.HasItem(itemID, quantity);
        }
        public bool HasItems(string inventoryID, Dictionary<string, int> itemQuantities)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.HasItems(itemQuantities);
        }

        // Write State
        public void ClearInventory(string inventoryID)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            inventory.Clear();
        }
        public int AddItemToInventory(string inventoryID, string itemID, int quantity = 1)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.AddItem(itemID, quantity: quantity);
        }
        public Dictionary<string, int> AddItemsToInventory(string inventoryID, Dictionary<string, int> itemQuantities)
        {
            Inventory inventory = GetInventoryByID(inventoryID);
            return inventory.AddItems(itemQuantities);
        }
        // public Dictionary<string, int> TakeItem(string inventoryID, string itemID, int quantity = 1)
        // {
        //     Inventory inventory = GetInventoryByID(inventoryID);
        //     return inventory.TakeItem(itemID, quantity);
        // }
        // public Dictionary<string, int> TakeItems(string inventoryID, Dictionary<string, int> itemQuantities)
        // {
        //     Inventory inventory = GetInventoryByID(inventoryID);
        //     return inventory.TakeItems(itemQuantities);
        // }
        // public bool SwapItems(string leftInventoryID, Dictionary<string, int> leftItemQuantities, string rightInventoryID, Dictionary<string, int> rightItemQuantities = null)
        // {
        //     // TODO: Introduce locks
        //     Inventory leftInventory = GetInventoryByID(leftInventoryID);
        //     Inventory rightInventory = GetInventoryByID(rightInventoryID);

        //     // Inventories must have the items to be swapped or we can immediately reject the request
        //     if (!leftInventory.HasItems(leftItemQuantities) || (rightItemQuantities != null && rightInventory.HasItems(rightItemQuantities)))
        //     {
        //         return false;
        //     }
        //     // TODO: Capacity(stackable=N, nonstackable=M)
        //     //   Stackables will need to know max sizes to know rollover
        //     leftItemsCapacity = leftItemQuantities.Capacity();
        //     rightSpareCapacity = CapacityFromInventory(rightInventory);
        //     if (rightItemQuantities == null)
        //     {
        //         if (!rightSpareCapacity.Fits(leftItemsCapacity))
        //             return false;
        //         leftInventory.TakeItems(leftItemQuantities);
        //         rightInventory.AddItems(leftItemQuantities);
        //     }
        //     else
        //     {
        //         rightItemsCapacity = rightItemQuantities.Capacity();
        //         if (!(rightSpareCapacity + rightItemsCapacity).Fits(leftItemsCapacity))
        //             return false;
        //         leftSpareCapacity = leftInventory.SpareCapacity();
        //         if (!(leftSpareCapacity + leftItemsCapacity).Fits(rightItemsCapacity))
        //             return false;
        //         leftInventory.TakeItems(leftItemQuantities);
        //         rightInventory.TakeItems(rightItemQuantities);

        //         leftInventory.AddItems(rightItemQuantities);
        //         rightInventory.AddItems(leftItemQuantities);
        //     }
        //     return true;
        // }


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