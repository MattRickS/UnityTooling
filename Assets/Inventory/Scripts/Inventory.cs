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
        // For now, dependency inject the manager and InventoryManager serialisation
        // ensures all inventories use a shared instance. Should look at making this
        // private and see if there are better serialization options.
        private ItemManager itemManager;
        public string inventoryID = System.Guid.NewGuid().ToString();

        public void SetItemManager(ItemManager itemManager) { this.itemManager = itemManager; }

        [SerializeField] List<Slot> slots;

        public Inventory(ItemManager itemManager, int size)
        {
            this.itemManager = itemManager;
            slots = new List<Slot>(size);
            for (int i = 0; i < size; i++)
            {
                slots.Add(new Slot());
            }
        }

        public string Id() { return inventoryID; }

        public ItemData GetItemData(int index) { return itemManager.GetItemData(slots[index].itemID); }

        public int NumSlots() { return slots.Count; }

        // Stacking
        public int StackSize(int index) { return slots[index].quantity; }
        public int MaxStackSize(int index) { return GetItemData(index).maxStackSize; }
        public bool IsStackable(int index) { return MaxStackSize(index) > 1; }

        // State
        public bool IsEmpty(int index) { return slots[index].itemID == Slot.NO_ITEM; }
        public bool IsFull(int index) { return StackSize(index) == MaxStackSize(index); }
        public bool HasItem(int index, string itemID) { return slots[index].itemID == itemID; }

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
