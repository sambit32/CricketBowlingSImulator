using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Throw Settings")]
    public float speed = 20f;
    public float launchAngleVertical = 10f;
    public float launchAngleHorizontal = 10f;

    private Rigidbody rb;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        isReseted = true;
    }
    private bool isReseted = false;
    public void ResetBall()
    {
        if (isReseted)
        {
            return;
        }
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.position = initialPosition;

        isReseted = true;
    }

    public void LaunchBall()
    {
        hasBounced = false; // IMPORTANT (reset bounce state)
        
        float angleRad = launchAngleVertical * Mathf.Deg2Rad;

        // Apply horizontal angle
        Quaternion yawRotation = Quaternion.Euler(0f, launchAngleHorizontal, 0f);
        Vector3 forward = yawRotation * transform.forward;

        Vector3 velocity = forward * speed * Mathf.Cos(angleRad);
        velocity.y = speed * Mathf.Sin(angleRad);

        rb.isKinematic = false;
        rb.linearVelocity = velocity;
        isReseted = false;
    }

    private bool hasBounced = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBounced && collision.gameObject.CompareTag("Ground"))
        {
            hasBounced = true;
            Debug.Log("Ball bounced!");
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
