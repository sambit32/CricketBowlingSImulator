using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Throw Settings")]
    public float speed = 20f;
    public float launchAngleVertical = 10f;
    public float launchAngleHorizontal = 0f;

    [Header("Swing Settings")]
    public float swingStrength = 2f;     // how strong the swing is
    public int swingDirection = 1;       // +1 right, -1 left
    public float maxSwing = 5f;          // clamp for realism

    private Rigidbody rb;

    private Vector3 initialPosition;

    private bool hasBounced = false;
    private bool isSwingActive = false;
    private float timeInAir = 0f;

    void Start()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void ResetBall()
    {
        // First enable physics temporarily to safely reset velocity
        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Then disable physics
        rb.isKinematic = true;

        transform.position = initialPosition;

        // Reset states
        hasBounced = false;
        isSwingActive = false;
        timeInAir = 0f;
    }

    public void LaunchBall()
    {
        ResetBall();

        float angleRad = launchAngleVertical * Mathf.Deg2Rad;

        // Apply horizontal angle
        Quaternion yawRotation = Quaternion.Euler(0f, launchAngleHorizontal, 0f);
        Vector3 forward = yawRotation * transform.forward;

        Vector3 velocity = forward * speed * Mathf.Cos(angleRad);
        velocity.y = speed * Mathf.Sin(angleRad);

        rb.isKinematic = false;
        rb.linearVelocity = velocity;

        // Activate swing
        isSwingActive = true;
        timeInAir = 0f;
    }

    void FixedUpdate()
    {
        if (isSwingActive && !hasBounced)
        {
            ApplySwing();
        }
    }

    void ApplySwing()
    {
        timeInAir += Time.fixedDeltaTime;

        // Gradual increase
        float swingFactor = timeInAir * swingStrength;
        swingFactor = Mathf.Clamp(swingFactor, 0, maxSwing);

        // Side direction (based on current movement, not transform)
        Vector3 velocityDir = rb.linearVelocity.normalized;
        Vector3 sideDir = Vector3.Cross(Vector3.up, velocityDir).normalized * swingDirection;

        // Apply small sideways force
        rb.linearVelocity += sideDir * swingFactor * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBounced && collision.gameObject.CompareTag("Ground"))
        {
            hasBounced = true;

            // STOP swing instantly
            isSwingActive = false;

            Debug.Log("Ball bounced - swing stopped");
        }
    }

    private void OnDrawGizmos()
    {
        float angleRad = launchAngleVertical * Mathf.Deg2Rad;

        Quaternion yawRotation = Quaternion.Euler(0f, launchAngleHorizontal, 0f);
        Vector3 forward = yawRotation * transform.forward;

        Vector3 velocity = forward * speed * Mathf.Cos(angleRad);
        velocity.y = speed * Mathf.Sin(angleRad);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
    }
}
