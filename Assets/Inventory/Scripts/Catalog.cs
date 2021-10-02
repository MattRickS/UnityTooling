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
        private Dictionary<string, ItemData> itemMapping;

        public static Catalog Create(List<ItemData> items)
        {
            Catalog catalog = ScriptableObject.CreateInstance<Catalog>();
            catalog.items = items;
            catalog.itemMapping = new Dictionary<string, ItemData>();
            foreach (ItemData item in items)
            {
                catalog.itemMapping.Add(item.Id(), item);
            }
            return catalog;
        }

        public ItemData GetItemData(string id)
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
            // TODO:
            // Something weird is happening here - we're seeing errors for duplicate
            // names, but each name is using the default values, ie, "Armour.".
            // It's triggered by the existing MasterCatalog, so presumably it's
            // deserializing multiple times, once without data. The Count is correct
            // however, so perhaps it's initialising once with empty data and
            // then populating after?
            foreach (ItemData item in items)
            {
                // Hack to avoid the error for now
                if (!string.IsNullOrEmpty(item.itemName))
                    itemMapping.Add(item.Id(), item);
            }
        }
    }

}