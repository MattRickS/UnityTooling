using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/*
Either all data is made of SOs and used to directly reference
  OR
Data is defined in an external file and SOs are generated from it at runtime

The former allows using SOs in the editor, but requires creating each SO.
The latter allows easy generation of items, but requires a unique ID to be used.

The latter sounds appealing, but those IDs would need to be defined as constants
somewhere to be realistically usable, requiring some sort of generation of them.
At that point, it's easier to just have tooling for generating SOs from an
external file when needed.
*/

namespace Inventory
{
    [CreateAssetMenu(fileName = "Data", menuName = "Inventory/Catalog", order = 1)]
    public class Catalog : ScriptableObject, ISerializationCallbackReceiver
    {
        // TODO: Find some way to change the element name to domain name without
        //       losing all the normal list features
        public List<ItemData> items;
        Dictionary<string, ItemData> mapping;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            // TryGetGUIDAndLocalFileIdentifier() to get guid as string
            // Convert to Guid class
            // Use as key
            // Guids then passed around for item identifiers
            mapping = new Dictionary<string, ItemData>();
            foreach (ItemData item in items)
            {
                mapping.Add(item.Id(), item);
            }
        }

        public ItemData GetItem(string id)
        {
            return mapping[id];
        }
    }

}