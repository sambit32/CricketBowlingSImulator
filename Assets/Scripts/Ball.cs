using System;
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

    [Header("Spin Settings")]
    [Range(0f, 45f)]
    public float spinAngle = 10f;
    public Direction spinDirection = Direction.Right;

    [Header("Ground")]
    public float groundY = 0f;

    [Header("Post Bounce")]
    public float postBounceSpeed = 10f;
    public float friction = 5f;

    [Header("Visuals")]
    [SerializeField] private TrailRenderer trailRenderer;

 
    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 initialVelocity;

    private float flightTime;
    private float elapsedTime;

    private bool isLaunched = false;
    private bool hasBounced = false;
    private bool isPostBounce = false;

    private Vector3 lastFramePos;
    private Vector3 postBounceVelocity;


    public event Action OnCurrentDirectionChange;
    public event Action<BallType> OnBallTypeChange;


    public void Initialize(BallData ballData)
    {
        startPos = ballData.position;
        transform.position = startPos;

        targetPos = aimTarget.position;
        swingDirection = (Direction)ballData.idealSwingDirection;

        ResetBall();

        OnCurrentDirectionChange?.Invoke();
        OnBallTypeChange?.Invoke(ballType);
    }

    public void ResetBall()
    {
        isLaunched = false;
        hasBounced = false;
        isPostBounce = false;

        elapsedTime = 0f;

        transform.position = startPos;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
    }

    public void LaunchBall()
    {
        ResetBall();

        startPos = transform.position;
        targetPos = aimTarget.position;

        initialVelocity = CalculateLaunchVelocity();
        flightTime = CalculateFlightTime(initialVelocity.y);

        elapsedTime = 0f;
        isLaunched = true;

        if (trailRenderer != null)
            trailRenderer.enabled = true;
    }

    private void FixedUpdate()
    {
        bounceMarker.position = aimTarget.position;

        if (isLaunched)
        {
            HandlePreBounceMotion();
        }
        else if (isPostBounce)
        {
            HandlePostBounceMotion();
        }
    }

    private void HandlePreBounceMotion()
    {
        lastFramePos = transform.position;

        elapsedTime += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(elapsedTime / flightTime);

        Vector3 basePos = GetBallisticPosition(t);
        Vector3 finalPos = basePos + GetSwingOffset(t);

        transform.position = finalPos;

        if (!hasBounced && t >= 1f)
        {
            OnBounce();
        }
    }

    private Vector3 CalculateLaunchVelocity()
    {
        Vector3 start = startPos;
        Vector3 target = targetPos;

        float gravity = Mathf.Abs(Physics.gravity.y);

        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);

        float distanceXZ = toTargetXZ.magnitude;
        float height = toTarget.y;

        float speedSq = speed * speed;

        float underRoot = speedSq * speedSq -
                          gravity * (gravity * distanceXZ * distanceXZ + 2 * height * speedSq);

        if (underRoot < 0)
        {
            Debug.LogWarning("Target unreachable");
            return toTarget.normalized * speed;
        }

        float root = Mathf.Sqrt(underRoot);
        float angle = Mathf.Atan((speedSq - root) / (gravity * distanceXZ));

        Vector3 dir = toTargetXZ.normalized;

        return dir * speed * Mathf.Cos(angle) +
               Vector3.up * speed * Mathf.Sin(angle);
    }

    private float CalculateFlightTime(float verticalVelocity)
    {
        float g = Mathf.Abs(Physics.gravity.y);

        float timeUp = verticalVelocity / g;
        float maxHeight = (verticalVelocity * verticalVelocity) / (2 * g);

        float totalHeight = maxHeight - (targetPos.y - startPos.y);
        float timeDown = Mathf.Sqrt(2 * totalHeight / g);

        return timeUp + timeDown;
    }

    private Vector3 GetBallisticPosition(float t)
    {
        float g = Physics.gravity.y;
        float time = t * flightTime;

        Vector3 horizontal = Vector3.Lerp(startPos, targetPos, t);

        float y = startPos.y +
                  initialVelocity.y * time +
                  0.5f * g * time * time;

        return new Vector3(horizontal.x, y, horizontal.z);
    }

    private Vector3 GetSwingOffset(float t)
    {
        if (ballType != BallType.Swing)
            return Vector3.zero;

        int dir = (int)swingDirection;

        float strength = swingStrength * BallManager.Instance.Accuracy;

        float curve = Mathf.Sin(t * Mathf.PI);

        Vector3 forward = (targetPos - startPos).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, forward).normalized * dir;

        return side * strength * curve;
    }

    private void OnBounce()
    {
        hasBounced = true;
        isLaunched = false;
        isPostBounce = true;

        // Snap to ground
        transform.position = new Vector3(targetPos.x, groundY, targetPos.z);

        // Calculate velocity from last frame
        Vector3 velocity = (transform.position - lastFramePos) / Time.fixedDeltaTime;
        velocity.y += 3f;

        // --- BOUNCE SETTINGS ---
        float bounceFactor = 1f; // 0 = no bounce, 1 = perfect elastic

        // Reflect velocity on ground normal (Y axis)
        Vector3 bouncedVelocity = new Vector3(
            velocity.x,
            Mathf.Abs(velocity.y) * bounceFactor, // invert & reduce Y
            velocity.z
        );

        // Normalize horizontal direction
        Vector3 horizontal = new Vector3(bouncedVelocity.x, 0f, bouncedVelocity.z).normalized;

        // Combine horizontal + vertical
        postBounceVelocity =
            horizontal * postBounceSpeed +
            Vector3.up * bouncedVelocity.y;

        // Apply spin
        if (ballType == BallType.Spin)
        {
            int dir = (int)spinDirection;
            float angle = spinAngle * BallManager.Instance.Accuracy * dir;

            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            postBounceVelocity = rot * postBounceVelocity;
        }
    }

    private void HandlePostBounceMotion()
    {
        // Apply gravity
        postBounceVelocity += Physics.gravity * Time.fixedDeltaTime;

        transform.position += postBounceVelocity * Time.fixedDeltaTime;

        // Ground collision again (optional multi-bounce)
        if (transform.position.y <= groundY)
        {
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

            // Lose energy on each bounce
            float bounceFactor = 1f;
            postBounceVelocity.y = Mathf.Abs(postBounceVelocity.y) * bounceFactor;

            // Apply friction on horizontal
            Vector3 horizontal = new Vector3(postBounceVelocity.x, 0, postBounceVelocity.z);
            //horizontal *= .95f;

            postBounceVelocity = horizontal + Vector3.up * postBounceVelocity.y;
        }

        // Stop condition
        if (postBounceVelocity.magnitude < 0.1f)
        {
            isPostBounce = false;
        }
    }

    public void ChangeBallType(BallType type)
    {
        ballType = type;
        OnBallTypeChange?.Invoke(type);
    }

    public void ChangeBallTypeDirection()
    {
        if (ballType == BallType.Swing)
        {
            swingDirection = swingDirection == Direction.Left ? Direction.Right : Direction.Left;
        }
        else
        {
            spinDirection = spinDirection == Direction.Left ? Direction.Right : Direction.Left;
        }

        OnCurrentDirectionChange?.Invoke();
    }

    public Direction GetCurrentDirection()
    {
        return ballType == BallType.Swing ? swingDirection : spinDirection;
    }

    public bool CanLaunch()
    {
        return true;
    }
}