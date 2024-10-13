using UnityEngine;
using UnityEngine.InputSystem;

public class XRControllerInputManager : MonoBehaviour
{
    public static XRControllerInputManager Instance { get; private set; }

    // ======================================================
    // Input Actions per il Controller Sinistro
    // ======================================================
    [Header("Controller Sinistro")]
    public InputActionReference leftTriggerAction;
    public InputActionReference leftGripAction;
    public InputActionReference leftPrimaryButtonAction;
    public InputActionReference leftSecondaryButtonAction;
    public InputActionReference leftThumbstickTranslate;
    public InputActionReference leftThumbstickRotate;
    public InputActionReference leftThumbstickClickAction;

    // ======================================================
    // Input Actions per il Controller Destro
    // ======================================================
    [Header("Controller Destro")]
    public InputActionReference rightTriggerAction;
    public InputActionReference rightGripAction;
    public InputActionReference rightPrimaryButtonAction;
    public InputActionReference rightSecondaryButtonAction;
    public InputActionReference rightThumbstickTranslate;
    public InputActionReference rightThumbstickRotate;
    public InputActionReference rightThumbstickClickAction;

    private Vector2 leftThumbstickValue = Vector2.zero;
    private Vector2 rightThumbstickValue = Vector2.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // ======================================================
    // Metodo OnEnable - Attivazione delle azioni
    // ======================================================
    private void OnEnable()
    {
        EnableActions();
    }

    // ======================================================
    // Metodo OnDisable - Disattivazione delle azioni
    // ======================================================
    private void OnDisable()
    {
        DisableActions();
    }

    // ======================================================
    // Abilitazione delle azioni del controller
    // ======================================================
    private void EnableActions()
    {
        leftTriggerAction.action.Enable();
        leftGripAction.action.Enable();
        leftPrimaryButtonAction.action.Enable();
        leftSecondaryButtonAction.action.Enable();
        leftThumbstickTranslate.action.Enable();
        leftThumbstickRotate.action.Enable();
        leftThumbstickClickAction.action.Enable();

        rightTriggerAction.action.Enable();
        rightGripAction.action.Enable();
        rightPrimaryButtonAction.action.Enable();
        rightSecondaryButtonAction.action.Enable();
        rightThumbstickTranslate.action.Enable();
        rightThumbstickRotate.action.Enable();
        rightThumbstickClickAction.action.Enable();
    }

    // ======================================================
    // Disabilitazione delle azioni del controller
    // ======================================================
    private void DisableActions()
    {
        leftTriggerAction.action.Disable();
        leftGripAction.action.Disable();
        leftPrimaryButtonAction.action.Disable();
        leftSecondaryButtonAction.action.Disable();
        leftThumbstickTranslate.action.Disable();
        leftThumbstickRotate.action.Disable();
        leftThumbstickClickAction.action.Disable();

        rightTriggerAction.action.Disable();
        rightGripAction.action.Disable();
        rightPrimaryButtonAction.action.Disable();
        rightSecondaryButtonAction.action.Disable();
        rightThumbstickTranslate.action.Disable();
        rightThumbstickRotate.action.Disable();
        rightThumbstickClickAction.action.Disable();
    }

    // ======================================================
    // Metodi pubblici per ottenere gli input sinistro
    // ======================================================
    public float GetLeftTriggerValue()
    {
        return leftTriggerAction.action.ReadValue<float>();
    }

    public float GetLeftGripValue()
    {
        return leftGripAction.action.ReadValue<float>();
    }

    public Vector2 GetLeftThumbstickValue()
    {
        return leftThumbstickTranslate.action.ReadValue<Vector2>();
    }

    public bool GetLeftPrimaryButton()
    {
        return leftPrimaryButtonAction.action.ReadValue<float>() > 0.5f;
    }

    public bool GetLeftSecondaryButton()
    {
        return leftSecondaryButtonAction.action.ReadValue<float>() > 0.5f;
    }

    public bool GetLeftThumbstickClick()
    {
        return leftThumbstickClickAction.action.ReadValue<float>() > 0.5f;
    }


    // ======================================================
    // Metodi pubblici per ottenere gli input destro
    // ======================================================

    public float GetRightTriggerValue()
    {
        return rightTriggerAction.action.ReadValue<float>();
    }

    public float GetRightGripValue()
    {
        return rightGripAction.action.ReadValue<float>();
    }

    public Vector2 GetRightThumbstickValue()
    {
        return rightThumbstickTranslate.action.ReadValue<Vector2>();
    }

    public bool GetRightPrimaryButton()
    {
        return rightPrimaryButtonAction.action.ReadValue<float>() > 0.5f;
    }

    public bool GetRightSecondaryButton()
    {
        return rightSecondaryButtonAction.action.ReadValue<float>() > 0.5f;
    }

    public bool GetRightThumbstickClick()
    {
        return rightThumbstickClickAction.action.ReadValue<float>() > 0.5f;
    }
}
