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
        Animator _animator;
        float _startDelay = 0f;  // No delay - start immediately
        float _delayTimer = 0f;
        bool _delayComplete = false;

        void Awake()
        {
            // Use GetComponentInChildren as fallback in case the sprite is on a child object
            _spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            _animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        }

        public void StartPlayback(List<FrameData> recordedFrames)
        {
            _frames = recordedFrames;
            _currentFrameIndex = 0;
            _playbackStartTime = Time.time;
            _isPlaying = true;
            _delayTimer = 0f;
            _delayComplete = true;  // Start immediately, no delay

            if (_spriteRenderer != null)
            {
                Color ghostColor = _spriteRenderer.color;
                ghostColor.a = 1f;
                _spriteRenderer.color = ghostColor;
            }

            // Immediately set position to first frame so ghost doesn't appear at spawn point
            if (_frames != null && _frames.Count > 0)
            {
                transform.position = _frames[0].position;
                if (_spriteRenderer != null)
                    _spriteRenderer.flipX = !_frames[0].facingRight;
                if (_animator != null)
                    _animator.SetFloat("Speed", _frames[0].speed);
            }
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
            if (_spriteRenderer != null)
                _spriteRenderer.flipX = !frame.facingRight;

            // Drive animation from recorded speed
            if (_animator != null)
            {
                _animator.SetFloat("Speed", frame.speed);
            }

            // Ghost fell off the map — restart current iteration (not full reset)
            if (transform.position.y < -10f)
            {
                _isPlaying = false;
                var state = GameManager.Instance?.CurrentState ?? GameState.HeroTurn;
                if (state == GameState.HeroTurn || state == GameState.MonsterTurn)
                {
                    IterationManager.Instance?.RestartCurrentIteration();
                }
            }
        }

        public bool IsPlaybackFinished() => !_isPlaying;
    }
}
