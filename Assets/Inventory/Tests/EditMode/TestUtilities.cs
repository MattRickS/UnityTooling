using System.Collections.Generic;
using UnityEngine;

using Inventory;

public static class TestUtilities
{
    public static ItemData CreateItemData(
        string name,
        Category category,
        string description,
        Dictionary<Statistic, int> stats = null,
        int maxStackSize = 1,
        bool isConsumable = false
    )
    {
        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        item.itemName = name;
        item.category = category;
        item.description = description;
        item.maxStackSize = maxStackSize;
        item.isConsumable = isConsumable;
        if (stats != null)
        {
            foreach (var pair in stats)
            {
                item.statistics.Add(pair.Key, pair.Value);
            }
        }
        return item;
    }
}
