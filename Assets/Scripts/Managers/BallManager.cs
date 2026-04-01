using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    #region Singleton
    public static BallManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion

    public Ball ball;
    private float accuracy = 1f;
    public float Accuracy => accuracy;

    [SerializeField] private List<BallData> ballDatasList;
    [SerializeField] private List<GameObject> bowlersList;
    [SerializeField] private AccuracyMeter accuracyMeter;

    private int currentBallIndex = 0;

    private void Start()
    {
        currentBallIndex = 0;
        ball.Initialize(ballDatasList[currentBallIndex]);

        InputManager.Instance.OnBowlSideChangeAction += ChangeSide;
        InputManager.Instance.OnDirectionChangeAction += ChangeBalltypeDirection;
        InputManager.Instance.OnSpinAction += InputManager_OnSpinAction;
        InputManager.Instance.OnSwingAction += InputManager_OnSwingAction;
        InputManager.Instance.OnBowlAction += Bowl;

        accuracyMeter.ResetAndStart();
    }

    private void InputManager_OnSwingAction()
    {
        ChangeBallType(BallType.Swing);

        ball.ResetBall();
    }

    private void InputManager_OnSpinAction()
    {
        ChangeBallType(BallType.Spin);

        ball.ResetBall();
    }

    public void ChangeSide()
    {
        if(currentBallIndex == 0)
        {
            currentBallIndex = 1;
        }
        else
        {
            currentBallIndex = 0;
        }

        for(int i = 0; i < bowlersList.Count; i++)
        {
            if (i == currentBallIndex)
            {
                bowlersList[i].SetActive(true);
            }
            else
            {
                bowlersList[i].SetActive(false);
            }
        }

        ball.Initialize(ballDatasList[currentBallIndex]);
    }

    public void ChangeBallType(BallType ballType)
    {
        ball.ChangeBallType(ballType);
    }

    public void ChangeBalltypeDirection()
    {
        ball.ChangeBallTypeDirection();

        ball.ResetBall();
    }

    public void Bowl()
    {
        if (OnCooldown) return; // Prevent bowling while on cooldown)
        accuracy = accuracyMeter.GetCurrentAccuracy();
        ball.LaunchBall();
        StartCoroutine(ResetBallAfterDelay(2.5f)); // Reset ball after 2 seconds, adjust as needed
    }

    private bool OnCooldown = false;
    private IEnumerator ResetBallAfterDelay(float delay)
    {
        OnCooldown = true;
        yield return new WaitForSeconds(delay);
        ball.ResetBall();
        accuracyMeter.ResetAndStart();
        OnCooldown = false;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnBowlSideChangeAction -= ChangeSide;
        InputManager.Instance.OnDirectionChangeAction -= ChangeBalltypeDirection;
        InputManager.Instance.OnSpinAction -= InputManager_OnSpinAction;
        InputManager.Instance.OnSwingAction -= InputManager_OnSwingAction;
        InputManager.Instance.OnBowlAction -= Bowl;
    }
}


[System.Serializable]
public class BallData
{
    public string ballID;
    public Vector3 position;
    public Direction idealSwingDirection;
}

public enum BallPosition
{
    OTW = 0,
    RTW = 1,
}

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


