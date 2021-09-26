using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    // ScriptableObject won't store runtime build data - Service must save to JSON
    // TODO: Is it better for this to not be a ScriptableObject?
    [CreateAssetMenu(fileName = "Data", menuName = "Inventory/Inventory", order = 3)]
    public class Inventory : ScriptableObject
    {
        public string inventoryID = System.Guid.NewGuid().ToString();

        [SerializeField] List<Slot> slots;

        public Inventory(int size) { slots = new List<Slot>(size); }

        public string Id() { return inventoryID; }

        public ItemData GetItemData(int index)
        {
            InventoryService service = GameServices.ServiceLocator.Instance.Get<InventoryService>();
            return service.GetItemData(slots[index].itemID);
        }

        public int NumSlots() { return slots.Count; }

        // Stacking
        public int StackSize(int index) { return slots[index].quantity + slots[index].instanceIDs.Count; }
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
            InventoryService service = GameServices.ServiceLocator.Instance.Get<InventoryService>();
            int value = service.GetItemStatistic(slot.itemID, stat) * slot.quantity;
            foreach (string instanceID in slot.instanceIDs)
            {
                value += service.GetItemStatistic(instanceID, stat);
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
        */
        public int AddItem(string itemID, int quantity = 1)
        {
            InventoryService service = GameServices.ServiceLocator.Instance.Get<InventoryService>();
            int remaining = quantity;
            int maxStackSize = service.GetItemData(itemID).maxStackSize;
            for (int currIndex = 0; currIndex < NumSlots(); currIndex++)
            {
                if (IsEmpty(currIndex))
                {
                    int n = Math.Min(quantity, maxStackSize);
                    Slot slot = slots[currIndex];
                    slot.itemID = itemID;
                    // TODO: if modified
                    slot.quantity = n;
                    remaining -= n;
                }
                else if (HasItem(currIndex, itemID) && !IsFull(currIndex))
                {
                    int n = Math.Min(quantity, maxStackSize - StackSize(currIndex));
                    slots[currIndex].quantity += n;
                    remaining -= n;
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
            int remaining = quantity;
            for (int currIndex = NumSlots() - 1; currIndex >= 0; currIndex--)
            {
                if (IsEmpty(currIndex) || !HasItem(currIndex, itemID))
                {
                    continue;
                }
                int n = Math.Min(remaining, StackSize(currIndex));
                Slot slot = slots[currIndex];
                slot.itemID = itemID;
                // TODO: remove delta IDs (optional?)
                slot.quantity -= n;
                remaining -= n;

                if (remaining == 0)
                {
                    slot.itemID = Slot.NO_ITEM;
                    break;
                }
            }
            return quantity - remaining;
        }
    }
}
