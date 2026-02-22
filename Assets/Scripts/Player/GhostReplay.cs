using System.Collections.Generic;
using UnityEngine;
using GeniesGambit.Core;

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
            ghostColor.a = 1f;
            _spriteRenderer.color = ghostColor;
        }

        /// <summary>Immediately halts playback — called before Destroy.</summary>
        public void Stop() => _isPlaying = false;

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
                return;
            }

            FrameData frame = _frames[_currentFrameIndex];
            transform.position = frame.position;
            _spriteRenderer.flipX = !frame.facingRight;

            // Only trigger a reset if the game is actually mid-round (not on win/wish screen)
            if (transform.position.y < -10f)
            {
                var state = GameManager.Instance?.CurrentState ?? GameState.HeroTurn;
                if (state == GameState.HeroTurn || state == GameState.MonsterTurn)
                {
                    _isPlaying = false;
                    IterationManager.Instance?.ResetIterations();
                }
                else
                {
                    // Game is on wish/complete screen — just stop, don't reset
                    _isPlaying = false;
                }
            }
        }

        public bool IsPlaybackFinished() => !_isPlaying;
    }
}
