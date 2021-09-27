using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    public enum Category
    {
        Armour,
        Weapon,
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
        public string itemName;
        public Category category;
        public string description;
        [Min(1)]
        public int maxStackSize = 1;
        public bool isConsumable = false;
        // TODO:
        // Sprite
        // Mesh?
        // Abilities (enum) list
        public SerializableDictionary<Statistic, int> statistics = new SerializableDictionary<Statistic, int>();

        public string Id() { return $"{category}.{itemName}"; }
        public bool IsStackable() { return maxStackSize > 1; }
        public int GetStat(Statistic stat)
        {
            int value;
            if (statistics.TryGetValue(stat, out value))
            {
                return value;
            }
            return 0;
        }
        public List<Statistic> ListStats()
        {
            List<Statistic> stats = new List<Statistic>();
            foreach (Statistic stat in statistics.Keys)
            {
                stats.Add(stat);
            }
            return stats;
        }
    }

}
