using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Player
{
    /// <summary>
    /// Scene-level singleton.  Place this component on ANY GameObject in the scene
    /// (e.g. the Managers object) and assign the "Player emblem" sprite in the inspector.
    /// It automatically tracks whichever PlayerController is currently active
    /// (IsActive == true) and renders a bobbing emblem sprite above their head.
    /// No prefab changes are required.
    /// </summary>
    public class PlayerEmblem : MonoBehaviour
    {
        [Header("Emblem Sprite")]
        [SerializeField] Sprite emblemSprite;
        [SerializeField] Vector2 size = new Vector2(0.6f, 0.6f);

        [Header("Position")]
        [SerializeField] float yOffset = 1.8f;

        [Header("Bob Animation")]
        [SerializeField] float bobAmplitude = 0.12f;
        [SerializeField] float bobSpeed = 2.5f;

        GameObject _emblemGO;
        SpriteRenderer _sr;
        Transform _target;   // transform of the currently active player

        void Awake()
        {
            // Create the floating sprite as a root-level object so it moves freely
            _emblemGO = new GameObject("__PlayerEmblemVisual__");
            _sr = _emblemGO.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = 20;
            _emblemGO.SetActive(false);
        }

        void Start()
        {
            if (emblemSprite != null)
            {
                _sr.sprite = emblemSprite;
                float ppu = emblemSprite.pixelsPerUnit;
                float nativeW = emblemSprite.rect.width / ppu;
                float nativeH = emblemSprite.rect.height / ppu;
                _emblemGO.transform.localScale = new Vector3(
                    size.x / nativeW,
                    size.y / nativeH,
                    1f);
            }
            else
            {
                // No sprite assigned — create a bright yellow diamond so the emblem is always visible
                var tex = new Texture2D(32, 32);
                var pixels = new Color[32 * 32];
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                    {
                        int dx = Mathf.Abs(x - 16), dy = Mathf.Abs(y - 16);
                        pixels[y * 32 + x] = (dx + dy <= 14) ? Color.yellow : Color.clear;
                    }
                tex.SetPixels(pixels);
                tex.Apply();
                _sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
                Debug.LogWarning("[PlayerEmblem] No emblem sprite assigned — using yellow diamond fallback.");
            }
            RefreshTarget();
        }

        void OnEnable()
        {
            GameManager.OnStateChanged += OnStateChanged;
            RefreshTarget();
        }

        void OnDisable()
        {
            GameManager.OnStateChanged -= OnStateChanged;
        }

        void OnDestroy()
        {
            if (_emblemGO != null)
                Destroy(_emblemGO);
        }

        void OnStateChanged(GameState old, GameState next) => RefreshTarget();

        /// <summary>Scan all PlayerControllers and pick the one that IsActive.</summary>
        void RefreshTarget()
        {
            _target = null;
            var all = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var pc in all)
            {
                if (pc != null && pc.isActiveAndEnabled && pc.IsActive)
                {
                    _target = pc.transform;
                    break;
                }
            }
            _emblemGO.SetActive(_target != null);
        }

        void Update()
        {
            // Re-scan every frame so we react instantly when the active player changes
            var all = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            Transform found = null;
            foreach (var pc in all)
            {
                if (pc != null && pc.isActiveAndEnabled && pc.IsActive)
                {
                    found = pc.transform;
                    break;
                }
            }

            if (found != _target)
            {
                _target = found;
                _emblemGO.SetActive(_target != null);
                if (_target != null)
                    Debug.Log($"[PlayerEmblem] Now tracking: {_target.name}");
            }

            if (_target == null) return;

            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            _emblemGO.transform.position = _target.position + new Vector3(0f, yOffset + bob, -0.1f);
            _emblemGO.transform.rotation = Quaternion.identity;
        }
    }
}
