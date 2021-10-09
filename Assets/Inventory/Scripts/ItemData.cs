using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    // TODO: Enums are not extensible. Consider replacing with something like this:
    // https://stackoverflow.com/questions/5424138/extending-enums-in-c-sharp/5424270
    public enum Category
    {
        Armour,
        Weapon,
        Consumable,
        KeyItem,
        Miscellaneous,
    }

    public enum Statistic
    {
        Value,
        Weight,
        Attack,
        Defense,
        HealthRestore,
        ManaRestore,
    }

    [CreateAssetMenu(fileName = "Data", menuName = "Inventory/Item", order = 1)]
    public class ItemData : ScriptableObject
    {
        // All data is private and serializable so that it can be edited in the Editor
        // but is otherwise immutable.
        [SerializeField] private string itemName;
        [SerializeField] private Category category;
        [SerializeField] private string description;
        [Min(1)]
        [SerializeField] private int maxStackSize = 1;
        [SerializeField] private bool isConsumable = false;
        [SerializeField] private SerializableDictionary<Statistic, int> statistics = new SerializableDictionary<Statistic, int>();
        // TODO:
        // Sprite - Icon to display
        // Asset - Unity asset that can be instantiated when appearing in game
        // Abilities list - should this be added to the base class or let games
        //                  implement in subclasses? eg, Vorpal, Sonic, etc...
        // Tags - used for UI/filtering purposes

        public string Name { get { return itemName; } }
        public Category Category { get { return category; } }
        public string Description { get { return description; } }
        public int MaxStackSize { get { return maxStackSize; } }
        public bool IsConsumable { get { return isConsumable; } }
        public IEnumerator<KeyValuePair<Statistic, int>> Statistics()
        {
            foreach (var pair in statistics)
            {
                yield return pair;
            }
        }
        public int Statistic(Statistic stat)
        {
            int value;
            statistics.TryGetValue(stat, out value);
            return value;
        }

        public string Id() { return $"{category}.{itemName}"; }
        public bool IsStackable() { return maxStackSize > 1; }

        /// Provided to allow tests to instantiate items as needed.
        /// This method should _not_ be called within the game.
        public void Init(string name, Category category, Dictionary<Statistic, int> statistics, int maxStackSize = 1, bool isConsumable = false, string description = "")
        {
            this.itemName = name;
            this.category = category;
            this.description = description;
            this.maxStackSize = maxStackSize;
            this.isConsumable = isConsumable;
            this.statistics = new SerializableDictionary<Statistic, int>();
            foreach (var pair in statistics)
            {
                this.statistics.Add(pair);
            }
        }
    }

}
