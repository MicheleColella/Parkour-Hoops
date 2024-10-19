using UnityEngine;
using UnityEngine.InputSystem;

public class XRControllerInputManager : MonoBehaviour
{
    public static XRControllerInputManager Instance { get; private set; }

    // ======================================================
    // Controllo per la visualizzazione dei log di debug
    // ======================================================
    [Header("Debug Settings")]
    public bool enableDebugLogs = false;  // Imposta questo a true per abilitare i log

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

    // Variabili per tracciare i valori X e Y degli stick sinistro e destro
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
    // Metodo OnEnable - Attivazione delle azioni e iscrizione agli eventi
    // ======================================================
    private void OnEnable()
    {
        // Sinistra
        leftThumbstickTranslate.action.performed += OnLeftThumbstickTranslatePerformed;
        leftThumbstickTranslate.action.canceled += OnLeftThumbstickTranslateCanceled;  // Azzerare quando viene rilasciato
        leftThumbstickRotate.action.performed += OnLeftThumbstickRotatePerformed;
        leftThumbstickRotate.action.canceled += OnLeftThumbstickRotateCanceled;  // Azzerare quando viene rilasciato

        // Destra
        rightThumbstickTranslate.action.performed += OnRightThumbstickTranslatePerformed;
        rightThumbstickTranslate.action.canceled += OnRightThumbstickTranslateCanceled;  // Azzerare quando viene rilasciato
        rightThumbstickRotate.action.performed += OnRightThumbstickRotatePerformed;
        rightThumbstickRotate.action.canceled += OnRightThumbstickRotateCanceled;  // Azzerare quando viene rilasciato

        EnableActions();
    }

    // ======================================================
    // Metodo OnDisable - Disattivazione delle azioni e rimozione degli eventi
    // ======================================================
    private void OnDisable()
    {
        // Sinistra
        leftThumbstickTranslate.action.performed -= OnLeftThumbstickTranslatePerformed;
        leftThumbstickTranslate.action.canceled -= OnLeftThumbstickTranslateCanceled;
        leftThumbstickRotate.action.performed -= OnLeftThumbstickRotatePerformed;
        leftThumbstickRotate.action.canceled -= OnLeftThumbstickRotateCanceled;

        // Destra
        rightThumbstickTranslate.action.performed -= OnRightThumbstickTranslatePerformed;
        rightThumbstickTranslate.action.canceled -= OnRightThumbstickTranslateCanceled;
        rightThumbstickRotate.action.performed -= OnRightThumbstickRotatePerformed;
        rightThumbstickRotate.action.canceled -= OnRightThumbstickRotateCanceled;

        DisableActions();
    }

    // ======================================================
    // Gestione del Thumbstick Sinistro (Traslazione)
    // ======================================================
    private void OnLeftThumbstickTranslatePerformed(InputAction.CallbackContext context)
    {
        leftThumbstickValue.y = context.ReadValue<Vector2>().y;
        if (enableDebugLogs)
            Debug.Log($"Left Thumbstick Translate - Y: {leftThumbstickValue.y}");
    }

    private void OnLeftThumbstickTranslateCanceled(InputAction.CallbackContext context)
    {
        leftThumbstickValue.y = 0f;  // Azzerare il valore Y quando viene rilasciato
        if (enableDebugLogs)
            Debug.Log($"Left Thumbstick Translate Released - Y reset to {leftThumbstickValue.y}");
    }

    // ======================================================
    // Gestione del Thumbstick Sinistro (Rotazione)
    // ======================================================
    private void OnLeftThumbstickRotatePerformed(InputAction.CallbackContext context)
    {
        leftThumbstickValue.x = context.ReadValue<Vector2>().x;
        if (enableDebugLogs)
            Debug.Log($"Left Thumbstick Rotate - X: {leftThumbstickValue.x}");
    }

    private void OnLeftThumbstickRotateCanceled(InputAction.CallbackContext context)
    {
        leftThumbstickValue.x = 0f;  // Azzerare il valore X quando viene rilasciato
        if (enableDebugLogs)
            Debug.Log($"Left Thumbstick Rotate Released - X reset to {leftThumbstickValue.x}");
    }

    // ======================================================
    // Gestione del Thumbstick Destro (Traslazione)
    // ======================================================
    private void OnRightThumbstickTranslatePerformed(InputAction.CallbackContext context)
    {
        rightThumbstickValue.y = context.ReadValue<Vector2>().y;
        if (enableDebugLogs)
            Debug.Log($"Right Thumbstick Translate - Y: {rightThumbstickValue.y}");
    }

    private void OnRightThumbstickTranslateCanceled(InputAction.CallbackContext context)
    {
        rightThumbstickValue.y = 0f;  // Azzerare il valore Y quando viene rilasciato
        if (enableDebugLogs)
            Debug.Log($"Right Thumbstick Translate Released - Y reset to {rightThumbstickValue.y}");
    }

    // ======================================================
    // Gestione del Thumbstick Destro (Rotazione)
    // ======================================================
    private void OnRightThumbstickRotatePerformed(InputAction.CallbackContext context)
    {
        rightThumbstickValue.x = context.ReadValue<Vector2>().x;
        if (enableDebugLogs)
            Debug.Log($"Right Thumbstick Rotate - X: {rightThumbstickValue.x}");
    }

    private void OnRightThumbstickRotateCanceled(InputAction.CallbackContext context)
    {
        rightThumbstickValue.x = 0f;  // Azzerare il valore X quando viene rilasciato
        if (enableDebugLogs)
            Debug.Log($"Right Thumbstick Rotate Released - X reset to {rightThumbstickValue.x}");
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
    public Vector2 GetLeftThumbstickValue()
    {
        return leftThumbstickValue;
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
    public Vector2 GetRightThumbstickValue()
    {
        return rightThumbstickValue;
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
