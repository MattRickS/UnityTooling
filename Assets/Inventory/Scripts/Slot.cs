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
    }

}
