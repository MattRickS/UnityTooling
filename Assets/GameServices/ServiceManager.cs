using UnityEngine;


namespace GameServices
{
    public class ServiceManager : MonoBehaviour
    {
        public InventoryService inventoryService;

        public void Start()
        {
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
