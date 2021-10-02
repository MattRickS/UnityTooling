using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using Inventory;


public class ItemManagerTest
{
    private ItemData item_shield;
    private ItemData item_sword;
    private Catalog catalog;

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

    [TestCase("Armour.shield", ExpectedResult = true)]
    [TestCase("Weapon.sword", ExpectedResult = true)]
    public bool CreateModifiedItemID_ValidID_Success(string name)
    {
        ItemManager itemManager = new ItemManager(catalog);
        string itemID = itemManager.CreateModifiedItemID(name);
        Assert.That(itemID, Does.StartWith($"{name}."));
        return itemManager.IsValidID(itemID);
    }

    [TestCase("Armour.sword")]
    [TestCase("Weapon.shield")]
    [TestCase("Jibberish")]
    [TestCase("")]
    public void CreateModifiedItemID_InvalidID_Throws(string name)
    {
        ItemManager itemManager = new ItemManager(catalog);
        Assert.That(() => { itemManager.CreateModifiedItemID(name); }, Throws.TypeOf<KeyNotFoundException>());
    }
}
