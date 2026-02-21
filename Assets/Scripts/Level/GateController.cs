using UnityEngine;

namespace GeniesGambit.Level
{
    public class GateController : MonoBehaviour
    {
        [SerializeField] Sprite openGateSprite;
        [SerializeField] Sprite closedGateSprite;
        
        SpriteRenderer _spriteRenderer;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            Initialize();
        }

        void Initialize()
        {
            if (_spriteRenderer == null) return;
            
            if (KeyMechanicManager.IsKeyMechanicActive)
            {
                if (closedGateSprite != null)
                {
                    _spriteRenderer.sprite = closedGateSprite;
                    Debug.Log("[Gate] Gate initialized as closed (key mechanic active)");
                }
            }
            else
            {
                if (openGateSprite != null)
                {
                    _spriteRenderer.sprite = openGateSprite;
                    Debug.Log("[Gate] Gate initialized as open (key mechanic not active)");
                }
            }
        }

        public void SetGateState(bool locked)
        {
            if (_spriteRenderer == null) return;
            
            if (locked && closedGateSprite != null)
            {
                _spriteRenderer.sprite = closedGateSprite;
            }
            else if (!locked && openGateSprite != null)
            {
                _spriteRenderer.sprite = openGateSprite;
            }
        }

        public void OpenGate()
        {
            if (openGateSprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = openGateSprite;
                Debug.Log("[Gate] Gate opened!");
            }
        }
    }
}
