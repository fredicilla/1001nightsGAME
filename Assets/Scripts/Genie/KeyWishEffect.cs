using UnityEngine;
using GeniesGambit.Level;

namespace GeniesGambit.Genie
{
    public class KeyWishEffect : MonoBehaviour
    {
        public static void ApplyKeyWish()
        {
            Debug.Log("[KeyWish] Applying Key Wish effect...");
            
            if (KeyMechanicManager.Instance != null)
            {
                KeyMechanicManager.Instance.ActivateKeyMechanic();
            }
            else
            {
                Debug.LogWarning("[KeyWish] KeyMechanicManager not found! Creating one...");
                var managerGO = new GameObject("KeyMechanicManager");
                managerGO.AddComponent<KeyMechanicManager>();
                KeyMechanicManager.Instance.ActivateKeyMechanic();
            }
            
            Debug.Log("[KeyWish] The Genie has locked all gates! You must find keys to proceed.");
        }
    }
}
