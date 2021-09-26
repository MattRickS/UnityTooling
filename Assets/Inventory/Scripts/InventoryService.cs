using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    public class InventoryService : GameServices.IGameService
    {
        [SerializeField]
        public Catalog catalog { get; private set; }
        // public InventoryFactory factory { get; private set; }

        public ItemData GetItemData(string id)
        {
            return catalog.GetItem(id);
        }

    }
}