using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class MenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] string gameSceneName = "SampleScene";

    [Header("Intro Video (Optional)")]
    [SerializeField] VideoPlayer introVideoPlayer;
    [SerializeField] GameObject introVideoRoot;

    bool _isPlayingIntro;

    void Awake()
    {
        if (introVideoRoot != null)
            introVideoRoot.SetActive(false);

        if (introVideoPlayer != null)
            introVideoPlayer.loopPointReached += OnIntroFinished;
    }

    void OnDestroy()
    {
        if (introVideoPlayer != null)
            introVideoPlayer.loopPointReached -= OnIntroFinished;
    }

    void Update()
    {
        if (!_isPlayingIntro) return;

        bool skipPressed = (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                        || (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame));

        if (skipPressed)
            SkipIntro();
    }

    // Function for your "Start" button
    public void StartGame()
    {
        if (_isPlayingIntro) return;

        if (introVideoPlayer == null || introVideoPlayer.clip == null)
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        _isPlayingIntro = true;
        if (introVideoRoot != null)
            introVideoRoot.SetActive(true);

        introVideoPlayer.time = 0;
        introVideoPlayer.Play();
    }

    public void SkipIntro()
    {
        if (!_isPlayingIntro) return;

        if (introVideoPlayer != null)
            introVideoPlayer.Stop();

        FinishAndLoadGame();
    }

    void OnIntroFinished(VideoPlayer _)
    {
        if (!_isPlayingIntro) return;
        FinishAndLoadGame();
    }

    void FinishAndLoadGame()
    {
        _isPlayingIntro = false;
        if (introVideoRoot != null)
            introVideoRoot.SetActive(false);

        SceneManager.LoadScene(gameSceneName);
    }

    // Function for your "Controls" button
    public void OpenControls()
    {
        SceneManager.LoadScene("Control");
    }

    // Pro-tip: A "Back" button to return to the menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}