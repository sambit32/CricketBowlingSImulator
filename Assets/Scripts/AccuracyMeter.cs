using UnityEngine;

public class AccuracyMeter : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1f;

    [Header("Accuracy Values (7 parts)")]
    public float[] accuracyValues = new float[7];

    private float timer;
    private float currentPosition; // 0 → 1
    private bool isRunning = false;

    [SerializeField] private MeterUI meterUI;

    private void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime * speed;
        currentPosition = Mathf.PingPong(timer, 1f);
        meterUI.UpdateMeter(currentPosition);
    }

    /// <summary>
    /// Start the meter movement
    /// </summary>
    public void StartMeter()
    {
        isRunning = true;
    }

    /// <summary>
    /// Stop the meter movement (freezes at current position)
    /// </summary>
    public void StopMeter()
    {
        isRunning = false;
    }

    /// <summary>
    /// Optional: Reset and start from beginning
    /// </summary>
    public void ResetAndStart()
    {
        timer = 0f;
        currentPosition = 0f;
        isRunning = true;
    }

    public float GetCurrentAccuracy()
    {
        StopMeter();
        return accuracyValues[GetCurrentSegment()];
    }

    public int GetCurrentSegment()
    {
        float segmentSize = 1f / 7f;
        int index = Mathf.FloorToInt(currentPosition / segmentSize);
        return Mathf.Clamp(index, 0, 6);
    }

    public float GetNormalizedPosition()
    {
        return currentPosition;
    }
}