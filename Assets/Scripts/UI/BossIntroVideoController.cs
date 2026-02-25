using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace GeniesGambit.UI
{
    public class BossIntroVideoController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] VideoPlayer videoPlayer;
        [SerializeField] GameObject skipText;

        [Header("Settings")]
        [SerializeField] string bossSceneName = "BossFight";

        void Start()
        {
            if (videoPlayer == null)
            {
                Debug.LogError("[BossIntroVideoController] VideoPlayer reference is missing!");
                return;
            }

            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                LoadBossScene();
            }
        }

        void OnVideoFinished(VideoPlayer vp)
        {
            LoadBossScene();
        }

        void LoadBossScene()
        {
            Debug.Log($"[BossIntroVideoController] Loading {bossSceneName}...");
            SceneManager.LoadScene(bossSceneName);
        }

        void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
            }
        }
    }
}
