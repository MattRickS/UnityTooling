using System;
using System.Collections.Generic;


namespace Inventory
{
    public class Slot
    {
        string itemID = null;
        int quantity = 0;
        List<string> instanceIDs;

        public bool HasModifiedItems() { return instanceIDs.Count > 0; }
        public bool IsEmpty() { return itemID == null; }
        public ItemData GetItemData() { return GameServices.ServiceLocator.Instance.Get<InventoryService>().GetItemData(itemID); }
        public Item GetItem()
        {
            // if (HasModifiedItems()) { return instances[instances.Count - 1]; }
            return new Item(GetItemData());
        }
        public int StackSize() { return quantity + instances.Count; }
        public int MaxStackSize() { return IsEmpty() ? 0 : GetItemData().maxStackSize; }
        public bool IsStackable() { return IsEmpty() ? false : GetItemData().IsStackable(); }
        public bool IsFull() { return StackSize() == MaxStackSize(); }
        public bool AddItem(Item item, int quantity = 1)
        {
            // TODO: Check input quantity
            if (IsFull()) { return false; }
            // TODO: Should set to the data ID
            // if (itemID == null) { itemID = item.Id(); }
            if (item.IsModified())
            {
                instances.Add(item);
            }
            else
            {
                quantity += 1;
            }
            return true;
        }
        public int AddQuantity(int quantity)
        {
            if (IsEmpty()) throw new Exception("Cannot add quantity to an empty slot");
            int toAdd = Math.Min(quantity, MaxStackSize() - StackSize());
            quantity += toAdd;
            return toAdd;
        }
        public int RemoveQuantity(int quantity)
        {
            if (IsEmpty()) throw new Exception("Cannot remove quantity from an empty slot");
            // TODO: Remove instances first
            int toRemove = Math.Min(quantity, StackSize());
            quantity -= toRemove;
            if (StackSize() == 0)
            {
                itemID = null;
            }
            return toRemove;
        }
        public List<Item> TakeItems(int n)
        {
            if (StackSize() < n)
            {
                throw new Exception("Insufficient items");
            }
            List<Item> items = new List<Item>();
            while (n > 0 && HasModifiedItems())
            {
                items.Add(items[instances.Count - 1]);
                n--;
            }
            quantity -= n;
            while (n > 0)
            {
                items.Add(new Item(GetItemData()));
                n--;
            }
            return items;
        }
    }

}
