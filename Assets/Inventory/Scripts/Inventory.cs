using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    public class Inventory : ScriptableObject
    {
        List<Slot> slots;

        public Inventory(int size) { slots = new List<Slot>(size); }

        public Dictionary<Statistic, int> AggregateStatistics(List<Statistic> stats)
        {
            Dictionary<Statistic, int> aggregates = new Dictionary<Statistic, int>();
            foreach (Statistic stat in stats)
            {
                aggregates[stat] = 0;
            }

            foreach (Slot slot in slots)
            {
                if (slot.IsEmpty()) { continue; }

                ItemData itemData = slot.GetItemData();
                foreach (Statistic stat in stats)
                {
                    aggregates[stat] += itemData.GetStat(stat);
                }
            }
            return aggregates;
        }

        public int AddItem(Item item, int quantity = 1)
        {
            int currIndex = 0;
            int remaining = quantity;
            int maxStackSize = item.ItemData.maxStackSize;
            for (; currIndex < quantity; currIndex++)
            {
                Slot slot = slots[currIndex];
                int n;
                if (slot.IsEmpty())
                {
                    n = Math.Min(quantity, maxStackSize);
                }
                else
                {
                    n = Math.Min(quantity, slot.MaxStackSize() - slot.StackSize());
                }

                while (n > 0)
                {
                    slot.AddItem(item.copy());
                    n--;
                }

                if (n == 0) break;
            }
            return quantity - remaining;
        }
        // public int AddItem(string itemID, int quantity = 1)
        // {

        // }
    }

}
