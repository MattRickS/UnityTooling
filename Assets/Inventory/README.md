# Usage
Inventory is designed to work with a Service Locator Pattern. The UnityTooling repo includes a basic GameServices Assembly that implements an InventoryService for use, which is just a subclass of the InventoryManager with Save/Load methods implemented by (de)serialising to a JSON file.

```C#
inventoryService = GameServices.ServiceLocator.Instance.Get<InventoryService>();
```

There is a ServiceManager MonoBehaviour that can be added as a component to a GameObject in the scene. The services can then be modified in the editor. For convenience, there are Load/Save options on the service to allow quick loading of saved states, or using a custom editor state.

## ItemData and Catalog
At the heart of the Inventory system are two types of ScriptableObject; ItemData and Catalog. Both can create instances from the Create Menu in Unity's UI. As ScriptableObjects, these are static, shared objects that cannot be modified during runtime. To be more explicit, they _can_ be modified at runtime but the changes will _never_ be saved; they should be treated like configuration.

### ItemData
ItemData represents the static data that describes an item. At present, this includes:
* Name
* Category : **At present, this is using an enum that limits choices to a very small selection of defaults. This will be changed in future.**
* Description
* Max Stack Size (default: 1, increase to make the item stackable)
* Is Consumable : Whether or not the item can be consumed.
* Statistics : A Dictionary of `Statistic`s mapped to integer values. Common examples include Value, Weight, Attack, Defense, etc... **At present, this is using an enum that limits choices to a very small selection of defaults. This will be changed in future.**

Users are expected to create any ItemData needed for their game.

### Catalog
A catalog is a selection of ItemData. The InventoryService requires a single Catalog (referred to as the Master Catalog) as the source of truth for what items can and can't be used in the game. This can be useful for only exposing a subset of created items.

Additional features intended to be added to Catalog:
* Weighting and RNG seed to simplify using catalogs as reward tables for dropped loot / randomised treasure.
* Catalog reference with Filters to allow using a subset of another catalog, eg, filter all weapons from the Master Catalog.

## IDs
Perhaps unexpectedly, the user is not expected to deal with Inventory or Item instances. Instead, the service provides IDs for each type, and methods for operating on them. The reason for this is to keep all Inventory objects within the InventoryService as they are stateful and will need to be serialised and deserialised between play sessions without creating duplicated instances. Combined with the service pattern, this also allows modifying any part of the InventoryService as long as the IDs are not changed.

### Item Instances
Each ItemData has a unique ID that is used to reference that item, meaning all instances refer to the same set of values. Some games may require modifying the values of a specific item instance however (eg, weapon durability damage, magazine clip rounds). To do this, the user can call `ItemManager.CreateModifiedItem(itemID)` to create a unique instance of that item. The method returns the new itemID to use. ItemData and ModifiedItem IDs are interchangeable for all non-modified item specific methods; Both are treated as referring to an instance of an item.

ModifiedItems can modify their statistic values as a delta, ie, it stores modifiers that are combined with the base ItemData statistic values to produce the result. Delta's can be set to an explicit value, or incrementally modified. The `ItemManager` provides methods for reading the combined value of a statistic, or the delta separately. An example use case would be:

```C#
itemID = "Weapon.Sword"
modifiedItemID = inventoryService.ItemManager.CreateModifiedItem(itemID);

void OnWeaponHit()
{
    // Incremental modifier, returns the total delta value
    inventoryService.ItemManager.ModifyItemStatisticDeltaValue(modifiedItemID, Statistic.Durability, -1);
    // GetItemStatisticValue combines any delta value (if it has one) in the result
    if (inventoryService.ItemManager.GetItemStatisticValue(modifiedItemID, Statistic.Durability) <= 0)
    {
        inventoryService.ItemManager.DestroyModifiedItemID(modifiedItemID);
        ...
    }
}
```

### Inventory Instances
Inventories are stored in the service, and are considered to be made up of ordered Slots. Most methods that operate on an inventory can operate on specific indexes, or allow the inventory to automatically organise it's contents. For example, to add an item to an Inventory:
```C#
Inventory inventory = inventoryService.CreateInventory(size: 10);
// Automatically adds as many of the item as it can to the next base space (stacked or empty slot)
int numShieldsAdded = inventoryService.AddItemToInventory(inventoryID, "Armour.Shield");
int numArrowsAdded = inventoryService.AddItemToInventory(inventoryID, "Ammo.PoisonArrow", quantity: 5);
// Attempts to add the item to a specific slot index.
int numBowsAdded = inventoryService.AddItemToInventoryIndex(inventoryID, "Weapon.Bow", 0);
```


# Assembly Definitions
Inventory uses the `Inventory` namespace and is built in one Assembly Definition. It does currently have a reference to another `SerializableTypes` Assembly Definition. This may be incorporated at a later date.

# Tests
Tests are included and can be run with the Unity Test Runner, or via the command line with
```powershell
& '...\Unity.exe' -batchmode -runTests -projectPath '...\UnityTooling'
```

CLI tests produces an XML file with the results.

# TODO
* Custom Exceptions
* Replace ID type of string with integer
* Stricter permissions; most things are public for convenience/serialization, should be locked down and use [SerializeField]/expose as properties where needed.
* Dynamic Statistics/Categories
* Write Editor GUIs
    * Can use master catalog for id lookup?
* Write UIs

* String metadata field on ItemData for arbitrary values?
    * Users can subclass and use their Type if needed
