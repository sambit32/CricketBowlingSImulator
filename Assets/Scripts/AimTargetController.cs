using System.Collections.Generic;
using UnityEngine;

public class AimTargetController : MonoBehaviour
{
    public float moveSpeed = 10f;

    [Header("References")]
    public Ball ball; // drag Ball here in inspector

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        Vector3 move = new Vector3(h, 0f, v);

        Vector3 newPosition = transform.position + move * moveSpeed * Time.deltaTime;

        //  Only allow movement if reachable
        if (ball != null && ball.IsTargetReachable(newPosition))
        {
            transform.position = newPosition;
        }
    }
}