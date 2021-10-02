using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using Inventory;


public class ItemManagerTest
{
    private ItemData item_shield;
    private ItemData item_sword;
    private Catalog catalog;
    private ItemManager sharedItemManager;
    private static string shieldItemID = "Armour.shield";
    private static string swordItemID = "Weapon.sword";
    private static string modifiedItemID = "MyCustomItemID";

    public static IEnumerable<string> validItemIDProvider()
    {
        yield return shieldItemID;
        yield return swordItemID;
    }

    public static IEnumerable<string> validMixedItemIDProvider()
    {
        yield return shieldItemID;
        yield return swordItemID;
        yield return modifiedItemID;
    }

    public static IEnumerable<string> invalidItemIDProvider()
    {
        yield return "Armour.sword";
        yield return "Weapon.shield";
        yield return "Jibberish";
        yield return "";
        yield return null;
    }

    [SetUp]
    public void SetUp()
    {
        item_shield = TestUtilities.CreateItemData("shield", Category.Armour, "Defensive shield", new Dictionary<Statistic, int>() {
            {Statistic.Value, 100},
            {Statistic.Weight, 20},
            {Statistic.Defense, 2},
        });
        item_sword = TestUtilities.CreateItemData("sword", Category.Weapon, "Stabby sword", new Dictionary<Statistic, int>() {
            {Statistic.Value, 150},
            {Statistic.Weight, 10},
            {Statistic.Attack, 3},
        });

        catalog = Catalog.Create(
            new List<ItemData>() { item_shield, item_sword }
        );
        sharedItemManager = new ItemManager(catalog);
        sharedItemManager.CreateModifiedItemID(swordItemID, modifiedItemID);
        // Can use this to load from an existing instance
        // catalog = (Catalog)AssetDatabase.LoadAssetAtPath( pathToTestCatalog, typeof(Catalog) );
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(item_shield);
        Object.DestroyImmediate(item_sword);
        Object.DestroyImmediate(catalog);
    }

    [Test, TestCaseSource("validItemIDProvider")]
    public void CreateModifiedItemID_ValidID_Success(string name)
    {
        // New instance so we they don't overlap/clash
        ItemManager itemManager = new ItemManager(catalog);
        string itemID = itemManager.CreateModifiedItemID(name);
        Assert.That(itemID, Does.StartWith($"{name}."));
        Assert.That(itemManager.IsValidID(itemID));
    }

    [Test, TestCaseSource("invalidItemIDProvider")]
    public void CreateModifiedItemID_InvalidID_Throws(string name)
    {
        Assert.That(() => { sharedItemManager.CreateModifiedItemID(name); }, Throws.TypeOf<KeyNotFoundException>());
    }

    [Test, TestCaseSource("validMixedItemIDProvider")]
    public void IsValidID_ValidID_Success(string itemID)
    {
        Assert.That(sharedItemManager.IsValidID(itemID));
    }
}
