using UnityEngine;

public class InputManager : MonoBehaviour
{
    #region Singleton
    public static InputManager Instance;

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

    #region Events
    public event System.Action OnBowlSideChangeAction;
    public event System.Action OnDirectionChangeAction;
    public event System.Action OnSpinAction;
    public event System.Action OnSwingAction;
    public event System.Action OnBowlAction;
    #endregion

    private InputAsset inputAsset;

    private void OnEnable()
    {
        inputAsset = new InputAsset();

        inputAsset.Enable();

        inputAsset.Player.BowlSide.performed += BowlSide_performed;
        inputAsset.Player.ChangeDirection.performed += ChangeDirection_performed;
        inputAsset.Player.Spin.performed += Spin_performed;
        inputAsset.Player.Swing.performed += Swing_performed;
        inputAsset.Player.Bowl.performed += Bowl_performed;
    }

    private void Bowl_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnBowlAction?.Invoke();
    }

    private void Swing_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnSwingAction?.Invoke();
    }

    private void Spin_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnSpinAction?.Invoke();
    }

    private void ChangeDirection_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnDirectionChangeAction?.Invoke();
    }

    private void BowlSide_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnBowlSideChangeAction?.Invoke();
    }

    public Vector2 GetMoveInput()
    {
        return inputAsset.Player.Move.ReadValue<Vector2>().normalized;
    }

    private void OnDisable()
    {
        inputAsset.Player.BowlSide.performed -= BowlSide_performed;
        inputAsset.Player.ChangeDirection.performed -= ChangeDirection_performed;
        inputAsset.Player.Spin.performed -= Spin_performed;
        inputAsset.Player.Swing.performed -= Swing_performed;
        inputAsset.Player.Bowl.performed -= Bowl_performed;

        inputAsset.Disable();
    }
}
