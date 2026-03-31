using UnityEngine;

public class Ball : MonoBehaviour
{
    public enum BallType
    {
        Swing,
        Spin
    }

    public enum Direction
    {
        Left = -1,
        Right = 1
    }

    [Header("Ball Type")]
    public BallType ballType = BallType.Swing;

    [Header("Throw Settings")]
    public float speed = 20f;
    public float launchAngleVertical = 10f;
    public float launchAngleHorizontal = 0f;

    [Header("Swing Settings")]
    public float swingStrength = 2f;
    public Direction swingDirection = Direction.Right;
    public float maxSwing = 5f;

    [Header("Spin Settings")]
    [Tooltip("Spin angle in degrees")]
    [Range(0f, 25f)]
    public float spinStrength = 10f;
    public Direction spinDirection = Direction.Right;

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
        rb.isKinematic = true;
        transform.position = initialPosition;

        hasBounced = false;
        isSwingActive = false;
        timeInAir = 0f;
    }

    public void LaunchBall()
    {
        ResetBall();

        float angleRad = launchAngleVertical * Mathf.Deg2Rad;

        Quaternion yawRotation = Quaternion.Euler(0f, launchAngleHorizontal, 0f);
        Vector3 forward = yawRotation * transform.forward;

        Vector3 velocity = forward * speed * Mathf.Cos(angleRad);
        velocity.y = speed * Mathf.Sin(angleRad);

        rb.isKinematic = false;
        rb.linearVelocity = velocity;

        // Activate swing only if swing mode
        isSwingActive = (ballType == BallType.Swing);
        timeInAir = 0f;
    }

    void FixedUpdate()
    {
        if (ballType == BallType.Swing && isSwingActive && !hasBounced)
        {
            ApplySwing();
        }
    }

    void ApplySwing()
    {
        timeInAir += Time.fixedDeltaTime;

        float swingFactor = timeInAir * swingStrength;
        swingFactor = Mathf.Clamp(swingFactor, 0, maxSwing);

        Vector3 velocityDir = rb.linearVelocity.normalized;

        // Perpendicular sideways direction
        Vector3 sideDir = Vector3.Cross(Vector3.up, velocityDir).normalized * (int)swingDirection;

        rb.linearVelocity += sideDir * swingFactor * Time.fixedDeltaTime;
    }

    void ApplySpin()
    {
        Vector3 velocity = rb.linearVelocity;

        float speed = velocity.magnitude;

        // Define spin angle (in degrees)
        float spinAngle = spinStrength * (int)spinDirection;

        // Rotate velocity around Y-axis
        Quaternion spinRotation = Quaternion.AngleAxis(spinAngle, Vector3.up);

        Vector3 newDir = spinRotation * velocity.normalized;

        rb.linearVelocity = newDir * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBounced && collision.gameObject.CompareTag("Ground"))
        {
            hasBounced = true;

            // Stop swing always
            isSwingActive = false;

            // Apply spin ONLY if spin mode
            if (ballType == BallType.Spin)
            {
                ApplySpin();
            }

            Debug.Log("Ball bounced");
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
