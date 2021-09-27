using UnityEngine;


namespace GameServices
{
    public class ServiceManager : MonoBehaviour
    {
        public Inventory.InventoryService inventoryService;

        public void Start()
        {
            ServiceLocator.Initialize();

            // Inventory Service
            ServiceLocator.Instance.Register<Inventory.InventoryService>(inventoryService);

            // Each service should load it's state
            foreach (IGameService service in ServiceLocator.Instance.Services())
            {
                if (!service.Load())
                {
                    Debug.LogWarning($"No data loaded for {service}");
                }
            }
        }
    }
}
