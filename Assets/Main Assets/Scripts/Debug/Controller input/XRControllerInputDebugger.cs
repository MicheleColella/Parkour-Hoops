using UnityEngine;
using UnityEngine.InputSystem;

public class XRControllerInputDebugger : MonoBehaviour
{
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

    // ======================================================
    // Metodo OnEnable - Attivazione delle azioni e iscrizione agli eventi
    // ======================================================
    private void OnEnable()
    {
        // Sinistra
        leftTriggerAction.action.performed += OnInputPerformed;
        leftGripAction.action.performed += OnInputPerformed;
        leftPrimaryButtonAction.action.performed += OnInputPerformed;
        leftSecondaryButtonAction.action.performed += OnInputPerformed;
        leftThumbstickTranslate.action.performed += OnLeftThumbstickPerformed;
        leftThumbstickRotate.action.performed += OnLeftThumbstickPerformed;
        leftThumbstickClickAction.action.performed += OnInputPerformed;

        // Destra
        rightTriggerAction.action.performed += OnInputPerformed;
        rightGripAction.action.performed += OnInputPerformed;
        rightPrimaryButtonAction.action.performed += OnInputPerformed;
        rightSecondaryButtonAction.action.performed += OnInputPerformed;
        rightThumbstickTranslate.action.performed += OnRightThumbstickPerformed;
        rightThumbstickRotate.action.performed += OnRightThumbstickPerformed;
        rightThumbstickClickAction.action.performed += OnInputPerformed;

        EnableActions();
    }

    // ======================================================
    // Metodo OnDisable - Disattivazione delle azioni e rimozione degli eventi
    // ======================================================
    private void OnDisable()
    {
        // Sinistra
        leftTriggerAction.action.performed -= OnInputPerformed;
        leftGripAction.action.performed -= OnInputPerformed;
        leftPrimaryButtonAction.action.performed -= OnInputPerformed;
        leftSecondaryButtonAction.action.performed -= OnInputPerformed;
        leftThumbstickTranslate.action.performed -= OnLeftThumbstickPerformed;
        leftThumbstickRotate.action.performed -= OnLeftThumbstickPerformed;
        leftThumbstickClickAction.action.performed -= OnInputPerformed;

        // Destra
        rightTriggerAction.action.performed -= OnInputPerformed;
        rightGripAction.action.performed -= OnInputPerformed;
        rightPrimaryButtonAction.action.performed -= OnInputPerformed;
        rightSecondaryButtonAction.action.performed -= OnInputPerformed;
        rightThumbstickTranslate.action.performed -= OnRightThumbstickPerformed;
        rightThumbstickRotate.action.performed -= OnRightThumbstickPerformed;
        rightThumbstickClickAction.action.performed -= OnInputPerformed;

        DisableActions();
    }

    // ======================================================
    // Abilitazione di tutte le azioni del controller
    // ======================================================
    private void EnableActions()
    {
        // Sinistra
        leftTriggerAction.action.Enable();
        leftGripAction.action.Enable();
        leftPrimaryButtonAction.action.Enable();
        leftSecondaryButtonAction.action.Enable();
        leftThumbstickTranslate.action.Enable();
        leftThumbstickRotate.action.Enable();
        leftThumbstickClickAction.action.Enable();

        // Destra
        rightTriggerAction.action.Enable();
        rightGripAction.action.Enable();
        rightPrimaryButtonAction.action.Enable();
        rightSecondaryButtonAction.action.Enable();
        rightThumbstickTranslate.action.Enable();
        rightThumbstickRotate.action.Enable();
        rightThumbstickClickAction.action.Enable();
    }

    // ======================================================
    // Disabilitazione di tutte le azioni del controller
    // ======================================================
    private void DisableActions()
    {
        // Sinistra
        leftTriggerAction.action.Disable();
        leftGripAction.action.Disable();
        leftPrimaryButtonAction.action.Disable();
        leftSecondaryButtonAction.action.Disable();
        leftThumbstickTranslate.action.Disable();
        leftThumbstickRotate.action.Disable();
        leftThumbstickClickAction.action.Disable();

        // Destra
        rightTriggerAction.action.Disable();
        rightGripAction.action.Disable();
        rightPrimaryButtonAction.action.Disable();
        rightSecondaryButtonAction.action.Disable();
        rightThumbstickTranslate.action.Disable();
        rightThumbstickRotate.action.Disable();
        rightThumbstickClickAction.action.Disable();
    }

    // ======================================================
    // Metodo di Callback per l'input generale
    // ======================================================
    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        // Debug dell'azione eseguita e del suo valore
        Debug.Log($"Input {context.action.name} performed with value: {context.ReadValueAsObject()}");
    }

    // ======================================================
    // Metodo di Callback per il Thumbstick Sinistro
    // ======================================================
    private void OnLeftThumbstickPerformed(InputAction.CallbackContext context)
    {
        // Controlla se è una rotazione o traslazione
        if (context.action == leftThumbstickTranslate.action)
        {
            leftThumbstickValue.y = context.ReadValue<Vector2>().y;
        }
        else if (context.action == leftThumbstickRotate.action)
        {
            leftThumbstickValue.x = context.ReadValue<Vector2>().x;
        }

        // Stampa i valori combinati in un singolo messaggio
        Debug.Log($"Left Thumbstick X: {leftThumbstickValue.x}, Y: {leftThumbstickValue.y}");
    }

    // ======================================================
    // Metodo di Callback per il Thumbstick Destro
    // ======================================================
    private void OnRightThumbstickPerformed(InputAction.CallbackContext context)
    {
        // Controlla se è una rotazione o traslazione
        if (context.action == rightThumbstickTranslate.action)
        {
            rightThumbstickValue.y = context.ReadValue<Vector2>().y;
        }
        else if (context.action == rightThumbstickRotate.action)
        {
            rightThumbstickValue.x = context.ReadValue<Vector2>().x;
        }

        // Stampa i valori combinati in un singolo messaggio
        Debug.Log($"Right Thumbstick X: {rightThumbstickValue.x}, Y: {rightThumbstickValue.y}");
    }
}
