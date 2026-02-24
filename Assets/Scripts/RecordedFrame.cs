using UnityEngine;

[System.Serializable]
public class RecordedFrame
{
    public float timestamp;
    public Vector3 position;
    public Quaternion rotation;
    public Vector2 moveInput;
    public bool jumpPressed;
    public bool shootPressed;
    public Vector3 shootDirection;
}
