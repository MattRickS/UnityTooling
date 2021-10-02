using UnityEngine;


namespace GameServices
{
    public class ServiceManager : MonoBehaviour
    {
        public bool loadOnStart = true;
        public bool saveOnQuit = true;
        public InventoryService inventoryService;

        public void Start()
        {
            ServiceLocator.Initialize();

            // Inventory Service
            ServiceLocator.Instance.Register<InventoryService>(inventoryService);

            // Each service should load it's state
            if (loadOnStart)
            {
                Load();
            }
        }

        public void OnApplicationQuit()
        {
            if (saveOnQuit)
            {
                Save();
            }
        }

        public void Load()
        {
            foreach (IGameService service in ServiceLocator.Instance.Services())
            {
                if (!service.Load())
                {
                    Debug.LogWarning($"No data loaded for {service}");
                }
            }
        }

        public void Save()
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
