using UnityEngine;
using GeniesGambit.Core;

namespace GeniesGambit.Player
{
    /// <summary>
    /// Floating emblem above the currently controlled character.
    ///
    /// PREFAB MODE (recommended):
    ///   Assign "Emblem Prefab" in the Inspector.  The prefab controls its own
    ///   sprite, sorting layer, rotation, scale etc.  The script only drives
    ///   its world position (yOffset + bob) and show/hide.
    ///
    /// FALLBACK MODE:
    ///   Leave "Emblem Prefab" empty.  The script creates a sprite object from
    ///   the assigned "Emblem Sprite" (or a yellow diamond if nothing is assigned).
    ///   Use the Rendering / Position fields to configure it.
    /// </summary>
    public class PlayerEmblem : MonoBehaviour
    {
        [Header("Emblem Prefab (recommended)")]
        [Tooltip("Drag a prefab here. It will be instantiated at runtime and positioned above the active player. Leave empty to use the sprite fallback below.")]
        [SerializeField] GameObject emblemPrefab;

        [Header("Fallback Sprite (used only when Emblem Prefab is empty)")]
        [SerializeField] Sprite emblemSprite;
        [SerializeField] Vector2 size = new Vector2(0.6f, 0.6f);
        [SerializeField] string sortingLayerName = "Default";
        [SerializeField] int sortingOrder = 20;
        [SerializeField] float rotationZ = 0f;

        [Header("Position")]
        [SerializeField] float yOffset = 1.0f;

        [Header("Bob Animation")]
        [SerializeField] float bobAmplitude = 0.12f;
        [SerializeField] float bobSpeed = 2.5f;

        GameObject _emblemGO;
        Quaternion _baseRotation;   // baked from prefab or fallback rotationZ
        Transform _target;

        void Awake()
        {
            if (emblemPrefab != null)
            {
                // Instantiate at scene root — the prefab owns its sorting layer, rotation, scale.
                _emblemGO = Instantiate(emblemPrefab, Vector3.zero, emblemPrefab.transform.rotation);
                _emblemGO.name = "__PlayerEmblemVisual__";
                _baseRotation = emblemPrefab.transform.rotation;
            }
            else
            {
                // Procedural fallback
                _emblemGO = new GameObject("__PlayerEmblemVisual__");
                var sr = _emblemGO.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;

                if (emblemSprite != null)
                {
                    sr.sprite = emblemSprite;
                    float ppu = emblemSprite.pixelsPerUnit;
                    _emblemGO.transform.localScale = new Vector3(
                        size.x / (emblemSprite.rect.width / ppu),
                        size.y / (emblemSprite.rect.height / ppu),
                        1f);
                }
                else
                {
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
                    sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
                    Debug.LogWarning("[PlayerEmblem] No prefab or sprite assigned — yellow diamond fallback.");
                }

                _baseRotation = Quaternion.Euler(0f, 0f, rotationZ);
            }

            _emblemGO.SetActive(false);
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
            if (_emblemGO != null)
                _emblemGO.SetActive(_target != null);
        }

        void Update()
        {
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
                if (_emblemGO != null)
                    _emblemGO.SetActive(_target != null);
            }

            if (_target == null || _emblemGO == null) return;

            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            _emblemGO.transform.position = _target.position + new Vector3(0f, yOffset + bob, -0.1f);
            _emblemGO.transform.rotation = _baseRotation;
        }
    }
}
