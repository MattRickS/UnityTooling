using System;
using System.Linq;
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
        public int MaxStackSize(int index) { return GetItemData(index).MaxStackSize; }
        public bool IsStackable(int index) { return MaxStackSize(index) > 1; }

        // State
        public bool IsEmpty(int index) { return slots[index].IsEmpty(); }
        public bool IsFull(int index) { return StackSize(index) == MaxStackSize(index); }
        public bool HasExactItemID(int index, string itemID) { return slots[index].HasExactItemID(itemID); }
        public bool HasCapacity(string itemID, int quantity = 1)
        {
            int maxStackSize = GetItemData(itemID).MaxStackSize;
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

                maxStackSizes[trueItemID] = itemData.MaxStackSize;
                // Avoid adding the key multiple times for modified and base items
                if (itemData.MaxStackSize > 1 && !stackableItems.Contains(trueItemID))
                    stackableItems.Add(trueItemID);
                else if ((itemData.MaxStackSize == 1 && !nonStackableItems.Contains(trueItemID)))
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
        public Dictionary<string, int> Items()
        {
            Dictionary<string, int> items = new Dictionary<string, int>();
            int itemQuantity, currentValue;
            foreach (Slot slot in slots)
            {
                if (!slot.IsEmpty())
                {
                    itemQuantity = slot.quantity - slot.instanceIDs.Count;
                    if (itemQuantity > 0)
                    {
                        items.TryGetValue(slot.itemID, out currentValue);
                        items[slot.itemID] = currentValue + itemQuantity;
                    }
                    // Modified items should be unique instances, but let's not assume
                    foreach (string modifiedItemID in slot.instanceIDs)
                    {
                        items.TryGetValue(slot.itemID, out currentValue);
                        items[modifiedItemID] = currentValue + 1;
                    }
                }
            }
            return items;
        }
        public bool FindItem(string itemID, out int index, int startIndex = 0)
        {
            for (int i = startIndex; i < slots.Count; i++)
            {
                if (slots[i].HasExactItemID(itemID))
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        public bool HasItem(string itemID, int quantity = 1)
        {
            foreach (Slot slot in slots)
            {
                if (slot.HasExactItemID(itemID))
                {
                    quantity -= slot.quantity;
                    if (quantity <= 0)
                        return true;
                }
            }
            return false;
        }
        public bool HasItems(Dictionary<string, int> itemQuantities)
        {
            if (itemQuantities.Count == 0)
                return true;

            Dictionary<string, int> remainingItems = new Dictionary<string, int>(itemQuantities);
            int value;

            void removeFoundQuantity(string itemID, int quantity, int requiredValue)
            {
                if (requiredValue > quantity)
                    remainingItems[itemID] -= quantity;
                else
                    remainingItems.Remove(itemID);
            }

            foreach (Slot slot in slots)
            {
                if (slot.IsEmpty())
                    continue;

                int quantity = slot.quantity;
                foreach (var modifiedItemID in slot.instanceIDs)
                {
                    if (remainingItems.TryGetValue(modifiedItemID, out value))
                    {
                        removeFoundQuantity(modifiedItemID, 1, value);
                        quantity -= 1;
                    }
                }

                if (remainingItems.TryGetValue(slot.itemID, out value))
                    removeFoundQuantity(slot.itemID, slot.quantity, value);

                if (remainingItems.Count == 0)
                    return true;
            }

            return false;
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
        /// <summary>Adds up to `quantity` items, filling stacks and then filling empty
        /// slots from from the front of the Inventory. Returns the quantity that was
        /// added which may be less than the given quantity if there is insufficient
        /// space. If adding a modified item ID, only one can be added (as it should
        /// always be a unique instance).</summary>
        public int AddItem(string itemID, int quantity = 1)
        {
            // Check if adding a modified item and determine the ItemData ID
            bool isModifiedItem = itemManager.IsModifiedItemID(itemID);
            string itemDataID = itemID;
            ItemData itemData = itemManager.GetItemData(itemID);
            if (isModifiedItem)
            {
                if (quantity > 1)
                {
                    throw new Exception($"Cannot add multiples ({quantity}) of modifiedItems");
                }
                itemDataID = itemData.Id();
            }

            int remaining = quantity;

            bool AddToSlot(Slot slot, int count)
            {
                slot.itemID = itemDataID;
                slot.quantity += count;
                if (isModifiedItem)
                    slot.instanceIDs.Add(itemID);
                remaining -= count;
                return remaining <= 0;
            }

            if (!itemData.IsStackable())
            {
                // Non-stackable items just fill up the first empty slots
                foreach (Slot slot in slots)
                {
                    if (slot.IsEmpty() && AddToSlot(slot, 1))
                        return remaining;
                }
            }
            else
            {
                List<Slot> emptySlots = new List<Slot>();
                foreach (Slot slot in slots)
                {
                    // Track empty slots in case existing stacks lack enough capacity
                    if (slot.IsEmpty())
                        emptySlots.Add(slot);
                    // Fill existing stacks
                    else if (slot.itemID == itemDataID && slot.quantity < itemData.MaxStackSize)
                    {
                        int toAdd = Math.Min(remaining, itemData.MaxStackSize - slot.quantity);
                        if (AddToSlot(slot, toAdd))
                            return remaining;
                    }
                }

                // Now that stack's are maxed out, fill up empty spaces;
                foreach (Slot slot in emptySlots)
                {
                    int toAdd = Math.Min(remaining, itemData.MaxStackSize);
                    if (AddToSlot(slot, toAdd))
                        return remaining;
                }
            }

            return remaining;
        }
        /// <summary>Adds multiple items into the inventory</summary>
        public Dictionary<string, int> AddItems(Dictionary<string, int> itemQuantities)
        {
            Dictionary<string, int> maxStackSizes = new Dictionary<string, int>();
            Dictionary<string, int> remainingItemQuantities = new Dictionary<string, int>(itemQuantities);
            List<string> stackableItems = new List<string>();
            List<string> nonStackableItems = new List<string>();

            List<string> modifiedItemsList;
            Dictionary<string, List<string>> modifiedItems = new Dictionary<string, List<string>>();

            // Preparation
            foreach (var itemID in itemQuantities.Keys)
            {
                ItemData itemData = GetItemData(itemID);
                // If the item is modified, aggregate it's quantity with the base itemID
                string baseItemID = itemID;
                if (itemData.Id() != itemID)
                {
                    baseItemID = itemData.Id();

                    // Map the base ID to all of it's modified instances
                    if (modifiedItems.TryGetValue(baseItemID, out modifiedItemsList))
                        modifiedItemsList.Add(itemID);
                    else
                    {
                        modifiedItemsList = new List<string>();
                        modifiedItemsList.Add(itemID);
                        modifiedItems[baseItemID] = modifiedItemsList;
                    }

                    // Increment the number of the base item being added
                    if (remainingItemQuantities.ContainsKey(baseItemID))
                        remainingItemQuantities[baseItemID] += remainingItemQuantities[itemID];
                    else
                        remainingItemQuantities[baseItemID] = remainingItemQuantities[itemID];

                    // Remove the modified item from the remaining items
                    remainingItemQuantities.Remove(itemID);
                }

                maxStackSizes[baseItemID] = itemData.MaxStackSize;
                // Avoid adding the key multiple times for modified and base items
                if (itemData.MaxStackSize > 1 && !stackableItems.Contains(baseItemID))
                    stackableItems.Add(baseItemID);
                else if ((itemData.MaxStackSize == 1 && !nonStackableItems.Contains(baseItemID)))
                    nonStackableItems.Add(baseItemID);
            }

            bool AddToSlot(Slot slot, string itemDataID, int quantity)
            {
                slot.itemID = itemDataID;
                slot.quantity += quantity;
                modifiedItems.TryGetValue(itemDataID, out modifiedItemsList);
                if (modifiedItemsList != null && modifiedItemsList.Count > 0)
                {
                    for (int i = 1; i <= Math.Min(quantity, modifiedItemsList.Count); i++)
                    {
                        slot.instanceIDs.Add(modifiedItemsList[modifiedItemsList.Count - i]);
                        modifiedItemsList.RemoveAt(modifiedItemsList.Count - i);
                    }
                }

                int remaining = remainingItemQuantities[itemDataID] - quantity;
                if (remaining == 0)
                    remainingItemQuantities.Remove(itemDataID);
                else
                    remainingItemQuantities[itemDataID] = remaining;

                return remaining == 0;
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
                        if (AddToSlot(slot, itemID, 1))
                            nonStackableItems.RemoveAt(nonStackableItems.Count - 1);
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
                    int toAdd = Math.Min(remainingQuantity, maxStackSizes[slot.itemID] - slot.quantity);
                    if (AddToSlot(slot, slot.itemID, toAdd))
                        stackableItems.Remove(slot.itemID);
                }
                // Stop iterating if everything has been added
                if (remainingItemQuantities.Count == 0)
                    return remainingItemQuantities;
            }


            // Place all remaining items in known empty slots. Aside from maxing
            // stack size, it no longer matters whether it's stacked or nonstacked.
            foreach (Slot slot in emptySlots)
            {
                var pair = remainingItemQuantities.ElementAt(0);
                AddToSlot(slot, pair.Key, Math.Min(maxStackSizes[pair.Key], pair.Value));
                // If everything has been added, return the empty result
                if (remainingItemQuantities.Count == 0)
                    return remainingItemQuantities;
            }

            // Un-aggregate remaining modifiedItems before returning
            foreach (var pair in modifiedItems)
            {
                if (pair.Value.Count == 0)
                    continue;
                remainingItemQuantities[pair.Key] -= pair.Value.Count;
                int value;
                foreach (string itemID in pair.Value)
                {
                    remainingItemQuantities.TryGetValue(itemID, out value);
                    remainingItemQuantities[itemID] = value + 1;
                }
            }

            return remainingItemQuantities;
        }
        /// <summary>Removes all items</summary>
        public void Clear()
        {
            foreach (Slot slot in slots)
                slot.Clear();
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
                        (isModifiedItem && HasExactItemID(currIndex, itemID)) ||
                        (!isModifiedItem && HasExactItemID(currIndex, itemDataID))
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
                    slot.Clear();
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
                if (IsEmpty(currIndex) || !HasExactItemID(currIndex, itemID))
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
                    slot.Clear();
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
