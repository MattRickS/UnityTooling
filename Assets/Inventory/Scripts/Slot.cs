using System;
using System.Collections.Generic;


namespace Inventory
{
    [Serializable]
    public class Slot
    {
        public const string NO_ITEM = "";

        public string itemID = NO_ITEM;
        public int quantity = 0;
        public List<string> instanceIDs;

        public bool IsEmpty() { return quantity == 0; }
        public bool IsStacked() { return quantity > 0; }
        public bool HasModifiedItem() { return instanceIDs.Count > 0; }
        public bool HasItem() { return itemID != NO_ITEM; }
        public bool HasExactItemID(string itemID) { return this.itemID == itemID || instanceIDs.Contains(itemID); }
    }

}
