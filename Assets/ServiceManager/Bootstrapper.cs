using UnityEngine;
using UnityEngine.SceneManagement;


namespace GameServices
{
    public class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public void Initialize()
        {
            ServiceLocator.Initialize();

            ServiceLocator.Instance.Register<Inventory.InventoryService>(new Inventory.InventoryService());

            SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        }
    }
}
