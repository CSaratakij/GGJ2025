using UnityEngine;
using Sirenix.OdinInspector;

namespace Game
{
    public class ServiceInitializer : MonoBehaviour
    {
        public static ServiceInitializer Instance = null;
        private static bool IsInitialized = false;

        [SerializeField, AssetsOnly] private GameObject[] services;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            if (!IsInitialized)
            {
                InitializeService();
                IsInitialized = true;
            }
        }

        private void InitializeService()
        {
            for (int i = 0; i < services.Length; i++)
            {
                var service = services[i];

                if (service)
                {
                    Instantiate(service);
                }
            }
        }
    }
}