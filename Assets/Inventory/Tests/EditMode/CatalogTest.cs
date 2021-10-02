using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Inventory;

public class CatalogTest
{
    private ItemData item_shield;
    private ItemData item_sword;
    private Catalog catalog;

    private ItemData CreateItemData(
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
        item_shield = CreateItemData("shield", Category.Armour, "Defensive shield", new Dictionary<Statistic, int>() {
            {Statistic.Value, 100},
            {Statistic.Weight, 20},
            {Statistic.Defense, 2},
        });
        item_sword = CreateItemData("sword", Category.Weapon, "Stabby sword", new Dictionary<Statistic, int>() {
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

    [Test]
    public void GetItemData_ValidID_FindsItem()
    {
        ItemData item = catalog.GetItemData("Armour.shield");
        Assert.That(item, Is.EqualTo(item_shield));

        item = catalog.GetItemData("Weapon.sword");
        Assert.That(item, Is.EqualTo(item_sword));
    }

    [TestCase("Weapon.shield")]
    [TestCase("Armour.sword")]
    [TestCase("Jibberish")]
    [TestCase("")]
    public void GetItemData_InvalidID_ReturnsNull(string name)
    {
        ItemData result = catalog.GetItemData(name);
        Assert.That(result, Is.Null);
    }
}
