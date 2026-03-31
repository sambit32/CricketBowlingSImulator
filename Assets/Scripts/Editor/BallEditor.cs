using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ball))]
public class BallEditor : Editor
{
    private Ball ball;

    private void OnEnable()
    {
        ball = (Ball)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Launch Ball"))
        {
            ball.LaunchBall();
        }

        if (GUILayout.Button("Reset"))
        {
            ball.ResetBall();
        }
    }
}
