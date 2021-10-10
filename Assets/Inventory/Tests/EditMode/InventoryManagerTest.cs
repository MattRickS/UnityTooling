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

    [TestCase(shieldItemID, 1, ExpectedResult = 0)]
    [TestCase(modifiedSwordItemID, 1, ExpectedResult = 0)]  // Modified items work
    [TestCase(swordItemID, 10, ExpectedResult = 0)]  // Max Capacity for unstacked
    [TestCase(healthPotionID, 100, ExpectedResult = 0)]  // Stack size of 10
    [TestCase(swordItemID, 11, ExpectedResult = 1)]
    [TestCase(healthPotionID, 101, ExpectedResult = 1)]
    public int AddItemToInventory_ValidItemEmpty_MatchesResult(string itemID, int quantity)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(10);
        return manager.AddItemToInventory(inventoryID, itemID, quantity: quantity);
    }

    // TODO: Tests for
    //  * Modified stackable item should stack fine
    //  * Resulting items should be correct
    [TestCase(shieldItemID, 1, ExpectedResult = 0)]
    [TestCase(modifiedSwordItemID, 1, ExpectedResult = 0)]
    [TestCase(swordItemID, 10, ExpectedResult = 9)]
    [TestCase(healthPotionID, 15, ExpectedResult = 0)]
    [TestCase(healthPotionID, 16, ExpectedResult = 1)]
    public int AddItemToInventory_ValidItemPartialFull_MatchesResult(string itemID, int quantity)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(3);
        manager.AddItemToInventory(inventoryID, healthPotionID, 5);
        manager.AddItemToInventory(inventoryID, swordItemID);
        return manager.AddItemToInventory(inventoryID, itemID, quantity: quantity);
    }

    [Test]
    public void GetInventoryItems_Empty_EmptyDict()
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(3);
        Assert.That(manager.GetInventoryItems(inventoryID), Is.EqualTo(new Dictionary<string, int>()));
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

    [TestCaseSource("itemBundleProviderA")]
    public void HasItems_Items_True(Dictionary<string, int> itemQuantities)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(3);
        manager.AddItemsToInventory(inventoryID, itemQuantities);

        Assert.That(manager.HasItems(inventoryID, itemQuantities), Is.True);
    }

    [TestCaseSource("itemBundleProviderA")]
    public void HasItems_Items_False(Dictionary<string, int> itemQuantities)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(3);
        manager.AddItemsToInventory(inventoryID, itemQuantities);

        Dictionary<string, int> hasItems = new Dictionary<string, int>(itemQuantities);
        var enumerator = hasItems.GetEnumerator();
        // Empty dict will always be true, skip it
        if (!enumerator.MoveNext())
            return;

        // Increment one of the required items by 1 to exceed the inventory content
        hasItems[enumerator.Current.Key] += 1;
        Assert.That(manager.HasItems(inventoryID, itemQuantities), Is.True);
    }


    [TestCaseSource("itemBundleProviderA")]
    public void GetInventoryItems_Items_Matches(Dictionary<string, int> itemQuantities)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(3);
        manager.AddItemsToInventory(inventoryID, itemQuantities);
        Assert.That(manager.GetInventoryItems(inventoryID), Is.EqualTo(itemQuantities));
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

    [TestCase(shieldItemID, 0, true, 0)]
    [TestCase(shieldItemID, 1, false, -1)]
    [TestCase(modifiedSwordItemID, 0, true, 1)]
    [TestCase(modifiedSwordItemID, 2, false, -1)]
    [TestCase(healthPotionID, 0, true, 2)]
    [TestCase(healthPotionID, 3, true, 4)]
    [TestCase(manaPotionID, 0, false, -1)]
    public void FindItem_Default_MatchesResult(string itemID, int startIndex, bool found, int expectedIndex)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(6);
        manager.AddItemToInventory(inventoryID, shieldItemID);
        manager.AddItemToInventory(inventoryID, modifiedSwordItemID);
        manager.AddItemToInventory(inventoryID, healthPotionID, quantity: 10);
        manager.AddItemToInventory(inventoryID, swordItemID);
        manager.AddItemToInventory(inventoryID, healthPotionID, quantity: 3);

        int index;
        bool result = manager.FindItem(inventoryID, itemID, out index, startIndex: startIndex);
        Assert.That(result, Is.EqualTo(found));
        Assert.That(index, Is.EqualTo(expectedIndex));
    }

    [TestCase(shieldItemID, 1, true)]
    [TestCase(healthPotionID, 13, true)]
    [TestCase(healthPotionID, 14, false)]
    [TestCase(swordItemID, 2, true)]
    [TestCase(manaPotionID, 1, false)]
    public void HasItem_Default_MatchesResult(string itemID, int quantity, bool expected)
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(6);
        manager.AddItemToInventory(inventoryID, shieldItemID);
        manager.AddItemToInventory(inventoryID, modifiedSwordItemID);
        manager.AddItemToInventory(inventoryID, healthPotionID, quantity: 10);
        manager.AddItemToInventory(inventoryID, swordItemID);
        manager.AddItemToInventory(inventoryID, healthPotionID, quantity: 3);

        bool result = manager.HasItem(inventoryID, itemID, quantity);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TakeItem_StaticIDSingleQuantity_Success()
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(6);
        manager.AddItemToInventory(inventoryID, swordItemID);
        manager.AddItemToInventory(inventoryID, shieldItemID);

        Dictionary<string, int> itemQuantities = new Dictionary<string, int>();

        // Defaults to taking one item, which is taken and added to the itemQuantities
        int notTaken = manager.TakeItem(inventoryID, itemQuantities, shieldItemID);
        Assert.That(notTaken, Is.EqualTo(0));
        Assert.That(itemQuantities[shieldItemID], Is.EqualTo(1));

        // No more shields to take, should return 1 untaken and not modify itemQuantities
        notTaken = manager.TakeItem(inventoryID, itemQuantities, shieldItemID);
        Assert.That(notTaken, Is.EqualTo(1));
        Assert.That(itemQuantities[shieldItemID], Is.EqualTo(1));
    }

    [Test]
    public void TakeItem_StaticIDMultipleQuantity_Success()
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(6);
        manager.AddItemToInventory(inventoryID, swordItemID);
        manager.AddItemToInventory(inventoryID, shieldItemID, 3);

        Dictionary<string, int> itemQuantities = new Dictionary<string, int>();

        // Takes all items and adds to the itemQuantities
        int notTaken = manager.TakeItem(inventoryID, itemQuantities, shieldItemID, quantity: 2);
        Assert.That(notTaken, Is.EqualTo(0));
        Assert.That(itemQuantities[shieldItemID], Is.EqualTo(2));

        // Only 1 left to take, should return 1 untaken and update itemQuantities
        notTaken = manager.TakeItem(inventoryID, itemQuantities, shieldItemID, quantity: 2);
        Assert.That(notTaken, Is.EqualTo(1));
        Assert.That(itemQuantities[shieldItemID], Is.EqualTo(3));
    }

    [Test]
    public void TakeItem_StaticIDMultipleQuantityWithModified_Success()
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(6);
        manager.AddItemToInventory(inventoryID, shieldItemID);
        manager.AddItemToInventory(inventoryID, swordItemID, 2);
        manager.AddItemToInventory(inventoryID, modifiedSwordItemID);

        Dictionary<string, int> itemQuantities = new Dictionary<string, int>();

        // Taking generic swords takes the modified item first, plus remaining from generic
        int notTaken = manager.TakeItem(inventoryID, itemQuantities, swordItemID, 2);
        Assert.That(notTaken, Is.EqualTo(0));
        Assert.That(itemQuantities[swordItemID], Is.EqualTo(1));
        Assert.That(itemQuantities[modifiedSwordItemID], Is.EqualTo(1));

        // Takes remaining swords, no more modified
        notTaken = manager.TakeItem(inventoryID, itemQuantities, swordItemID, 2);
        Assert.That(notTaken, Is.EqualTo(1));
        Assert.That(itemQuantities[swordItemID], Is.EqualTo(2));
        Assert.That(itemQuantities[modifiedSwordItemID], Is.EqualTo(1));
    }

    [Test]
    public void TakeItem_ModifiedID_Success()
    {
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(6);
        manager.AddItemToInventory(inventoryID, shieldItemID);
        manager.AddItemToInventory(inventoryID, swordItemID, 2);
        manager.AddItemToInventory(inventoryID, modifiedSwordItemID);

        Dictionary<string, int> itemQuantities = new Dictionary<string, int>();

        // Taking a specific instance works
        int notTaken = manager.TakeItem(inventoryID, itemQuantities, modifiedSwordItemID);
        Assert.That(notTaken, Is.EqualTo(0));
        Assert.That(itemQuantities[modifiedSwordItemID], Is.EqualTo(1));

        // Does not take any generic swords if modified is not present
        notTaken = manager.TakeItem(inventoryID, itemQuantities, modifiedSwordItemID);
        Assert.That(notTaken, Is.EqualTo(1));
        Assert.That(itemQuantities[modifiedSwordItemID], Is.EqualTo(1));
    }

    [Test]
    public void TakeItem_StackedID_Success()
    {
        // TODO: Rework this test when able to specify slots when adding items
        InventoryManager manager = new InventoryManager(sharedItemManager);
        string inventoryID = manager.CreateInventory(6);

        // Expected slots: [(healthPotion, 10), (shield, 1), (healthPotion, 3)]
        manager.AddItemToInventory(inventoryID, healthPotionID, 10);
        manager.AddItemToInventory(inventoryID, shieldItemID);
        int index;
        bool found = manager.FindItem(inventoryID, healthPotionID, out index);
        Assert.That(found, Is.True);
        Assert.That(index, Is.EqualTo(0));

        manager.AddItemToInventory(inventoryID, healthPotionID, 3);
        found = manager.FindItem(inventoryID, healthPotionID, out index, startIndex: 1);
        Assert.That(found, Is.True);
        Assert.That(index, Is.EqualTo(2));

        Dictionary<string, int> itemQuantities = new Dictionary<string, int>();

        // Takes from multiple slots, should consume smaller stack from end first
        int notTaken = manager.TakeItem(inventoryID, itemQuantities, healthPotionID, 6);
        Assert.That(notTaken, Is.EqualTo(0));
        Assert.That(itemQuantities[healthPotionID], Is.EqualTo(6));

        found = manager.FindItem(inventoryID, healthPotionID, out index, startIndex: 1);
        Assert.That(found, Is.False);

        // Takes multiple from a stack fine, leaves correct number untaken
        notTaken = manager.TakeItem(inventoryID, itemQuantities, healthPotionID, 10);
        Assert.That(notTaken, Is.EqualTo(3));
        Assert.That(itemQuantities[healthPotionID], Is.EqualTo(13));
    }
}
