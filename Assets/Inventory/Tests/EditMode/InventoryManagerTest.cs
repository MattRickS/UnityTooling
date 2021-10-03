using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Inventory;

public class InventoryManagerTest : ItemTestHarness
{
    [Test]
    public void CreateInventory()
    {
        InventoryManager manager = new InventoryManager();
        Assert.That(manager.NumInventories(), Is.EqualTo(0));
        string inventoryID = manager.CreateInventory(10);
        Assert.That(manager.NumInventories(), Is.EqualTo(1));
        Assert.That(manager.IsValidInventoryID(inventoryID), Is.True);
    }

    [TestCase(shieldItemID, 1, ExpectedResult = true)]
    [TestCase(modifiedSwordItemID, 1, ExpectedResult = true)]  // Modified items work
    [TestCase(swordItemID, 10, ExpectedResult = true)]  // Max Capacity for unstacked
    [TestCase(healthPotionID, 100, ExpectedResult = true)]  // Stack size of 10
    [TestCase(swordItemID, 11, ExpectedResult = false)]
    [TestCase(healthPotionID, 101, ExpectedResult = false)]
    public bool HasCapacity_ValidItemHasCapacity_MatchesResult(string itemID, int quantity)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(10);
        return manager.HasCapacity(inventoryID, itemID, quantity: quantity);
    }

    [TestCase(shieldItemID, 1, ExpectedResult = 1)]
    [TestCase(modifiedSwordItemID, 1, ExpectedResult = 1)]  // Modified items work
    [TestCase(swordItemID, 10, ExpectedResult = 10)]  // Max Capacity for unstacked
    [TestCase(healthPotionID, 100, ExpectedResult = 100)]  // Stack size of 10
    [TestCase(swordItemID, 11, ExpectedResult = 10)]
    [TestCase(healthPotionID, 101, ExpectedResult = 100)]
    public int AddItemToInventory_ValidItemEmpty_MatchesResult(string itemID, int quantity)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(10);
        return manager.AddItemToInventory(inventoryID, itemID, quantity: quantity);
    }

    [TestCase(shieldItemID, 1, ExpectedResult = 1)]
    [TestCase(modifiedSwordItemID, 1, ExpectedResult = 1)]
    [TestCase(swordItemID, 10, ExpectedResult = 1)]
    [TestCase(healthPotionID, 15, ExpectedResult = 15)]
    [TestCase(healthPotionID, 16, ExpectedResult = 15)]
    public int AddItemToInventory_ValidItemPartialFull_MatchesResult(string itemID, int quantity)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(3);
        manager.AddItemToInventory(inventoryID, healthPotionID, 5);
        manager.AddItemToInventory(inventoryID, swordItemID);
        return manager.AddItemToInventory(inventoryID, itemID, quantity: quantity);
    }

    public static IEnumerable<Dictionary<string, int>> itemBundleProviderA()
    {
        yield return new Dictionary<string, int>() { { shieldItemID, 3 } };
        yield return new Dictionary<string, int>() { { shieldItemID, 2 }, { swordItemID, 1 } };
        yield return new Dictionary<string, int>() { { shieldItemID, 1 }, { swordItemID, 1 }, { modifiedSwordItemID, 1 } };
        yield return new Dictionary<string, int>() { { shieldItemID, 1 }, { swordItemID, 1 }, { healthPotionID, 10 } };
        yield return new Dictionary<string, int>() { { shieldItemID, 1 }, { healthPotionID, 20 } };
        yield return new Dictionary<string, int>() { };
    }

    public static IEnumerable<Dictionary<string, int>> itemBundleProviderB()
    {
        yield return new Dictionary<string, int>() { { shieldItemID, 2 }, { swordItemID, 2 } };
        yield return new Dictionary<string, int>() { { shieldItemID, 1 }, { swordItemID, 1 }, { healthPotionID, 11 } };
        yield return new Dictionary<string, int>() { { shieldItemID, 1 }, { healthPotionID, 21 } };
        yield return new Dictionary<string, int>() { { shieldItemID, 1 }, { swordItemID, 1 }, { modifiedSwordItemID, 1 }, { healthPotionID, 1 } };
    }

    // TODO: Add cases with existing items in inventory
    [TestCaseSource("itemBundleProviderA")]
    public void HasCapacity_ValidItemQuantitiesHasCapacity_True(Dictionary<string, int> itemQuantities)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(3);
        Assert.That(manager.HasCapacity(inventoryID, itemQuantities), Is.True);
    }

    [TestCaseSource("itemBundleProviderB")]
    public void HasCapacity_ValidItemQuantitiesHasCapacity_False(Dictionary<string, int> itemQuantities)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(3);
        Assert.That(manager.HasCapacity(inventoryID, itemQuantities), Is.False);
    }
}
