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

    [TestCase(ItemTestHarness.shieldItemID, 1)]
    [TestCase(ItemTestHarness.modifiedSwordItemID, 1)]  // Modified items work
    [TestCase(ItemTestHarness.swordItemID, 10)]  // Max Capacity for unstacked
    [TestCase(ItemTestHarness.healthPotionID, 100)]  // Stack size of 10
    public void HasCapacity_ValidItemHasCapacity_True(string itemID, int quantity)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(10);
        Assert.That(manager.HasCapacity(inventoryID, itemID, quantity: quantity), Is.True);
    }

    [TestCase(ItemTestHarness.swordItemID, 11)]  // Max Capacity for unstacked
    [TestCase(ItemTestHarness.healthPotionID, 101)]  // Stack size of 10
    public void HasCapacity_ValidItemHasCapacity_False(string itemID, int quantity)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(10);
        Assert.That(manager.HasCapacity(inventoryID, itemID, quantity: quantity), Is.False);
    }
}
