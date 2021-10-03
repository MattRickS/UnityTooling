using NUnit.Framework;
using Inventory;


public class CatalogTest : ItemTestHarness
{
    [Test]
    public void GetItemData_ValidID_FindsItem()
    {
        ItemData item = catalog.GetItemData(shieldItemID);
        Assert.That(item, Is.EqualTo(item_shield));

        item = catalog.GetItemData(swordItemID);
        Assert.That(item, Is.EqualTo(item_sword));
    }

    [TestCaseSource("invalidItemIDProvider")]
    public void GetItemData_InvalidID_ReturnsNull(string name)
    {
        ItemData result = catalog.GetItemData(name);
        Assert.That(result, Is.Null);
    }
}
