using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GeniesGambit.Core;
using System.Collections;

namespace GeniesGambit.UI
{
    public class IterationUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI iterationText;
        [SerializeField] TextMeshProUGUI roleText;

        [Header("Transition Banner")]
        [SerializeField] GameObject transitionBanner;
        [SerializeField] TextMeshProUGUI transitionText;
        [SerializeField] float bannerDuration = 0.3f;
        [SerializeField] float fadeSpeed = 10f;

        int _lastIteration = 0;
        CanvasGroup _bannerCanvasGroup;
        bool _isRewindBanner = false;

        void Start()
        {
            // Create transition banner if not assigned
            if (transitionBanner == null)
            {
                CreateTransitionBanner();
            }
            else if (transitionBanner.GetComponent<CanvasGroup>() == null)
            {
                _bannerCanvasGroup = transitionBanner.AddComponent<CanvasGroup>();
            }
            else
            {
                _bannerCanvasGroup = transitionBanner.GetComponent<CanvasGroup>();
            }

            if (transitionBanner != null)
            {
                transitionBanner.SetActive(false);
            }
        }

        void CreateTransitionBanner()
        {
            // Find or create canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // Create banner object
            transitionBanner = new GameObject("TransitionBanner");
            transitionBanner.transform.SetParent(canvas.transform, false);

            // Setup RectTransform to cover center of screen
            RectTransform bannerRect = transitionBanner.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 0.4f);
            bannerRect.anchorMax = new Vector2(1, 0.6f);
            bannerRect.offsetMin = Vector2.zero;
            bannerRect.offsetMax = Vector2.zero;

            // Add background image
            Image bgImage = transitionBanner.AddComponent<Image>();
            bgImage.color = new Color(0.55f, 0.4f, 0.15f, 0.75f);

            // Add canvas group for fading
            _bannerCanvasGroup = transitionBanner.AddComponent<CanvasGroup>();

            // Create text
            GameObject textObj = new GameObject("TransitionText");
            textObj.transform.SetParent(transitionBanner.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            transitionText = textObj.AddComponent<TextMeshProUGUI>();
            transitionText.alignment = TextAlignmentOptions.Center;
            transitionText.fontSize = 48;
            transitionText.color = Color.white;
            transitionText.fontStyle = FontStyles.Bold;

            transitionBanner.SetActive(false);
        }

        void Update()
        {
            if (IterationManager.Instance == null)
            {
                if (iterationText != null)
                    iterationText.text = "";
                if (roleText != null)
                    roleText.text = "";
                return;
            }

            int iteration = IterationManager.Instance.CurrentIteration;
            int totalIterations = IterationManager.Instance.TotalIterations;

            if (iteration != _lastIteration && iteration > 0)
            {
                ShowTransitionBanner(iteration);
                _lastIteration = iteration;
            }

            if (iterationText != null)
            {
                iterationText.text = $"Iteration: {iteration}/{totalIterations}";
            }

            if (roleText != null)
            {
                var def = IterationManager.Instance.CurrentDef;
                if (def != null)
                {
                    switch (def.role)
                    {
                        case IterationRole.Hero:
                            if (def.replayEnemy1 || def.replayEnemy2)
                                roleText.text = "You control: HERO - Dodge the ghosts and reach the gate!";
                            else
                                roleText.text = "You control: HERO - Reach the gate!";
                            break;
                        case IterationRole.Enemy1:
                            roleText.text = "You control: ENEMY #1 - Stop the ghost hero!";
                            break;
                        case IterationRole.Enemy2:
                            roleText.text = "You control: ENEMY #2 - Stop the ghost hero!";
                            break;
                    }
                }
                else
                {
                    roleText.text = "";
                }
            }
        }

        void ShowTransitionBanner(int iteration)
        {
            if (transitionBanner == null || transitionText == null) return;

            string message = iteration switch
            {
                1 => "ITERATION 1\nYou are the HERO!",
                2 => "ITERATION 2\nNow you are ENEMY #1!",
                3 => "ITERATION 3\nYou are the HERO again!",
                4 => "ITERATION 4\nNow you are ENEMY #2!",
                5 => "ITERATION 5\nHERO - Dodge both ghosts!",
                6 => "ITERATION 6\nHERO - Dodge both ghosts!",
                7 => "ITERATION 7\nFinal Challenge - HERO!",
                _ => $"ITERATION {iteration}"
            };

            _isRewindBanner = false;
            transitionText.text = message;
            StopAllCoroutines();
            StartCoroutine(ShowBannerCoroutine());
        }

        public void ShowRewindBanner(int targetIteration)
        {
            if (transitionBanner == null || transitionText == null) return;

            string message = $"REWINDING TO ITERATION {targetIteration}...";
            _isRewindBanner = true;
            transitionText.text = message;
            StopAllCoroutines();
            StartCoroutine(ShowBannerCoroutine());
        }

        IEnumerator ShowBannerCoroutine()
        {
            transitionBanner.SetActive(true);
            _bannerCanvasGroup.alpha = 0f;

            var bgImage = transitionBanner.GetComponent<Image>();
            if (bgImage != null)
            {
                if (_isRewindBanner)
                {
                    bgImage.color = new Color(0.2f, 0.55f, 0.75f, 0.75f);
                }
                else
                {
                    bgImage.color = new Color(0.55f, 0.4f, 0.15f, 0.75f);
                }
            }

            // Fade in
            while (_bannerCanvasGroup.alpha < 1f)
            {
                _bannerCanvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            _bannerCanvasGroup.alpha = 1f;

            // Wait
            yield return new WaitForSeconds(bannerDuration);

            // Fade out
            while (_bannerCanvasGroup.alpha > 0f)
            {
                _bannerCanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }
            _bannerCanvasGroup.alpha = 0f;

            transitionBanner.SetActive(false);
        }
    }
}