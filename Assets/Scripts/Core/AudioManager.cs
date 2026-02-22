using UnityEngine;

namespace GeniesGambit.Core
{
    /// <summary>
    /// Central audio manager.  Add this component to any persistent GameObject
    /// (e.g. GameManager).  Assign AudioClips in the Inspector under each header.
    /// Call  AudioManager.Play(SoundID.Jump)  from anywhere.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        // ─── Sound IDs ────────────────────────────────────────────────────────────
        public enum SoundID
        {
            Jump,
            Land,
            Fall,
            CoinCollect,
            KeyCollect,
            Win,          // reaching the gate/flag
            GateOpen,
            SpikeDamage,
            Die,          // hit by apple
            Throw,        // throwing apple
            Rewind,
            WishScreenOpen,
            Select,       // choosing a wish
            NewRound,
            GameWin,
        }

        // ─── Inspector slots ──────────────────────────────────────────────────────
        [Header("Player")]
        [SerializeField] AudioClip jumpClip;
        [SerializeField] AudioClip landClip;
        [SerializeField] AudioClip fallClip;

        [Header("Combat")]
        [SerializeField] AudioClip throwClip;
        [SerializeField] AudioClip dieClip;
        [SerializeField] AudioClip spikeDamageClip;

        [Header("Collectibles")]
        [SerializeField] AudioClip coinCollectClip;
        [SerializeField] AudioClip keyCollectClip;

        [Header("Level")]
        [SerializeField] AudioClip winClip;          // reaching the gate
        [SerializeField] AudioClip gateOpenClip;

        [Header("UI / Meta")]
        [SerializeField] AudioClip rewindClip;
        [SerializeField] AudioClip wishScreenOpenClip;
        [SerializeField] AudioClip selectClip;       // choosing a wish
        [SerializeField] AudioClip newRoundClip;
        [SerializeField] AudioClip gameWinClip;

        [Header("Background Music")]
        [SerializeField] AudioClip musicClip;
        [SerializeField][Range(0f, 1f)] float musicVolume = 0.35f;

        [Header("SFX Volume")]
        [SerializeField][Range(0f, 1f)] float sfxVolume = 1f;

        AudioSource _sfxSource;
        AudioSource _musicSource;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // SFX source
            _sfxSource = GetComponent<AudioSource>();
            if (_sfxSource == null)
                _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            // Music source — separate so volume and loop are independent
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = musicVolume;
        }

        void Start()
        {
            if (musicClip != null)
            {
                _musicSource.clip = musicClip;
                _musicSource.Play();
            }
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        public static void Play(SoundID id)
        {
            if (Instance == null) return;
            AudioClip clip = Instance.ClipFor(id);
            if (clip == null) return;
            Instance._sfxSource.PlayOneShot(clip, Instance.sfxVolume);
        }

        // ─── Private helpers ──────────────────────────────────────────────────────

        AudioClip ClipFor(SoundID id) => id switch
        {
            SoundID.Jump => jumpClip,
            SoundID.Land => landClip,
            SoundID.Fall => fallClip,
            SoundID.CoinCollect => coinCollectClip,
            SoundID.KeyCollect => keyCollectClip,
            SoundID.Win => winClip,
            SoundID.GateOpen => gateOpenClip,
            SoundID.SpikeDamage => spikeDamageClip,
            SoundID.Die => dieClip,
            SoundID.Throw => throwClip,
            SoundID.Rewind => rewindClip,
            SoundID.WishScreenOpen => wishScreenOpenClip,
            SoundID.Select => selectClip,
            SoundID.NewRound => newRoundClip,
            SoundID.GameWin => gameWinClip,
            _ => null
        };
    }
}
