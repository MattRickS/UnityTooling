using System.Collections.Generic;
using NUnit.Framework;

using Inventory;


public class ItemQuantitiesTest : ItemTestHarness
{
    [Test]
    public void ItemQuantities_test()
    {
        ItemQuantities itemQuantities = new ItemQuantities(sharedItemManager);
        itemQuantities.Add(swordItemID, 3);
        itemQuantities.Add(shieldItemID, 1);
    }
}
