using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

using Inventory;

public class ItemTestHarness
{
    public ItemData item_shield;
    public ItemData item_sword;
    public ItemData item_healthPotion;
    public ItemDatabase sharedItemManager;
    public const string shieldItemID = "Armour.shield";
    public const string swordItemID = "Weapon.sword";
    public const string healthPotionID = "Miscellaneous.healthPotion";
    public const string manaPotionID = "Miscellaneous.manaPotion";
    public const string modifiedSwordItemID = "MyCustomItemID";

    public static IEnumerable<string> validStaticItemIDProvider()
    {
        yield return shieldItemID;
        yield return swordItemID;
        yield return healthPotionID;
    }

    public static IEnumerable<string> validMixedItemIDProvider()
    {
        foreach (string itemID in validStaticItemIDProvider())
            yield return itemID;
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
        item.Init(
            name, category, maxStackSize: maxStackSize, isConsumable: isConsumable, description: description, statistics: stats
        );
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
        item_healthPotion = ItemTestHarness.CreateItemData("healthPotion", Category.Miscellaneous, "Heals Wounds", new Dictionary<Statistic, int>() {
            {Statistic.Value, 20},
            {Statistic.Weight, 1},
            {Statistic.HealthRestore, 3},
        },
        maxStackSize: 10, isConsumable: true);

        sharedItemManager = new ItemDatabase();
        sharedItemManager.CreateModifiedItemID(swordItemID, modifiedSwordItemID);
        sharedItemManager.SetItemStatisticDeltaValue(modifiedSwordItemID, Statistic.Value, -10);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(item_shield);
        Object.DestroyImmediate(item_sword);
    }
}
