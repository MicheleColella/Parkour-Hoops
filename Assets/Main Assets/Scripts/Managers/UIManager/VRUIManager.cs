using UnityEngine;
using UnityEngine.InputSystem;

public class VRUIManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Il target della UI nel mondo.")]
    public Transform uiTarget;
    [Tooltip("Il GameObject della UI (es. Canvas).")]
    public GameObject uiCanvas;

    [Header("Input Settings")]
    [Tooltip("Input Action per attivare/disattivare la UI.")]
    public InputActionProperty toggleUIInput;

    [Header("Movement Settings")]
    [Tooltip("Velocità con cui la UI segue il target.")]
    public float followSpeed = 10f;
    [Tooltip("Velocità di rotazione della UI verso il player.")]
    public float rotationSpeed = 10f;

    private bool isUIActive = false;
    private Transform playerTransform;

    private void Start()
    {
        // Trova il player tramite il tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player non trovato! Assicurati che il player abbia il tag 'Player'.");
        }

        // Disattiva inizialmente la UI
        if (uiCanvas != null)
        {
            uiCanvas.SetActive(false);
        }

        // Associa l'azione di input
        if (toggleUIInput != null)
        {
            toggleUIInput.action.performed += ToggleUI;
        }
    }

    private void OnDestroy()
    {
        // Rimuovi il listener per l'input quando lo script viene distrutto
        if (toggleUIInput != null)
        {
            toggleUIInput.action.performed -= ToggleUI;
        }
    }

    private void Update()
    {
        if (isUIActive && uiCanvas != null && uiTarget != null && playerTransform != null)
        {
            // Posiziona la UI verso il target in modo fluido
            Vector3 targetPosition = uiTarget.position;
            uiCanvas.transform.position = Vector3.Lerp(uiCanvas.transform.position, targetPosition, followSpeed * Time.deltaTime);

            // Calcola la direzione verso il player
            Vector3 directionToPlayer = (playerTransform.position - uiCanvas.transform.position).normalized;

            // Ruota il canvas verso il player con un'inversione di 180 gradi
            Quaternion targetRotation = Quaternion.LookRotation(-directionToPlayer, Vector3.up);
            uiCanvas.transform.rotation = Quaternion.Slerp(uiCanvas.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void ToggleUI(InputAction.CallbackContext context)
    {
        if (uiCanvas != null)
        {
            isUIActive = !isUIActive;

            if (isUIActive)
            {
                // Attiva la UI e posizionala immediatamente sul target
                uiCanvas.SetActive(true);
                uiCanvas.transform.position = uiTarget.position;

                if (playerTransform != null)
                {
                    Vector3 directionToPlayer = (playerTransform.position - uiCanvas.transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer, Vector3.up); // Inversione di 180 gradi
                    uiCanvas.transform.rotation = lookRotation;
                }
            }
            else
            {
                // Disattiva la UI
                uiCanvas.SetActive(false);
            }
        }
    }
}
