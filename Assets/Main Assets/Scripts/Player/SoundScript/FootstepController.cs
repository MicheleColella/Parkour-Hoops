using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Questo script simula i passi del giocatore alternando vibrazioni e suoni
/// tra i controller destro e sinistro in base alla velocità di movimento.
/// </summary>
public class FootstepController : MonoBehaviour
{
    [Header("Riferimenti")]
    public HexaBodyController hexaBodyController; // Riferimento a HexaBodyController per ottenere la velocità del giocatore
    public JumpController jumpController;         // Riferimento a JumpController per verificare se il giocatore è a terra

    [Header("Controller")]
    [Tooltip("Nodo XR per il controller destro")]
    public XRNode rightHandNode = XRNode.RightHand;

    [Tooltip("Nodo XR per il controller sinistro")]
    public XRNode leftHandNode = XRNode.LeftHand;

    private InputDevice rightHandDevice;
    private InputDevice leftHandDevice;

    [Header("Impostazioni del Feedback Aptico")]
    [Tooltip("Intensità massima del feedback aptico (da 0.0 a 1.0)")]
    public float maximumHapticIntensity = 1.0f; // Intensità massima della vibrazione

    [Tooltip("Durata dell'impulso aptico")]
    public float hapticDuration = 0.05f; // Durata della vibrazione

    [Header("Audio dei Passi")]
    [Tooltip("AudioSource per il suono del passo destro")]
    public AudioSource rightFootstepAudioSource;

    [Tooltip("AudioSource per il suono del passo sinistro")]
    public AudioSource leftFootstepAudioSource;

    [Header("Impostazioni del Timing dei Passi")]
    [Tooltip("Velocità minima per iniziare a generare passi")]
    public float minSpeed = 0.5f;

    [Tooltip("Velocità alla quale i passi raggiungono la massima frequenza")]
    public float maxSpeed = 5.0f;

    [Tooltip("Intervallo minimo tra i passi alla massima velocità")]
    public float minStepInterval = 0.2f;

    [Tooltip("Intervallo massimo tra i passi alla velocità minima")]
    public float maxStepInterval = 0.6f;

    private float timeSinceLastStep = 0f;   // Tempo trascorso dall'ultimo passo
    private bool isRightFootNext = false;   // Flag per alternare tra piede destro e sinistro

    void Start()
    {
        // Ottieni i riferimenti dagli script esistenti se non sono già assegnati
        if (hexaBodyController == null)
        {
            hexaBodyController = GetComponent<HexaBodyController>();
            if (hexaBodyController == null)
            {
                Debug.LogError("HexaBodyController non trovato sul GameObject.");
            }
        }

        if (jumpController == null)
        {
            jumpController = GetComponent<JumpController>();
            if (jumpController == null)
            {
                Debug.LogError("JumpController non trovato sul GameObject.");
            }
        }

        // Inizializza i dispositivi di input
        TryInitialize();
    }

    void TryInitialize()
    {
        rightHandDevice = InputDevices.GetDeviceAtXRNode(rightHandNode);
        leftHandDevice = InputDevices.GetDeviceAtXRNode(leftHandNode);
    }

    void Update()
    {
        // Se i dispositivi non sono validi, tenta di inizializzarli nuovamente
        if (!rightHandDevice.isValid || !leftHandDevice.isValid)
        {
            TryInitialize();
        }

        // Verifica se il giocatore è a terra
        if (!jumpController.IsGrounded)
        {
            // Reset del tempo dall'ultimo passo quando in aria
            timeSinceLastStep = 0f;
            return;
        }

        // Ottieni la velocità orizzontale del giocatore
        Vector3 horizontalVelocity = new Vector3(
            hexaBodyController.monoballRb.velocity.x,
            0,
            hexaBodyController.monoballRb.velocity.z
        );
        float speed = horizontalVelocity.magnitude;

        // Se la velocità è sotto la minima, resetta il timer e non generare passi
        if (speed < minSpeed)
        {
            timeSinceLastStep = 0f;
            return;
        }

        // Calcola l'intervallo tra i passi in base alla velocità
        float normalizedSpeed = Mathf.Clamp01((speed - minSpeed) / (maxSpeed - minSpeed));
        float stepInterval = Mathf.Lerp(maxStepInterval, minStepInterval, normalizedSpeed);

        // Incrementa il tempo dall'ultimo passo
        timeSinceLastStep += Time.deltaTime;

        // Se è il momento di generare un nuovo passo
        if (timeSinceLastStep >= stepInterval)
        {
            GenerateFootstep(speed);
            timeSinceLastStep = 0f;

            // Alterna il piede
            isRightFootNext = !isRightFootNext;
        }
    }

    /// <summary>
    /// Genera un passo vibrando il controller appropriato e riproducendo il suono del passo.
    /// </summary>
    /// <param name="speed">La velocità attuale del giocatore per calcolare l'intensità.</param>
    void GenerateFootstep(float speed)
    {
        // Calcola l'intensità aptica in base alla velocità del giocatore
        float intensity = Mathf.Clamp01(speed / maxSpeed) * maximumHapticIntensity;

        if (isRightFootNext)
        {
            // Vibrazione del controller destro
            if (rightHandDevice.isValid)
            {
                HapticCapabilities capabilities;
                if (rightHandDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    rightHandDevice.SendHapticImpulse(channel, intensity, hapticDuration);
                }
            }

            // Riproduce il suono del passo destro
            if (rightFootstepAudioSource != null)
            {
                rightFootstepAudioSource.Play();
            }
        }
        else
        {
            // Vibrazione del controller sinistro
            if (leftHandDevice.isValid)
            {
                HapticCapabilities capabilities;
                if (leftHandDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    leftHandDevice.SendHapticImpulse(channel, intensity, hapticDuration);
                }
            }

            // Riproduce il suono del passo sinistro
            if (leftFootstepAudioSource != null)
            {
                leftFootstepAudioSource.Play();
            }
        }
    }
}
