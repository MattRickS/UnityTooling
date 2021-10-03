using System.Collections.Generic;
using NUnit.Framework;

using Inventory;


public class ItemManagerTest : ItemTestHarness
{
    [TestCaseSource("validStaticItemIDProvider")]
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

    [TestCaseSource("invalidItemIDProvider")]
    public void CreateModifiedItemID_InvalidID_Throws(string name)
    {
        Assert.That(() => { sharedItemManager.CreateModifiedItemID(name); }, Throws.TypeOf<KeyNotFoundException>());
    }

    [TestCaseSource("validMixedItemIDProvider")]
    public void IsValidID_ValidID_True(string itemID)
    {
        Assert.That(sharedItemManager.IsValidID(itemID), Is.True);
    }

    [TestCaseSource("invalidItemIDProvider")]
    public void IsValidID_InvalidID_False(string itemID)
    {
        Assert.That(sharedItemManager.IsValidID(itemID), Is.False);
    }

    [TestCaseSource("validStaticItemIDProvider")]
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

    [TestCaseSource("validStaticItemIDProvider")]
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

    [TestCaseSource("invalidItemIDProvider")]
    public void GetItemData_InvalidID_null(string itemID)
    {
        Assert.That(sharedItemManager.GetItemData(itemID), Is.Null);
    }

    [TestCase(ItemManagerTest.shieldItemID, Statistic.Value, ExpectedResult = 100)]
    [TestCase(ItemManagerTest.shieldItemID, Statistic.Weight, ExpectedResult = 20)]
    [TestCase(ItemManagerTest.shieldItemID, Statistic.Attack, ExpectedResult = 0)]
    [TestCase(ItemManagerTest.shieldItemID, Statistic.Defense, ExpectedResult = 2)]
    [TestCase(ItemManagerTest.swordItemID, Statistic.Value, ExpectedResult = 150)]
    [TestCase(ItemManagerTest.swordItemID, Statistic.Weight, ExpectedResult = 10)]
    [TestCase(ItemManagerTest.swordItemID, Statistic.Attack, ExpectedResult = 3)]
    [TestCase(ItemManagerTest.swordItemID, Statistic.Defense, ExpectedResult = 0)]
    // modifiedSword is the same, but with a delta modifying the value
    [TestCase(ItemManagerTest.modifiedSwordItemID, Statistic.Value, ExpectedResult = 140)]
    [TestCase(ItemManagerTest.modifiedSwordItemID, Statistic.Weight, ExpectedResult = 10)]
    [TestCase(ItemManagerTest.modifiedSwordItemID, Statistic.Attack, ExpectedResult = 3)]
    [TestCase(ItemManagerTest.modifiedSwordItemID, Statistic.Defense, ExpectedResult = 0)]
    public int GetItemStatisticValue_ValidID_GetsValue(string itemID, Statistic statistic)
    {
        return sharedItemManager.GetItemStatisticValue(itemID, statistic);
    }

    [TestCaseSource("invalidItemIDProvider")]
    public void GetItemStatisticValue_InvalidID_Throws(string itemID)
    {
        Assert.That(
            () => { sharedItemManager.GetItemStatisticValue(itemID, Statistic.Value); },
            Throws.TypeOf<KeyNotFoundException>()
        );
    }

    [TestCase(ItemManagerTest.modifiedSwordItemID, Statistic.Value, ExpectedResult = -10)]
    [TestCase(ItemManagerTest.modifiedSwordItemID, Statistic.Weight, ExpectedResult = 0)]
    [TestCase(ItemManagerTest.modifiedSwordItemID, Statistic.Attack, ExpectedResult = 0)]
    [TestCase(ItemManagerTest.modifiedSwordItemID, Statistic.Defense, ExpectedResult = 0)]
    public int GetItemStatisticDeltaValue_ValidID_GetsValue(string itemID, Statistic statistic)
    {
        return sharedItemManager.GetItemStatisticDeltaValue(itemID, statistic);
    }

    [TestCaseSource("validStaticItemIDProvider")]
    public void GetItemStatisticDeltaValue_StaticID_Throws(string itemID)
    {
        Assert.That(
            () => { sharedItemManager.GetItemStatisticDeltaValue(itemID, Statistic.Value); },
            Throws.TypeOf<KeyNotFoundException>()
        );
    }

    [TestCaseSource("invalidItemIDProvider")]
    public void GetItemStatisticDeltaValue_InvalidID_Throws(string itemID)
    {
        Assert.That(
            () => { sharedItemManager.GetItemStatisticDeltaValue(itemID, Statistic.Value); },
            Throws.TypeOf<KeyNotFoundException>()
        );
    }

    [Test]
    public void SetItemStatisticDeltaValue_ModifiedID_UpdatedExistingItem()
    {
        ItemManager manager = new ItemManager(catalog);
        string knownID = manager.CreateModifiedItemID(swordItemID);

        // No new item created for existing modified item
        string itemID = manager.SetItemStatisticDeltaValue(knownID, Statistic.Value, -10);
        Assert.That(itemID, Is.EqualTo(knownID));
        int value = manager.GetItemStatisticDeltaValue(knownID, Statistic.Value);
        Assert.That(value, Is.EqualTo(-10));
    }

    [Test]
    public void SetItemStatisticDeltaValue_StaticID_NewModifiedItem()
    {
        ItemManager manager = new ItemManager(catalog);

        // New item created for static item ID
        string itemID = manager.SetItemStatisticDeltaValue(shieldItemID, Statistic.Weight, 50);
        Assert.That(itemID, Is.Not.EqualTo(modifiedSwordItemID));
        int weight = manager.GetItemStatisticDeltaValue(itemID, Statistic.Weight);
        Assert.That(weight, Is.EqualTo(50));
    }

    [TestCaseSource("invalidItemIDProvider")]
    public void SetItemStatisticDeltaValue_InvalidID_Throws(string itemID)
    {
        ItemManager manager = new ItemManager(catalog);
        Assert.That(
            () => { manager.SetItemStatisticDeltaValue(itemID, Statistic.Weight, 50); },
            Throws.TypeOf<KeyNotFoundException>()
        );
    }

    [Test]
    public void ModifyItemStatisticDeltaValue_ModifiedID_ModifiesValue()
    {
        ItemManager manager = new ItemManager(catalog);
        string itemID = manager.CreateModifiedItemID(swordItemID);
        manager.SetItemStatisticDeltaValue(itemID, Statistic.Value, -10);

        // Already modified -10, additional -10
        int value = manager.ModifyItemStatisticDeltaValue(itemID, Statistic.Value, -10);
        Assert.That(value, Is.EqualTo(-20));

        // No weight delta set yet
        int weight = manager.ModifyItemStatisticDeltaValue(itemID, Statistic.Weight, 35);
        Assert.That(weight, Is.EqualTo(35));
    }

    [TestCaseSource("validStaticItemIDProvider")]
    public void ModifyItemStatisticDeltaValue_StaticID_Throws(string itemID)
    {
        ItemManager manager = new ItemManager(catalog);
        Assert.That(
            () => { manager.ModifyItemStatisticDeltaValue(itemID, Statistic.Weight, 50); },
            Throws.TypeOf<KeyNotFoundException>()
        );
    }

    [TestCaseSource("invalidItemIDProvider")]
    public void ModifyItemStatisticDeltaValue_InvalidID_Throws(string itemID)
    {
        ItemManager manager = new ItemManager(catalog);
        Assert.That(
            () => { manager.ModifyItemStatisticDeltaValue(itemID, Statistic.Weight, 50); },
            Throws.TypeOf<KeyNotFoundException>()
        );
    }
}
