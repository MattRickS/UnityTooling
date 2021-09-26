using System.Collections.Generic;
using UnityEngine;

/*
Discussion on storing items as ScriptableObjects
https://forum.unity.com/threads/best-practices-for-accessioning-inventory-data.692530/

SerializableDictionary from
https://wiki.unity3d.com/index.php/SerializableDictionary

Discussion on Serializable editor names
https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/

Script Serialization considerations
https://docs.unity3d.com/2021.2/Documentation/Manual/script-Serialization.html

ItemDefinitions will be serialized by unity (yay).
All item instances to be stored on service and manually serialized.

Service
.catalog
  .definitions[]
.modifiedItems[]
.inventories[]
.Save(path)
.Load(path)

An inventory could be a ScriptableObject that adds JSON serialization for runtime objects.
  This would allow pre-configuring inventories in the UI, but also saving state during build.

*** How to have persistent deterministic GUIDs ***

If we generate new each time it loads, they'll never match up with any saved state.
Ideally should use the existing asset GUID.

Method for getting GUID but AFAIK only valid in editor, not game:
    AssetDatabase.TryGetGUIDAndLocalFileIdentifier



*/

namespace Inventory
{
    public enum Category
    {
        Armour,
        Weapon,
        Miscellaneous,
    }

    public enum Statistic
    {
        Value,
        Weight,
    }

    [CreateAssetMenu(fileName = "Data", menuName = "Inventory/Item", order = 1)]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public Category category;
        public string description;
        [Range(1, 999)]
        public int maxStackSize = 1;
        public bool isConsumable = false;
        // TODO:
        // Sprite
        // Mesh?
        // Abilities (enum) list
        public SerializableDictionary<Statistic, int> statistics;

        public string Id() { return $"{category}.{itemName}"; }
        public bool IsStackable() { return maxStackSize > 1; }
        public int GetStat(Statistic stat)
        {
            int value;
            if (statistics.TryGetValue(stat, out value))
            {
                return value;
            }
            return 0;
        }
    }

}
