using UnityEngine;

public class Ball : MonoBehaviour
{

    [Header("Ball Type")]
    public BallType ballType = BallType.Swing;

    [Header("References")]
    public Transform aimTarget;
    public Transform bounceMarker;

    [Header("Throw Settings")]
    public float speed = 20f;

    [Header("Swing Settings")]
    public float swingStrength = 2f;
    public Direction swingDirection = Direction.Right;
    public float maxSwing = 5f;

    [Header("Spin Settings")]
    [Range(0f, 20f)]
    public float spinAngle = 10f;
    public Direction spinDirection = Direction.Right;

    [Header("Pitch Limits")]
    public float pitchHalfWidth = 1.5f;

    [Header("Bounce Prediction")]
    public float simulationTimeStep = 0.02f;
    public int simulationSteps = 500;
    public float groundY = 0f;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private SphereCollider sphereCollider;
    private PhysicsMaterial originalMaterial;

    private bool hasBounced = false;
    private bool isSwingActive = false;
    private float timeInAir = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        rb.isKinematic = true;
        sphereCollider = GetComponent<SphereCollider>();
        originalMaterial = sphereCollider != null ? sphereCollider.material : null;
        UpdateBounceMarker();
    }

    void Update()
    {
        if (rb.isKinematic)
        {
            UpdateBounceMarker();
        }
    }

    void UpdateBounceMarker()
    {
        Vector3 velocity = CalculateLaunchVelocity();
        Vector3 bouncePoint = PredictBouncePoint(transform.position, velocity);

        bounceMarker.position = new Vector3(
            bouncePoint.x,
            groundY,
            bouncePoint.z
        );
    }

    public void ResetBall()
    {
        rb.isKinematic = true;
        transform.position = initialPosition;

        hasBounced = false;
        isSwingActive = false;
        timeInAir = 0f;
    }
    [SerializeField] private Vector3 LaunchVelocity = Vector3.zero;
    public void LaunchBall()
    {
        ResetBall();
        if(sphereCollider != null)
        {
            if(ballType == BallType.Swing)
            {
                sphereCollider.material = null;
            }
            else
            {
                sphereCollider.material = originalMaterial;
            }
        }
        Vector3 velocity = CalculateLaunchVelocity();

        LaunchVelocity = velocity; // For debugging/visualization

        rb.isKinematic = false;
        rb.linearVelocity = velocity;

        isSwingActive = (ballType == BallType.Swing);
        timeInAir = 0f;
    }

    // ------------------ LAUNCH ------------------
    Vector3 CalculateLaunchVelocity()
    {
        Vector3 start = transform.position;
        Vector3 target = aimTarget.position;

        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);

        float distanceXZ = toTargetXZ.magnitude;
        float height = toTarget.y;

        float speedSquared = speed * speed;

        float underRoot = speedSquared * speedSquared -
            gravity * (gravity * distanceXZ * distanceXZ + 2 * height * speedSquared);

        if (underRoot < 0)
        {
            Debug.LogWarning("Target too far/close for given speed");
            return transform.forward * speed;
        }

        float root = Mathf.Sqrt(underRoot);

        float angle = Mathf.Atan((speedSquared - root) / (gravity * distanceXZ));

        Vector3 direction = toTargetXZ.normalized;

        Vector3 velocity =
            direction * speed * Mathf.Cos(angle) +
            Vector3.up * speed * Mathf.Sin(angle);

        return velocity;
    }

    // ------------------ PREDICTION ------------------
    Vector3 PredictBouncePoint(Vector3 startPos, Vector3 initialVelocity)
    {
        Vector3 pos = startPos;
        Vector3 vel = initialVelocity;

        float simTime = 0f;

        for (int i = 0; i < simulationSteps; i++)
        {
            simTime += simulationTimeStep;

            // Gravity
            vel += Physics.gravity * simulationTimeStep;

            // Swing
            if (ballType == BallType.Swing)
            {
                float swingFactor = simTime * swingStrength;
                swingFactor = Mathf.Clamp(swingFactor, 0, maxSwing);

                // Smooth reduction near pitch edges
                float distanceFromCenter = Mathf.Abs(pos.x);
                float t = Mathf.InverseLerp(pitchHalfWidth, 0f, distanceFromCenter);
                float edgeFactor = Mathf.SmoothStep(0f, 1f, t);
                swingFactor *= edgeFactor;

                Vector3 velocityDir = vel.normalized;
                float speed = vel.magnitude;

                int dir = (int)swingDirection;
                Vector3 sideDir = Vector3.Cross(Vector3.up, velocityDir).normalized * dir;

                Vector3 newDir = (velocityDir + sideDir * swingFactor * simulationTimeStep).normalized;
                vel = newDir * speed;
            }

            // Move
            pos += vel * simulationTimeStep;

            // Hard clamp to pitch
            if (Mathf.Abs(pos.x) > pitchHalfWidth)
            {
                pos.x = Mathf.Sign(pos.x) * pitchHalfWidth;
                vel.x = 0f;
            }

            // Bounce detection
            if (pos.y <= groundY)
            {
                pos.y = groundY;
                return pos;
            }
        }

        return pos;
    }

    // ------------------ REAL PHYSICS ------------------
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

        // Smooth edge reduction
        float distanceFromCenter = Mathf.Abs(transform.position.x);
        float t = Mathf.InverseLerp(pitchHalfWidth, 0f, distanceFromCenter);
        float edgeFactor = Mathf.SmoothStep(0f, 1f, t);
        swingFactor *= edgeFactor;

        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        Vector3 velocityDir = velocity.normalized;
        int dir = (int)swingDirection;

        Vector3 sideDir = Vector3.Cross(Vector3.up, velocityDir).normalized * dir;

        Vector3 newDir = (velocityDir + sideDir * swingFactor * Time.fixedDeltaTime).normalized;

        rb.linearVelocity = newDir * speed;

        // Hard clamp (safety)
        if (Mathf.Abs(transform.position.x) > pitchHalfWidth)
        {
            Vector3 v = rb.linearVelocity;
            v.x = 0f;
            rb.linearVelocity = v;

            Vector3 p = transform.position;
            p.x = Mathf.Sign(p.x) * pitchHalfWidth;
            transform.position = p;
        }
    }

    // ------------------ SPIN ------------------
    void ApplySpin()
    {
        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        int dir = (int)spinDirection;
        float finalAngle = spinAngle * dir;

        Quaternion rotation = Quaternion.AngleAxis(finalAngle, Vector3.up);
        Vector3 newDir = rotation * velocity.normalized;

        rb.linearVelocity = newDir * speed;
    }

    // ------------------ COLLISION ------------------
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBounced && collision.gameObject.CompareTag("Ground"))
        {
            hasBounced = true;
            isSwingActive = false;

            Debug.Log("REAL BOUNCE: " + transform.position);

            if (ballType == BallType.Spin)
            {
                ApplySpin();
            }
        }
    }

    public float GetMinimumReachableDistance()
    {
        float gravity = Mathf.Abs(Physics.gravity.y);

        // Approximation: very steep angle (~80–85 degrees)
        float steepAngle = 80f * Mathf.Deg2Rad;

        float vx = speed * Mathf.Cos(steepAngle);
        float vy = speed * Mathf.Sin(steepAngle);

        // Time to hit ground (same height assumption)
        float time = (2 * vy) / gravity;

        float minDistance = vx * time;

        return minDistance;
    }

    public bool IsTargetReachable(Vector3 target)
    {
        Vector3 start = transform.position;

        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);

        float distanceXZ = toTargetXZ.magnitude;
        float height = toTarget.y;
        height = Mathf.Abs(height); // Consider only positive height for reachability

        float speedSquared = speed * speed;

        float underRoot = speedSquared * speedSquared -
            gravity * (gravity * distanceXZ * distanceXZ + 2 * height * speedSquared);

        return underRoot >= 0f;
    }
}