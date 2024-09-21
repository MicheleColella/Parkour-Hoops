using UnityEngine;
using TMPro; // Per gestire TextMeshPro

public class VRStatsDisplay : MonoBehaviour
{
    [Header("References")]
    public VRLocomotionController locomotionController; // Riferimento allo script principale
    public TextMeshPro textMeshPro; // Riferimento al componente TextMeshPro

    void Update()
    {
        if (locomotionController != null && textMeshPro != null)
        {
            // Prende i dati dallo script di locomotion e li mostra tramite TextMeshPro
            string statsText = GetLocomotionStats();
            textMeshPro.text = statsText;
        }
    }

    string GetLocomotionStats()
    {
        // Ottieni i valori che ti interessano
        bool isGrounded = locomotionController.IsGrounded();
        float currentSpeed = locomotionController.GetCurrentSpeed();

        // Mostra i dati, puoi aggiungere altri valori che ritieni importanti
        return $"Grounded: {isGrounded}\n" +
               $"Speed: {currentSpeed:F2}\n" +
               $"Jumping: {locomotionController.IsJumping()}\n" +
               $"Jump Charge: {locomotionController.GetJumpChargeTime():F2}";
    }
}
