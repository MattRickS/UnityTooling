using UnityEngine;
// using UnityEngine.SceneManagement;


namespace GameServices
{
    public class Bootstrapper : MonoBehaviour
    {
        public string inventoryFile = "InventoryService.json";
        public Inventory.InventoryService inventoryService;

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // public void Initialize()
        public void Start()
        {
            ServiceLocator.Initialize();

            // Inventory Service
            if (FileManager.FileExists(inventoryFile))
            {
                inventoryService.Load(inventoryFile);
            }
            ServiceLocator.Instance.Register<Inventory.InventoryService>(inventoryService);

            // TODO: Whatever loading pattern is needed
            // SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        }
    }
}
