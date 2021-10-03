using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

using Inventory;

public class ItemTestHarness
{
    public ItemData item_shield;
    public ItemData item_sword;
    public Catalog catalog;
    public ItemManager sharedItemManager;
    public const string shieldItemID = "Armour.shield";
    public const string swordItemID = "Weapon.sword";
    public const string modifiedSwordItemID = "MyCustomItemID";

    public static IEnumerable<string> validStaticItemIDProvider()
    {
        yield return shieldItemID;
        yield return swordItemID;
    }

    public static IEnumerable<string> validMixedItemIDProvider()
    {
        yield return shieldItemID;
        yield return swordItemID;
        yield return modifiedSwordItemID;
    }

    public static IEnumerable<string> invalidItemIDProvider()
    {
        yield return "Armour.sword";
        yield return "Weapon.shield";
        yield return "Jibberish";
        yield return "";
        yield return null;
    }

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

    [SetUp]
    public void SetUp()
    {
        item_shield = ItemTestHarness.CreateItemData("shield", Category.Armour, "Defensive shield", new Dictionary<Statistic, int>() {
            {Statistic.Value, 100},
            {Statistic.Weight, 20},
            {Statistic.Defense, 2},
        });
        item_sword = ItemTestHarness.CreateItemData("sword", Category.Weapon, "Stabby sword", new Dictionary<Statistic, int>() {
            {Statistic.Value, 150},
            {Statistic.Weight, 10},
            {Statistic.Attack, 3},
        });

        catalog = Catalog.Create(
            new List<ItemData>() { item_shield, item_sword }
        );
        sharedItemManager = new ItemManager(catalog);
        sharedItemManager.CreateModifiedItemID(swordItemID, modifiedSwordItemID);
        sharedItemManager.SetItemStatisticDeltaValue(modifiedSwordItemID, Statistic.Value, -10);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(item_shield);
        Object.DestroyImmediate(item_sword);
        Object.DestroyImmediate(catalog);
    }
}
