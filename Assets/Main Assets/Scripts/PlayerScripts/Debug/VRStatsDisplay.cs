using UnityEngine;
using TMPro; // Per gestire TextMeshPro
using System.Collections.Generic; // Per usare Queue

public class VRStatsDisplay : MonoBehaviour
{
    [Header("References")]
    public VRLocomotionController locomotionController; // Riferimento allo script principale
    public TextMeshPro statsTextMeshPro; // Riferimento al componente TextMeshPro per le statistiche
    public TextMeshPro consoleTextMeshPro; // Riferimento al componente TextMeshPro per i log della console

    private Queue<string> consoleLogQueue = new Queue<string>(); // Coda per memorizzare i log
    public int maxLogMessages = 5; // Numero massimo di messaggi da mostrare

    void OnEnable()
    {
        // Registra la funzione per catturare i log della console
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // Rimuovi la funzione di log quando lo script è disabilitato
        Application.logMessageReceived -= HandleLog;
    }

    void Update()
    {
        // Mostra le statistiche della locomotion
        if (locomotionController != null && statsTextMeshPro != null)
        {
            string statsText = GetLocomotionStats();
            statsTextMeshPro.text = statsText;
        }

        // Mostra i log della console
        if (consoleTextMeshPro != null)
        {
            consoleTextMeshPro.text = GetConsoleLogs();
        }
    }

    string GetLocomotionStats()
    {
        // Ottieni i valori dallo script di locomotion
        bool isGrounded = locomotionController.IsGrounded();
        float currentSpeed = locomotionController.GetCurrentSpeed();

        // Restituisci il testo con le statistiche aggiornate
        return $"Grounded: {isGrounded}\n" +
               $"Speed: {currentSpeed:F2}\n" +
               $"Jumping: {locomotionController.IsJumping()}\n" +
               $"Jump Charge: {locomotionController.GetJumpChargeTime():F2}";
    }

    // Gestione dei log della console
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Aggiungi il nuovo log alla coda
        consoleLogQueue.Enqueue(logString);

        // Rimuovi i log più vecchi se si supera il numero massimo
        if (consoleLogQueue.Count > maxLogMessages)
        {
            consoleLogQueue.Dequeue();
        }
    }

    string GetConsoleLogs()
    {
        // Combina tutti i log presenti nella coda in un'unica stringa
        return string.Join("\n", consoleLogQueue.ToArray());
    }
}
