using NUnit.Framework;
using UnityEngine;

public class CurrentBallTypeUI : MonoBehaviour
{
    [SerializeField] private GameObject swingIndicator;
    [SerializeField] private GameObject spinIndicator;

    [SerializeField] private Ball ball;

    private void Awake()
    {
        ball.OnBallTypeChange += Ball_OnBallTypeChange;
    }

    private void Ball_OnBallTypeChange(BallType ballType)
    {
        if(ballType == BallType.Swing)
        {
            swingIndicator.SetActive(true);
            spinIndicator.SetActive(false);
        }
        else
        {
            swingIndicator.SetActive(false);
            spinIndicator.SetActive(true);
        }
    }

    private void OnDisable()
    {
        ball.OnBallTypeChange -= Ball_OnBallTypeChange;
    }
}
