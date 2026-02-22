using System.Collections.Generic;
using UnityEngine;

namespace GeniesGambit.Player
{
    [System.Serializable]
    public struct FrameData
    {
        public Vector3 position;
        public bool facingRight;
        public float timestamp;
        public float speed;  // horizontal speed magnitude for animation
    }

    public class MovementRecorder : MonoBehaviour
    {
        List<FrameData> _recordedFrames = new List<FrameData>();
        bool _isRecording = false;
        float _startTime;
        SpriteRenderer _spriteRenderer;
        Rigidbody2D _rb;

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void StartRecording()
        {
            _recordedFrames.Clear();
            _isRecording = true;
            _startTime = Time.time;
            Debug.Log("[MovementRecorder] Started recording");
        }

        public void StopRecording()
        {
            _isRecording = false;
            Debug.Log($"[MovementRecorder] Stopped recording. Total frames: {_recordedFrames.Count}");
        }

        void Update()
        {
            if (!_isRecording) return;

            FrameData frame = new FrameData
            {
                position = transform.position,
                facingRight = !_spriteRenderer.flipX,
                timestamp = Time.time - _startTime,
                speed = _rb != null ? Mathf.Abs(_rb.linearVelocity.x) : 0f
            };

            _recordedFrames.Add(frame);
        }

        public List<FrameData> GetRecording()
        {
            return new List<FrameData>(_recordedFrames);
        }

        public int GetFrameCount()
        {
            return _recordedFrames.Count;
        }
    }
}
