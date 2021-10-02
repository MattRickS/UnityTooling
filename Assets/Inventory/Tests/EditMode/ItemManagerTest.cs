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
    private static string modifiedSwordItemID = "MyCustomItemID";

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
        sharedItemManager.CreateModifiedItemID(swordItemID, modifiedSwordItemID);
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

    // =========================================================================
    // Tests

    [Test, TestCaseSource("validStaticItemIDProvider")]
    public void CreateModifiedItemID_ValidID_Success(string name)
    {
        // New instance so we they don't overlap/clash
        ItemManager itemManager = new ItemManager(catalog);
        string firstItemID = itemManager.CreateModifiedItemID(name);
        Assert.That(firstItemID, Does.StartWith($"{name}."));
        Assert.That(itemManager.IsValidID(firstItemID));
        // Each call should produce a valid unique ID
        string secondItemID = itemManager.CreateModifiedItemID(name);
        Assert.That(itemManager.IsValidID(secondItemID));
        Assert.That(firstItemID, Is.Not.EqualTo(secondItemID));
    }

    [Test, TestCaseSource("invalidItemIDProvider")]
    public void CreateModifiedItemID_InvalidID_Throws(string name)
    {
        Assert.That(() => { sharedItemManager.CreateModifiedItemID(name); }, Throws.TypeOf<KeyNotFoundException>());
    }

    [Test, TestCaseSource("validMixedItemIDProvider")]
    public void IsValidID_ValidID_True(string itemID)
    {
        Assert.That(sharedItemManager.IsValidID(itemID), Is.True);
    }

    [Test, TestCaseSource("invalidItemIDProvider")]
    public void IsValidID_InvalidID_False(string itemID)
    {
        Assert.That(sharedItemManager.IsValidID(itemID), Is.False);
    }

    [Test, TestCaseSource("validStaticItemIDProvider")]
    public void IsStaticItemID_StaticID_True(string itemID)
    {
        Assert.That(sharedItemManager.IsStaticItemID(itemID), Is.True);
    }

    [Test]
    public void IsStaticItemID_ModifiedID_False()
    {
        Assert.That(sharedItemManager.IsStaticItemID(modifiedSwordItemID), Is.False);
    }

    [Test]
    public void IsModifiedItemID_ModifiedID_True()
    {
        Assert.That(sharedItemManager.IsModifiedItemID(modifiedSwordItemID), Is.True);
    }

    [Test, TestCaseSource("validStaticItemIDProvider")]
    public void IsModifiedItemID_StaticID_False(string itemID)
    {
        Assert.That(sharedItemManager.IsModifiedItemID(itemID), Is.False);
    }

    [Test]
    public void NumModifiedItems()
    {
        ItemManager manager = new ItemManager(catalog);
        Assert.That(manager.NumModifiedItems(), Is.EqualTo(0));
        manager.CreateModifiedItemID(swordItemID);
        Assert.That(manager.NumModifiedItems(), Is.EqualTo(1));
        manager.CreateModifiedItemID(shieldItemID);
        Assert.That(manager.NumModifiedItems(), Is.EqualTo(2));
    }

    [Test]
    public void NumStaticIDs()
    {
        // Modified items shouldn't increment the static item count
        ItemManager manager = new ItemManager(catalog);
        Assert.That(manager.NumStaticItems(), Is.EqualTo(2));
        manager.CreateModifiedItemID(swordItemID);
        Assert.That(manager.NumStaticItems(), Is.EqualTo(2));
    }

    [Test]
    public void GetItemData_ValidID_Success()
    {
        Assert.That(sharedItemManager.GetItemData(shieldItemID), Is.EqualTo(item_shield));
        Assert.That(sharedItemManager.GetItemData(swordItemID), Is.EqualTo(item_sword));
        Assert.That(sharedItemManager.GetItemData(modifiedSwordItemID), Is.EqualTo(item_sword));
    }

    [Test, TestCaseSource("invalidItemIDProvider")]
    public void GetItemData_InvalidID_null(string itemID)
    {
        Assert.That(sharedItemManager.GetItemData(itemID), Is.Null);
    }
}
