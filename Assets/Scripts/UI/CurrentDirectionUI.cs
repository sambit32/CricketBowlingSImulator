using TMPro;
using UnityEngine;

public class CurrentDirectionUI : MonoBehaviour
{
    [SerializeField] Ball ball;

    private bool left;

    [SerializeField] private TextMeshProUGUI currentDirectionText;

    private void Awake()
    {
        ball.OnCurrentDirectionChange += Ball_OnCurrentDirectionChange;
    }

    private void Ball_OnCurrentDirectionChange()
    {
        left = ball.GetCurrentDirection() == Direction.Left;

        if (left)
        {
            currentDirectionText.text = "<=";

        }
        else
        {
            currentDirectionText.text = "=>";
        }
    }

    private void OnDisable()
    {
        ball.OnCurrentDirectionChange -= Ball_OnCurrentDirectionChange;
    }
}
