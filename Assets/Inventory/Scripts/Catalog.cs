using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    [CreateAssetMenu(fileName = "Data", menuName = "Inventory/Catalog", order = 2)]
    public class Catalog : ScriptableObject, ISerializationCallbackReceiver
    {
        // TODO: Find some way to change the UI element name to domain name
        //       without losing all the normal list features
        public List<ItemData> items;
        Dictionary<string, ItemData> mapping;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            mapping = new Dictionary<string, ItemData>();
            foreach (ItemData item in items)
            {
                mapping.Add(item.Id(), item);
            }
        }

        public ItemData GetItem(string id)
        {
            ItemData data;
            if (mapping.TryGetValue(id, out data))
            {
                return data;
            }
            return null;
        }
    }

}