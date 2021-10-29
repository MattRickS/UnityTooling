using UnityEngine;


namespace GameServices
{
    public class ServiceManager : MonoBehaviour
    {
        public InventoryService inventoryService;

        /*
        Looks like Awake() and OnEnable are called in the Editor: https://docs.unity3d.com/Manual/ExecutionOrder.html
        Obviously, this is only for MonoBehaviours.

        Inventory
            InventoryManager(itemDatabase, inventoryDatabase)
            .itemDatabase
            .inventoryDatabase

            InventoryService
            .defaultItemDatabase
            .defaultInventoryDatabase
            .defaultInventoryManager

            .Instance
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            static void StaticStart() {
                ItemDatabase itemDatabase = new ItemDatabase();
                InventoryDatabase inventoryDatabase = new InventoryDatabase();
                InventoryService inventoryService = new InventoryService(itemDatabase, inventoryDatabase);
            }

        ------------------------------------------------------------------------

        Inventory
            InventoryManager(itemDatabase, inventoryDatabase)
            .itemDatabase
            .inventoryDatabase

        GameServices
            ServiceLocator
            .Instance
            .Get<InventoryService>()

            static class Bootstrapper
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            static void StaticStart() {
                ItemDatabase itemDatabase = new ItemDatabase();
                InventoryDatabase inventoryDatabase = new InventoryDatabase();
                InventoryService inventoryService = new InventoryService(itemDatabase, inventoryDatabase);
                ServiceLocator.Instance.Register<InventoryService>(inventoryService);
            }

        EditorWindow (This now has a dependency on the service locator... FUCK)
        inventoryService = ServiceLocator.Instance.Get<InventoryService>()

        */

        // Don't use Resources folder, consider Addressable Assets instead if needed

        // ScriptableObject for the itemDatabase (list of items, built into map on load)
        // ScriptableObject for items makes them usable in editor - I think
        //   that's more useful than having to use a bespoke UI? Could be better
        //   to keep them as basic C# class only use is via Inventory UIs

        // Should move to a static Bootstrap class. Options
        // * ExecuteInEditMode
        // * RuntimeInitializeOnLoadMethod - does not support constructors
        // * static constructor: static Bootstrap() { initialise stuff }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public void StaticStart()
        {
            Debug.Log("Service Manager Awaking");
            ServiceLocator.Initialize();

            // Inventory Service
            ServiceLocator.Instance.Register<InventoryService>(inventoryService);

            // Each service should load it's state
            foreach (IGameService service in ServiceLocator.Instance.Services())
            {
                if (!service.LoadOnStart)
                {
                    Debug.LogWarning($"Load is disabled for {service}");
                }
                else if (!service.Load())
                {
                    // If data fails to load on startup, we don't want to save a corrupt version
                    Debug.LogError($"Failed to load data for {service}; Disabling Save.");
                    service.LoadOnStart = false;
                }
            }
        }

        public void OnApplicationQuit()
        {
            foreach (IGameService service in ServiceLocator.Instance.Services())
            {
                if (!service.SaveOnQuit)
                {
                    Debug.LogWarning($"Save is disabled for {service}");
                }
                else if (!service.Save())
                {
                    Debug.LogError($"Failed to save data for {service}");
                }
            }
        }

        public void LoadServices()
        {
            foreach (IGameService service in ServiceLocator.Instance.Services())
            {
                if (!service.Load())
                {
                    Debug.LogWarning($"No data loaded for {service}");
                }
            }
        }

        public void SaveServices()
        {
            foreach (IGameService service in ServiceLocator.Instance.Services())
            {
                if (!service.Save())
                {
                    Debug.LogError($"Failed to save data for {service}");
                }
            }
        }
    }
}
