using BossFight;
using System.Collections.Generic;
using UnityEngine;

public class RecordingManager : MonoBehaviour
{
    private List<RecordedFrame> recording = new List<RecordedFrame>();
    private bool isRecording = false;
    private float recordingStartTime;
    private PlayerController playerController;
    private bool recordingStarted = false;
    private Transform characterModel;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        characterModel = transform.Find("CharacterModel");
    }

    public void StartRecording()
    {
        recording.Clear();
        isRecording = true;
        recordingStarted = false;
        recordingStartTime = 0;
        Debug.Log("📼 Recording prepared (waiting for game to start)");
    }

    public void StopRecording()
    {
        isRecording = false;
        Debug.Log($"📼 Recording stopped! Total frames: {recording.Count}");
    }

    private void FixedUpdate()
    {
        if (!isRecording || playerController == null) return;

        // Check if game has started (player is active)
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && gameManager.IsWaitingForInput)
        {
            return; // Don't record during freeze!
        }

        // First frame after freeze - set start time!
        if (!recordingStarted)
        {
            recordingStarted = true;
            recordingStartTime = Time.time;
            Debug.Log($"📼 Recording STARTED! Start time: {recordingStartTime}");
        }

        RecordedFrame frame = new RecordedFrame
        {
            timestamp = Time.time - recordingStartTime,
            position = transform.position,
            rotation = characterModel != null ? characterModel.rotation : transform.rotation,
            moveInput = playerController.MoveInput,
            jumpPressed = playerController.JumpInput,
            shootPressed = playerController.ShootInput,
            shootDirection = characterModel != null ? characterModel.forward : transform.forward
        };

        recording.Add(frame);
    }

    public List<RecordedFrame> GetRecording()
    {
        return new List<RecordedFrame>(recording);
    }

    public bool IsRecording => isRecording;
}
