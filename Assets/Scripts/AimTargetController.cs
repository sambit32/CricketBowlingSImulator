using UnityEngine;

public class AimTargetController : MonoBehaviour
{
    public float moveSpeed = 10f;

    [Header("References")]
    public Ball ball; // drag Ball here in inspector

    void Update()
    {
        Vector2 input = InputManager.Instance.GetMoveInput();
        float h = input.x; // A/D
        float v = input.y;   // W/S

        Vector3 move = new Vector3(h, 0f, v);

        Vector3 newPosition = transform.position + move * moveSpeed * Time.deltaTime;

        //  Only allow movement if reachable
        if (ball != null && ball.IsTargetReachable(newPosition))
        {
            transform.position = newPosition;
        }
    }
}