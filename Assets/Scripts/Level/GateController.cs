using UnityEngine;

namespace GeniesGambit.Level
{
    public class GateController : MonoBehaviour
    {
        [SerializeField] Sprite openGateSprite;
        
        SpriteRenderer _spriteRenderer;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
