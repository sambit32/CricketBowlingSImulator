using UnityEngine;

public class BallManager : MonoBehaviour
{
    public class BallData
    {
        public BallPosition position;
        public Direction swingDirection;
        public float swingStrength;
        public float maxSwing;

        public float spinStrength;
        public Direction spinDirection;
    }
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


