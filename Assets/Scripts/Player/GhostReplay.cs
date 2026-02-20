using System.Collections.Generic;
using UnityEngine;

namespace GeniesGambit.Player
{
    public class GhostReplay : MonoBehaviour
    {
        List<FrameData> _frames;
        int _currentFrameIndex = 0;
        float _playbackStartTime;
        bool _isPlaying = false;
        SpriteRenderer _spriteRenderer;
        float _startDelay = 3f;
        float _delayTimer = 0f;
        bool _delayComplete = false;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void StartPlayback(List<FrameData> recordedFrames)
        {
            _frames = recordedFrames;
            _currentFrameIndex = 0;
            _playbackStartTime = Time.time;
            _isPlaying = true;
            _delayTimer = 0f;
            _delayComplete = false;

            Color ghostColor = _spriteRenderer.color;
            ghostColor.a = 0.5f;
            _spriteRenderer.color = ghostColor;

            Debug.Log($"[GhostReplay] Started playback with {_frames.Count} frames (3 second delay before movement)");
        }

        void Update()
        {
            if (!_isPlaying || _frames == null || _frames.Count == 0) return;

            if (!_delayComplete)
            {
                _delayTimer += Time.deltaTime;
                if (_delayTimer >= _startDelay)
                {
                    _delayComplete = true;
                    _playbackStartTime = Time.time;
                    Debug.Log("[GhostReplay] Delay complete! Ghost starting to move now!");
                }
                return;
            }

            float currentPlaybackTime = Time.time - _playbackStartTime;

            while (_currentFrameIndex < _frames.Count - 1 &&
                   _frames[_currentFrameIndex + 1].timestamp <= currentPlaybackTime)
            {
                _currentFrameIndex++;
            }

            if (_currentFrameIndex >= _frames.Count)
            {
                _isPlaying = false;
                Debug.Log("[GhostReplay] Playback finished");
                return;
            }

            FrameData frame = _frames[_currentFrameIndex];
            transform.position = frame.position;
            _spriteRenderer.flipX = !frame.facingRight;

            if (transform.position.y < -10f)
            {
                Debug.Log("[GhostReplay] Ghost fell off map!");
                var iterationManager = FindFirstObjectByType<Core.IterationManager>();
                if (iterationManager != null)
                {
                    iterationManager.ResetIterations();
                }
            }
        }

        public bool IsPlaybackFinished()
        {
            return !_isPlaying;
        }
    }
}
