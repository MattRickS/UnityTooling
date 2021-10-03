using System;
using UnityEngine;

namespace GameServices
{
    /*
    Interface for managing items and inventories.

    ItemData and Catalogs are static data that can be freely referenced and shared.
    ModifiedItems and Inventories are stateful, so are tracked by the InventoryService
    and referenced via IDs. This ensures the (de)serialization is centralised to avoid
    data becoming duplicated.
    */
    [Serializable]
    public class InventoryService : Inventory.InventoryManager, IGameService
    {
        [SerializeField] private string saveName = "InventoryService";

        public bool Save()
        {
            // TODO: This will save the catalog into the JSON? Uneeded though.
            //       Could remove from the before serialisation?
            string json = JsonUtility.ToJson(this);
            return SaveManager.SaveJSON(saveName, json);
        }
        public bool Load()
        {
            if (!SaveManager.SaveExists(saveName))
            {
                return false;
            }
            string json;
            if (!SaveManager.LoadJSON(saveName, out json))
            {
                return false;
            }
            JsonUtility.FromJsonOverwrite(json, this);
            return true;
        }
    }
}