using System;
using System.Collections;
using System.Collections.Generic;

namespace Inventory
{
    public class DictUtils
    {
        public static Dictionary<string, int> AddDicts(Dictionary<string, int> left, Dictionary<string, int> right)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            int quantity;
            foreach (var pair in right)
            {
                result.TryGetValue(pair.Key, out quantity);
                result[pair.Key] = quantity + pair.Value;
            }
            return result;
        }
        public static Dictionary<string, int> SubDicts(Dictionary<string, int> left, Dictionary<string, int> right, bool errOnNegative = true)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            int quantity, diff;
            foreach (var pair in right)
            {
                result.TryGetValue(pair.Key, out quantity);
                diff = quantity - pair.Value;
                // Only include items if they still have values
                if (diff > 0)
                    result[pair.Key] = quantity - pair.Value;
                else if (diff < 0 && errOnNegative)
                    throw new ArithmeticException("Insufficient value to be removed");
            }
            return result;
        }
        public static int SumDictValues(Dictionary<string, int> data)
        {
            int total = 0;
            foreach (int value in data.Values)
            {
                total += value;
            }
            return total;
        }
    }

    // TODO: Capacity needs to merge modified items with their base type

    /// <summary>
    /// Capacity is an interchange format for comparing Inventory space and ItemQuantites
    /// It represents a number of required slots, and is stackable aware.
    ///
    /// The idea of capacity works for empty space in an Inventory
    ///   inv = Inventory(3)
    ///   inv.AddItem(SWORD, 1)
    ///   inv.AddItem(POTION, 8)
    ///   inv.SpareCapacity() // Capacity(numSlots=1, 2 POTION)
    ///
    /// Capacity knows when different stackable items cannot be combined
    ///   items = ItemQuantities(2 GOLD, 1 SHIELD)
    ///   items.RequiredCapacity() // Capacity(numSlots=1, 2 GOLD)
    ///   inv.SpareCapacity().Fits(items.RequiredCapacity())  // false
    ///
    /// Mathematical operations handle max stack sizes
    ///   Capacity(slots=1, POTION=8) + Capacity(slots=1, POTION=3) = Capacity(slots=2, POTION=1)
    ///   Capacity(slots=1, POTION=8) - Capacity(POTION=3) = Capacity(slots=1, POTION=3)
    ///   Capacity(slots=1, POTION=8) - Capacity(GOLD=3) = Capacity(GOLD=7, POTION=8)
    ///
    /// Distributed content
    ///   capacity = inv1.SpareCapacity() + inv2.SpareCapacity() + inv3.SpareCapacity()
    ///   capacity.Fits(items)
    /// </summary>
    public class Capacity
    {
        private ItemManager itemManager;
        private int numSlots;
        private Dictionary<string, int> stackableItemQuantities = new Dictionary<string, int>();

        public Capacity(ItemManager itemManager, int numSlots = 0, Dictionary<string, int> stackableItemQuantities = null)
        {
            this.itemManager = itemManager;
            this.numSlots = numSlots;
            this.stackableItemQuantities = (stackableItemQuantities == null) ? new Dictionary<string, int>() : new Dictionary<string, int>(stackableItemQuantities);
        }

        /// <summary>Whether or not the items fit in this capacity</summary>
        public bool Fits(ItemQuantities itemQuantities)
        {
            int spareSlots = numSlots;
            int value, remainingCount;
            foreach (var pair in itemQuantities)
            {
                remainingCount = pair.Value;
                // If Capacity has stack space for the item, deduct that first
                if (stackableItemQuantities.TryGetValue(pair.Key, out value))
                {
                    remainingCount -= value;
                }
                // If there's still items to be placed, determine how many
                // slots are needed to fit the item stacks
                if (remainingCount > 0)
                {
                    spareSlots -= RequiredSlots(pair.Key, pair.Value);
                }
                // If the required slots exceeded capacity, return false
                if (spareSlots < 0)
                    return false;
            }
            return true;
        }

        // Helper methods
        /// <summary>How many slots a specific item and quantity requires</summary>
        private int RequiredSlots(string itemID, int quantity)
        {
            int size = itemManager.MaxStackSize(itemID);
            return quantity + size - 1 / size;
        }

        // Operators
        public static Capacity operator +(Capacity left, Capacity right)
        {
            return new Capacity(
                left.itemManager,
                left.numSlots + right.numSlots,
                DictUtils.AddDicts(left.stackableItemQuantities, right.stackableItemQuantities)
            );
        }
        public static Capacity operator -(Capacity left, Capacity right)
        {
            int numSlots = left.numSlots - right.numSlots;
            if (numSlots < 0)
                throw new ArithmeticException("Insufficient slots");

            Dictionary<string, int> spareStackableCapacity = new Dictionary<string, int>();
            int value;
            foreach (var pair in right.stackableItemQuantities)
            {
                if (left.stackableItemQuantities.TryGetValue(pair.Key, out value))
                {
                    int difference = value - pair.Value;
                    if (difference < 0)
                    {
                        int maxStackSize = left.itemManager.MaxStackSize(pair.Key);
                        int remainder;
                        numSlots -= Math.DivRem(Math.Abs(difference), maxStackSize, out remainder);
                        if (remainder > 0)
                        {
                            // Consume another spare slot to partially fill
                            numSlots -= 1;
                            spareStackableCapacity[pair.Key] = maxStackSize - remainder;
                        }
                    }
                    else if (difference > 0)
                    {
                        spareStackableCapacity[pair.Key] = difference;
                    }
                    // If the subtraction is exact, no further slots are consumed and nothing left.
                }
                else
                {
                    int maxStackSize = left.itemManager.MaxStackSize(pair.Key);
                    int remainder;
                    numSlots -= Math.DivRem(pair.Value, maxStackSize, out remainder);
                    if (remainder > 0)
                    {
                        // Consume another spare slot to partially fill
                        numSlots -= 1;
                        spareStackableCapacity[pair.Key] = maxStackSize - remainder;
                    }
                }
                if (numSlots < 0)
                    throw new ArithmeticException("Insufficient slots");
            }
            return new Capacity(left.itemManager, numSlots, spareStackableCapacity);
        }
    }

    public class ItemQuantities : IDictionary<string, int>
    {
        private ItemManager itemManager;
        private Dictionary<string, int> stackableItemQuantities = new Dictionary<string, int>();
        private Dictionary<string, int> nonStackableItemQuantities = new Dictionary<string, int>();

        // Constructors
        public ItemQuantities(ItemManager itemManager) { this.itemManager = itemManager; }
        public ItemQuantities(ItemQuantities itemQuantities)
        {
            this.itemManager = itemQuantities.itemManager;
            this.stackableItemQuantities = new Dictionary<string, int>(itemQuantities.stackableItemQuantities);
            this.nonStackableItemQuantities = new Dictionary<string, int>(itemQuantities.nonStackableItemQuantities);
        }
        public ItemQuantities(ItemManager itemManager, Dictionary<string, int> stackableItemQuantities, Dictionary<string, int> nonStackableItemQuantities)
        {
            this.itemManager = itemManager;
            this.stackableItemQuantities = stackableItemQuantities;
            this.nonStackableItemQuantities = nonStackableItemQuantities;
        }
        public ItemQuantities(ItemManager itemManager, Dictionary<string, int> itemQuantities)
        {
            this.itemManager = itemManager;
            // Split the items into stackable and non-stackable
            foreach (var pair in itemQuantities)
            {
                if (itemManager.MaxStackSize(pair.Key) > 1)
                    stackableItemQuantities[pair.Key] = pair.Value;
                else
                    nonStackableItemQuantities[pair.Key] = pair.Value;
            }
        }

        public void Increment(string itemID, int quantity = 1)
        {
            int current;
            TryGetValue(itemID, out current);
            this[itemID] = current + quantity;
        }
        /// Explicit itemIDs - does not remap modified item IDs
        public void Decrement(string itemID, int quantity = 1)
        {
            int current;
            TryGetValue(itemID, out current);
            int remaining = current - quantity;
            if (remaining < 0)
                throw new ArithmeticException("Cannot decrement more than exists");
            else if (current > 0 && remaining == 0)
                Remove(itemID);
            else if (remaining > 0)
                this[itemID] = remaining;
        }
        public Capacity RequiredCapacity()
        {
            return new Capacity(itemManager, DictUtils.SumDictValues(nonStackableItemQuantities), stackableItemQuantities);
        }

        // Operators
        public static ItemQuantities operator +(ItemQuantities left, ItemQuantities right)
        {
            ItemQuantities itemQuantities = new ItemQuantities(left.itemManager);
            itemQuantities.stackableItemQuantities = DictUtils.AddDicts(left.stackableItemQuantities, right.stackableItemQuantities);
            itemQuantities.nonStackableItemQuantities = DictUtils.AddDicts(left.nonStackableItemQuantities, right.nonStackableItemQuantities);
            return itemQuantities;
        }
        public static ItemQuantities operator -(ItemQuantities left, ItemQuantities right)
        {
            ItemQuantities itemQuantities = new ItemQuantities(left.itemManager);
            itemQuantities.stackableItemQuantities = DictUtils.SubDicts(left.stackableItemQuantities, right.stackableItemQuantities);
            itemQuantities.nonStackableItemQuantities = DictUtils.SubDicts(left.nonStackableItemQuantities, right.nonStackableItemQuantities);
            return itemQuantities;
        }
        // Equality operators are supposed to be used for instance equality, not content equaity.
        // public static bool operator ==(ItemQuantities left, ItemQuantities right)
        // {
        //     return left.stackableItemQuantities == right.stackableItemQuantities && left.nonStackableItemQuantities == right.nonStackableItemQuantities;
        // }
        // public static bool operator !=(ItemQuantities left, ItemQuantities right)
        // {
        //     return left.stackableItemQuantities != right.stackableItemQuantities || left.nonStackableItemQuantities != right.nonStackableItemQuantities;
        // }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            ItemQuantities other = (ItemQuantities)obj;
            return stackableItemQuantities != other.stackableItemQuantities || nonStackableItemQuantities != other.nonStackableItemQuantities;
        }
        // TODO: Dictionaries shouldn't be hashable as they're mutable, yet this is enforced?
        public override int GetHashCode() { return 0; }

        // Dictionary Operations
        public void Add(string itemID, int quantity)
        {
            if (itemManager.MaxStackSize(itemID) > 1)
                stackableItemQuantities.Add(itemID, quantity);
            else
                nonStackableItemQuantities.Add(itemID, quantity);
        }
        public void Add(KeyValuePair<string, int> pair)
        {
            Add(pair.Key, pair.Value);
        }
        public void Clear()
        {
            stackableItemQuantities.Clear();
            nonStackableItemQuantities.Clear();
        }
        public bool ContainsKey(string itemID)
        {
            return nonStackableItemQuantities.ContainsKey(itemID) || stackableItemQuantities.ContainsKey(itemID);
        }
        public bool Contains(KeyValuePair<string, int> pair)
        {
            int value;
            return (
                nonStackableItemQuantities.TryGetValue(pair.Key, out value) && value == pair.Value
                || stackableItemQuantities.TryGetValue(pair.Key, out value) && value == pair.Value
            );
        }
        public void CopyTo(KeyValuePair<string, int>[] pairs, int size)
        {
            if (size > nonStackableItemQuantities.Count)
            {
                size -= nonStackableItemQuantities.Count;
                int i = 0;
                foreach (var pair in stackableItemQuantities)
                {
                    if (i >= size)
                    {
                        pairs[i - size] = pair;
                    }
                    i++;
                }
            }
            else
            {
                int i = 0;
                foreach (var pair in nonStackableItemQuantities)
                {
                    if (i >= size)
                    {
                        pairs[i - size] = pair;
                    }
                    i++;
                }
                foreach (var pair in stackableItemQuantities)
                {
                    pairs[i - size] = pair;
                    i++;
                }
            }
        }
        public bool Remove(string itemID)
        {
            return nonStackableItemQuantities.Remove(itemID) || stackableItemQuantities.Remove(itemID);
        }
        public bool Remove(KeyValuePair<string, int> pair)
        {
            int value;
            if (nonStackableItemQuantities.TryGetValue(pair.Key, out value) && value == pair.Value)
                return nonStackableItemQuantities.Remove(pair.Key);
            else if (stackableItemQuantities.TryGetValue(pair.Key, out value) && value == pair.Value)
                return stackableItemQuantities.Remove(pair.Key);
            return false;
        }
        public bool TryGetValue(string itemID, out int quantity)
        {
            int result;
            if (nonStackableItemQuantities.TryGetValue(itemID, out result) || stackableItemQuantities.TryGetValue(itemID, out result))
            {
                quantity = result;
                return true;
            }
            quantity = result;
            return false;
        }

        public int Count { get; }
        public bool IsReadOnly { get { return false; } }
        public ICollection<string> Keys { get; }
        public ICollection<int> Values { get; }

        public int this[string itemID]
        {
            get
            {
                int value;
                if (stackableItemQuantities.TryGetValue(itemID, out value)) return value;
                if (nonStackableItemQuantities.TryGetValue(itemID, out value)) return value;
                throw new KeyNotFoundException(itemID);
            }
            set
            {
                if (itemManager.MaxStackSize(itemID) > 1)
                    stackableItemQuantities[itemID] = value;
                else
                    nonStackableItemQuantities[itemID] = value;
            }
        }
        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            foreach (var pair in nonStackableItemQuantities)
                yield return pair;
            foreach (var pair in stackableItemQuantities)
                yield return pair;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}