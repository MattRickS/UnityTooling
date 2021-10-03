using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    /*
    Inventories are not ScriptableObjects as they are stateful and must be
    tracked by the InventoryManager. They can be prepopulated on the manager or
    loaded from configuration.
    */
    [Serializable]
    public class Inventory
    {
        public string inventoryID = System.Guid.NewGuid().ToString();

        // For now, dependency inject the manager and InventoryManager serialisation
        // ensures all inventories use a shared instance. Should look at making this
        // private and see if there are better serialization options.
        private ItemManager itemManager;
        [SerializeField] private List<Slot> slots;

        private ItemData GetItemData(int index) { return itemManager.GetItemData(slots[index].itemID); }
        private ItemData GetItemData(string itemID) { return itemManager.GetItemData(itemID); }

        public Inventory(ItemManager itemManager, uint size)
        {
            this.itemManager = itemManager;
            slots = new List<Slot>();
            for (int i = 0; i < size; i++)
            {
                slots.Add(new Slot());
            }
        }
        public void SetItemManager(ItemManager itemManager) { this.itemManager = itemManager; }

        public string Id() { return inventoryID; }
        public int NumSlots() { return slots.Count; }

        // Stacking
        public int StackSize(int index) { return slots[index].quantity; }
        public int MaxStackSize(int index) { return GetItemData(index).maxStackSize; }
        public bool IsStackable(int index) { return MaxStackSize(index) > 1; }

        // State
        public bool IsEmpty(int index) { return slots[index].itemID == Slot.NO_ITEM; }
        public bool IsFull(int index) { return StackSize(index) == MaxStackSize(index); }
        public bool HasItem(int index, string itemID) { return slots[index].itemID == itemID; }
        public bool HasCapacity(string itemID, int quantity = 1)
        {
            int maxStackSize = GetItemData(itemID).maxStackSize;
            int quantityPlaceable = 0;
            foreach (Slot slot in slots)
            {
                if (slot.IsEmpty() || (slot.HasExactItemID(itemID) && slot.quantity < maxStackSize))
                {
                    quantityPlaceable += (maxStackSize - slot.quantity);
                    if (quantityPlaceable >= quantity)
                        return true;
                }
            }
            return false;
        }
        public bool HasCapacity(Dictionary<string, int> itemQuantities)
        {
            Dictionary<string, int> maxStackSizes = new Dictionary<string, int>();
            Dictionary<string, int> remainingItemQuantities = new Dictionary<string, int>(itemQuantities);
            List<string> stackableItems = new List<string>();
            List<string> nonStackableItems = new List<string>();

            foreach (var itemID in itemQuantities.Keys)
            {
                ItemData itemData = GetItemData(itemID);
                // If the item is modified, aggregate it with the base itemID
                string trueItemID = itemID;
                if (itemData.Id() != itemID)
                {
                    trueItemID = itemData.Id();
                    if (remainingItemQuantities.ContainsKey(trueItemID))
                        remainingItemQuantities[trueItemID] += remainingItemQuantities[itemID];
                    else
                        remainingItemQuantities[trueItemID] = remainingItemQuantities[itemID];
                    remainingItemQuantities.Remove(itemID);
                }

                maxStackSizes[trueItemID] = itemData.maxStackSize;
                // Avoid adding the key multiple times for modified and base items
                if (itemData.maxStackSize > 1 && !stackableItems.Contains(trueItemID))
                    stackableItems.Add(trueItemID);
                else if ((itemData.maxStackSize == 1 && !nonStackableItems.Contains(trueItemID)))
                    nonStackableItems.Add(trueItemID);
            }

            List<Slot> emptySlots = new List<Slot>();
            foreach (Slot slot in slots)
            {
                if (slot.IsEmpty())
                {
                    // Non-stackable items can be immediately placed in empty slots
                    if (nonStackableItems.Count > 0)
                    {
                        string itemID = nonStackableItems[nonStackableItems.Count - 1];
                        remainingItemQuantities[itemID] -= 1;
                        if (remainingItemQuantities[itemID] == 0)
                        {
                            remainingItemQuantities.Remove(itemID);
                            if (remainingItemQuantities.Count == 0)
                                return true;
                            nonStackableItems.RemoveAt(nonStackableItems.Count - 1);
                        }
                    }
                    // Track remaining empty slots. This is to avoid a situation where
                    // we pick an abritrary stackedItem to fill the empty slot when
                    // there could be a partial stack for that item later.
                    else
                    {
                        emptySlots.Add(slot);
                    }
                }
                // Max out partial stacks
                else if (stackableItems.Contains(slot.itemID))
                {
                    int remainingQuantity = remainingItemQuantities[slot.itemID];
                    remainingQuantity -= Math.Min(remainingQuantity, maxStackSizes[slot.itemID] - slot.quantity);
                    if (remainingQuantity == 0)
                    {
                        remainingItemQuantities.Remove(slot.itemID);
                        stackableItems.Remove(slot.itemID);
                    }
                    else
                    {
                        remainingItemQuantities[slot.itemID] = remainingQuantity;
                    }
                }
            }

            int requiredSlots = 0;
            foreach (var pair in remainingItemQuantities)
                requiredSlots += (pair.Value + maxStackSizes[pair.Key] - 1) / maxStackSizes[pair.Key];

            return emptySlots.Count >= requiredSlots;
        }

        // Statistics
        public int SlotStatistic(int index, Statistic stat)
        {
            if (IsEmpty(index)) { return 0; }

            Slot slot = slots[index];
            int value = itemManager.GetItemStatisticValue(slot.itemID, stat) * (slot.quantity - slot.instanceIDs.Count);
            foreach (string instanceID in slot.instanceIDs)
            {
                value += itemManager.GetItemStatisticValue(instanceID, stat);
            }
            return value;
        }
        public int AggregateStatistic(Statistic stat)
        {
            int value = 0;
            for (int currIndex = 0; currIndex < NumSlots(); currIndex++)
            {
                if (IsEmpty(currIndex)) { continue; }

                ItemData itemData = GetItemData(currIndex);
                value += SlotStatistic(currIndex, stat);
            }
            return value;
        }
        public Dictionary<Statistic, int> AggregateStatistics(List<Statistic> stats)
        {
            Dictionary<Statistic, int> aggregates = new Dictionary<Statistic, int>();
            foreach (Statistic stat in stats)
            {
                aggregates.Add(stat, 0);
            }

            for (int currIndex = 0; currIndex < NumSlots(); currIndex++)
            {
                if (IsEmpty(currIndex)) { continue; }

                foreach (Statistic stat in stats)
                {
                    aggregates[stat] += SlotStatistic(currIndex, stat);
                }
            }
            return aggregates;
        }

        // Modification
        /*
        Adds up to `quantity` items, starting from the front of the Inventory.
        Returns the quantity that was added which may be less than the given
        quantity if there is insufficient space.
        If adding a modified item ID, only one can be added (as it should always
        be a unique instance).
        */
        public int AddItem(string itemID, int quantity = 1)
        {
            // Check if adding a modified item and determine the ItemData ID
            bool isModifiedItem = itemManager.IsModifiedItemID(itemID);
            string itemDataID = itemID;
            if (isModifiedItem)
            {
                if (quantity > 1)
                {
                    throw new Exception($"Cannot add multiples ({quantity}) of modifiedItems");
                }
                itemDataID = itemManager.GetItemData(itemID).Id();
            }

            // TODO: Populate stacks first
            int remaining = quantity;
            int maxStackSize = itemManager.GetItemData(itemID).maxStackSize;
            Slot slot;
            int n;
            for (int currIndex = 0; currIndex < NumSlots(); currIndex++)
            {
                if (IsEmpty(currIndex))
                {
                    n = Math.Min(quantity, maxStackSize);
                    slot = slots[currIndex];
                    slot.itemID = itemDataID;
                    slot.quantity = n;
                    remaining -= n;
                    if (isModifiedItem)
                    {
                        slot.instanceIDs.Add(itemID);
                    }
                }
                else if (HasItem(currIndex, itemID) && !IsFull(currIndex))
                {
                    n = Math.Min(quantity, maxStackSize - StackSize(currIndex));
                    slot = slots[currIndex];
                    slot.quantity += n;
                    remaining -= n;
                    if (isModifiedItem)
                    {
                        slot.instanceIDs.Add(itemID);
                    }
                }

                if (remaining == 0) { break; }
            }
            return quantity - remaining;
        }
        /*
        Removes up to `quantity` occurrences of the item as possible starting
        from the back of the Inventory. Returns the quantity that was removed
        which may be less than the given quantity if insufficient items exist.
        */
        public int RemoveItem(string itemID, int quantity = 1)
        {
            // Check if removing a modified item and determine the ItemData ID
            bool isModifiedItem = itemManager.IsModifiedItemID(itemID);
            string itemDataID = itemID;
            if (isModifiedItem)
            {
                if (quantity > 1)
                {
                    throw new Exception($"Cannot remove multiples ({quantity}) of modifiedItems");
                }
                itemDataID = itemManager.GetItemData(itemID).Id();
            }

            int remaining = quantity;
            Slot slot;
            int n;
            for (int currIndex = NumSlots() - 1; currIndex >= 0; currIndex--)
            {
                if (
                    IsEmpty(currIndex) || !(
                        (isModifiedItem && HasItem(currIndex, itemID)) ||
                        (!isModifiedItem && HasItem(currIndex, itemDataID))
                    )
                )
                {
                    continue;
                }

                n = Math.Min(remaining, StackSize(currIndex));
                slot = slots[currIndex];
                slot.quantity -= n;
                remaining -= n;

                // Modified instances are assumed to be top of the stack
                if (slot.instanceIDs.Count > 0)
                {
                    int numRemoveInstances = Math.Min(slot.instanceIDs.Count, n);
                    slot.instanceIDs.RemoveRange(slot.instanceIDs.Count - n, n);
                }

                if (remaining == 0)
                {
                    slot.itemID = Slot.NO_ITEM;
                    break;
                }
            }
            return quantity - remaining;
        }
        /*
        Removes the requested number of item and returns them as a list of IDs.
        IDs may or may not be modified, and duplicate IDs are returned for
        multiple non-modified items.
        */
        public List<string> TakeItems(string itemID, int quantity)
        {
            List<string> takenIDs = new List<string>();

            int remaining = quantity;
            Slot slot;
            int n;
            for (int currIndex = NumSlots() - 1; currIndex >= 0; currIndex--)
            {
                if (IsEmpty(currIndex) || !HasItem(currIndex, itemID))
                {
                    continue;
                }
                n = Math.Min(remaining, StackSize(currIndex));
                slot = slots[currIndex];
                slot.itemID = itemID;

                for (int i = 0; i < n; i++)
                {
                    // Modified instances are assumed to be top of the stack
                    if (slot.instanceIDs.Count > 0)
                    {
                        takenIDs.Add(slot.instanceIDs[slot.instanceIDs.Count - 1]);
                        slot.instanceIDs.RemoveAt(slot.instanceIDs.Count - 1);
                    }
                    else
                    {
                        takenIDs.Add(slot.itemID);
                    }
                }

                slot.quantity -= n;
                remaining -= n;

                if (remaining == 0)
                {
                    slot.itemID = Slot.NO_ITEM;
                    break;
                }
            }
            return takenIDs;
        }
        /*
        Removes the requested item and outputs the itemID which may be modified.
        Returns bool for whether the item was found or not.
        */
        public bool TakeItem(string itemID, out string takenItemID)
        {
            List<string> taken = TakeItems(itemID, 1);
            if (taken.Count > 0)
            {
                takenItemID = taken[0];
                return true;
            }
            takenItemID = "";
            return false;
        }
    }
}
