using UnityEngine;

namespace GeniesGambit.Level
{
    public class KeyMechanicManager : MonoBehaviour
    {
        public static KeyMechanicManager Instance { get; private set; }
        public static bool IsKeyMechanicActive { get; private set; } = false;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ActivateKeyMechanic()
        {
            IsKeyMechanicActive = true;
            Debug.Log("[KeyMechanic] Key mechanic activated! All gates are now locked.");

            KeyCollectible.ResetKey();

            GateController[] gates = FindObjectsByType<GateController>(FindObjectsSortMode.None);
            foreach (var gate in gates)
            {
                gate.SetGateState(true);
                gate.RefreshGateState();
            }
        }

        public void ResetKeyMechanic()
        {
            IsKeyMechanicActive = false;
            Debug.Log("[KeyMechanic] Key mechanic reset for new game.");
        }

        public bool ShouldKeysBeVisible()
        {
            return IsKeyMechanicActive;
        }

        public bool ShouldGateCheckForKey()
        {
            return IsKeyMechanicActive;
        }
    }
}
