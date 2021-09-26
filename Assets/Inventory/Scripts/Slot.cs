using System;
using System.Collections.Generic;


namespace Inventory
{
    [Serializable]
    public class Slot
    {
        public const string NO_ITEM = "EMPTY";

        public string itemID = NO_ITEM;
        public int quantity = 0;
        public List<string> instanceIDs;

    }

}
