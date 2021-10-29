using System.Collections.Generic;
using UnityEngine;

namespace GameServices
{

    public class ServiceLocator
    {
        private readonly Dictionary<string, IGameService> services = new Dictionary<string, IGameService>();

        public static ServiceLocator Instance { get; private set; }

        protected ServiceLocator()
        {
            Instance ??= this;
        }

        public static void Initialize()
        {
            Instance = new ServiceLocator();
        }

        public T Get<T>() where T : IGameService
        {
            string key = typeof(T).Name;
            if (!services.ContainsKey(key))
            {
                Debug.LogError($"{key} not registered");
                throw new KeyNotFoundException();
            }
            return (T)services[key];
        }

        public void Register<T>(T service) where T : IGameService
        {
            string key = typeof(T).Name;
            if (services.ContainsKey(key))
            {
                Debug.LogError($"Attempted to register service of type {key} which is already registered.");
                return;
            }
            services.Add(key, service);
        }

        public void Unregister<T>(T service) where T : IGameService
        {
            string key = typeof(T).Name;
            if (!services.ContainsKey(key))
            {
                Debug.LogError($"Attempted to unregister service of type {key} which is not registered.");
                return;
            }
            services.Remove(key);
        }

        public IEnumerable<IGameService> Services()
        {
            foreach (IGameService service in services.Values)
                yield return service;
        }
    }
}
