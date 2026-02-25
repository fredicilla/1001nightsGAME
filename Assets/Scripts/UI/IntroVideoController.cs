using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace GeniesGambit.UI
{
    public class IntroVideoController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] VideoPlayer videoPlayer;
        [SerializeField] GameObject skipText;

        [Header("Settings")]
        [SerializeField] string nextSceneName = "SampleScene";

        void Start()
        {
            if (videoPlayer == null)
            {
                Debug.LogError("[IntroVideoController] VideoPlayer reference is missing!");
                return;
            }

            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                LoadNextScene();
            }
        }

        void OnVideoFinished(VideoPlayer vp)
        {
            LoadNextScene();
        }

        void LoadNextScene()
        {
            Debug.Log($"[IntroVideoController] Loading {nextSceneName}...");
            SceneManager.LoadScene(nextSceneName);
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
