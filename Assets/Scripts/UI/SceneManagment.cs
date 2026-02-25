using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [SerializeField] string videoPath = "intro.mp4";
    [SerializeField] string gameSceneName = "SampleScene";

    VideoPlayer _videoPlayer;
    RawImage _videoImage;
    GameObject _videoOverlay;
    bool _isPlayingVideo = false;
    bool _videoFinished = false;
    bool _sceneLoadStarted = false;

    // Function for your "Start" button
    public void StartGame()
    {
        if (_isPlayingVideo) return;
        StartCoroutine(PlayIntroVideo());
    }

    bool SkipInputPressed()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb != null && (kb.enterKey.wasPressedThisFrame ||
                           kb.numpadEnterKey.wasPressedThisFrame ||
                           kb.spaceKey.wasPressedThisFrame ||
                           kb.escapeKey.wasPressedThisFrame))
            return true;

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;

        return false;
    }

    IEnumerator PlayIntroVideo()
    {
        _isPlayingVideo = true;

        // --- Create fullscreen overlay ---
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("VideoCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasGO.AddComponent<CanvasScaler>();
        }

        _videoOverlay = new GameObject("VideoOverlay");
        _videoOverlay.transform.SetParent(canvas.transform, false);

        RectTransform rt = _videoOverlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Black background
        Image bg = _videoOverlay.AddComponent<Image>();
        bg.color = Color.black;

        // RawImage for video render texture
        GameObject rawImgGO = new GameObject("VideoRawImage");
        rawImgGO.transform.SetParent(_videoOverlay.transform, false);
        RectTransform rawRt = rawImgGO.AddComponent<RectTransform>();
        rawRt.anchorMin = Vector2.zero;
        rawRt.anchorMax = Vector2.one;
        rawRt.offsetMin = Vector2.zero;
        rawRt.offsetMax = Vector2.zero;

        _videoImage = rawImgGO.AddComponent<RawImage>();
        _videoImage.color = Color.white;

        // --- Setup VideoPlayer ---
        _videoPlayer = _videoOverlay.AddComponent<VideoPlayer>();
        _videoPlayer.playOnAwake = false;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.source = VideoSource.Url;

        // Try Assets path first (editor), then StreamingAssets
        string editorPath = System.IO.Path.Combine(Application.dataPath, videoPath);
        if (System.IO.File.Exists(editorPath))
            _videoPlayer.url = editorPath;
        else
            _videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, videoPath);

        Debug.Log($"[MenuController] Video URL: {_videoPlayer.url}");

        RenderTexture renderTex = new RenderTexture(Screen.width, Screen.height, 0);
        _videoPlayer.targetTexture = renderTex;
        _videoImage.texture = renderTex;

        _videoFinished = false;
        _videoPlayer.loopPointReached += OnVideoEnd;
        _videoPlayer.errorReceived += OnVideoError;

        // Prepare
        _videoPlayer.Prepare();

        float timeout = 5f;
        float elapsed = 0f;
        while (!_videoPlayer.isPrepared && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!_videoPlayer.isPrepared)
        {
            Debug.LogWarning("[MenuController] Video failed to prepare. Loading game directly.");
            LoadGameScene();
            yield break;
        }

        Debug.Log($"[MenuController] Video prepared. Length: {_videoPlayer.length}s. Playing...");
        _videoPlayer.Play();

        // Grace period so the Start-button click doesn't immediately skip
        yield return new WaitForSecondsRealtime(0.8f);

        // Wait for video to finish or user skip
        float maxWait = (float)_videoPlayer.length + 5f; // safety timeout
        float waited = 0f;
        while (!_videoFinished)
        {
            waited += Time.unscaledDeltaTime;

            // Fallback: video stopped playing on its own
            if (_videoPlayer == null || !_videoPlayer.isPlaying && waited > 1f)
            {
                Debug.Log("[MenuController] Video stopped playing. Transitioning.");
                break;
            }

            // Fallback: time-based end detection
            if (_videoPlayer != null && _videoPlayer.length > 0 &&
                _videoPlayer.time >= _videoPlayer.length - 0.2)
            {
                Debug.Log("[MenuController] Video reached end by time check.");
                break;
            }

            // Safety timeout
            if (waited >= maxWait)
            {
                Debug.LogWarning("[MenuController] Video safety timeout reached. Transitioning.");
                break;
            }

            // Skip via new Input System
            if (SkipInputPressed())
            {
                Debug.Log("[MenuController] User skipped video.");
                break;
            }

            yield return null;
        }

        LoadGameScene();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("[MenuController] loopPointReached fired.");
        _videoFinished = true;
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[MenuController] Video error: {message}");
        _videoFinished = true;
    }

    void LoadGameScene()
    {
        if (_sceneLoadStarted) return;
        _sceneLoadStarted = true;

        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();
            _videoPlayer.loopPointReached -= OnVideoEnd;
            _videoPlayer.errorReceived -= OnVideoError;
        }

        if (_videoOverlay != null)
            Destroy(_videoOverlay);

        _isPlayingVideo = false;
        Debug.Log($"[MenuController] Loading scene: {gameSceneName}");
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