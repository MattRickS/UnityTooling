using System;
using System.Collections.Generic;


namespace Inventory
{
    [Serializable]
    public class Slot
    {
        public string itemID = null;
        public int quantity = 0;
        public List<string> instanceIDs = new List<string>();

        public void Clear()
        {
            itemID = null;
            quantity = 0;
            instanceIDs.Clear();
        }
        public bool IsEmpty() { return quantity == 0; }
        public bool IsStacked() { return quantity > 0; }
        public bool HasModifiedItem() { return instanceIDs.Count > 0; }
        public bool HasItem() { return itemID != null; }
        public bool HasExactItemID(string itemID) { return this.itemID == itemID || instanceIDs.Contains(itemID); }
        // public int RemoveQuantity(int quantity)
        // {
        //     if (this.quantity < quantity)
        //     {
        //         int removed = this.quantity;
        //         Clear();
        //         return removed;
        //     }

        //     this.quantity -= quantity;
        //     if (quantity > instanceIDs.Count)
        //         instanceIDs.Clear();
        //     else
        //         instanceIDs.RemoveRange(instanceIDs.Count - quantity, quantity);

        //     return quantity;
        // }
        // public int RemoveQuantity(string itemID, int quantity)
        // {
        //     if (HasModifiedItem())
        //     {
        //         List<int> indices = new List<int>();
        //         for (int i = 0; i < instanceIDs.Count; i++)
        //         {
        //             if (instanceIDs[i] == itemID)
        //                 indices.Add(i);
        //         }
        //         if (indices.Count > 0)
        //         {
        //             for (int i = indices.Count - 1; i >= 0; i--)
        //             {
        //                 instanceIDs.RemoveAt(indices[i]);
        //             }
        //             this.quantity -= indices.Count;
        //             return indices.Count;
        //         }
        //     }

        //     if (this.itemID == itemID)
        //         return RemoveQuantity(quantity);

        //     return 0;
        // }
    }

}
