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
        Dictionary<string, ItemData> itemMapping;

        public int Count() { return items.Count; }

        public ItemData GetItem(string id)
        {
            ItemData data;
            if (itemMapping.TryGetValue(id, out data))
            {
                return data;
            }
            return null;
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            itemMapping = new Dictionary<string, ItemData>();
            foreach (ItemData item in items)
            {
                itemMapping.Add(item.Id(), item);
            }
        }
    }

}