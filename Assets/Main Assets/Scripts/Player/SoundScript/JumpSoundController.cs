using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class JumpSoundAndHapticsController : MonoBehaviour
{
    [Header("Riferimenti ai Componenti")]
    public JumpController jumpController;
    public HexaBodyController hexaBodyController;

    [Header("AudioSource per i Suoni")]
    [Tooltip("AudioSource per il suono del salto")]
    public AudioSource jumpSound;
    [Tooltip("AudioSource per il suono dell'atterraggio")]
    public AudioSource landSound;

    [Header("Impostazioni Feedback Aptico")]
    [Tooltip("Nodo XR per il controller destro")]
    public XRNode rightHandNode = XRNode.RightHand;
    [Tooltip("Nodo XR per il controller sinistro")]
    public XRNode leftHandNode = XRNode.LeftHand;
    public float jumpHapticIntensity = 0.5f;           // Intensità vibrazione al salto
    public float maxLandHapticIntensity = 1.0f;        // Intensità massima vibrazione all'atterraggio
    public float minLandHapticIntensity = 0.2f;        // Intensità minima vibrazione all'atterraggio
    public float hapticDuration = 0.1f;                // Durata dell'impulso aptico

    private InputDevice rightHandDevice;
    private InputDevice leftHandDevice;
    private bool wasGrounded = true;

    void Start()
    {
        if (jumpController == null)
        {
            Debug.LogError("JumpController non assegnato.");
        }
        if (hexaBodyController == null)
        {
            Debug.LogError("HexaBodyController non assegnato.");
        }

        TryInitializeHapticDevices();
    }

    void TryInitializeHapticDevices()
    {
        rightHandDevice = InputDevices.GetDeviceAtXRNode(rightHandNode);
        leftHandDevice = InputDevices.GetDeviceAtXRNode(leftHandNode);
    }

    void Update()
    {
        if (!rightHandDevice.isValid || !leftHandDevice.isValid)
        {
            TryInitializeHapticDevices();
        }

        bool isGrounded = jumpController.IsGrounded;

        if (wasGrounded && !isGrounded)
        {
            PlayJumpSound();
            TriggerHapticFeedback(jumpHapticIntensity, hapticDuration);
        }

        if (!wasGrounded && isGrounded)
        {
            PlayLandSound();
            float impactIntensity = CalculateImpactIntensity();
            TriggerHapticFeedback(impactIntensity, hapticDuration);
        }

        wasGrounded = isGrounded;
    }

    private void PlayJumpSound()
    {
        if (jumpSound != null && !jumpSound.isPlaying)
        {
            jumpSound.Play();
        }
    }

    private void PlayLandSound()
    {
        if (landSound != null && !landSound.isPlaying)
        {
            landSound.Play();
        }
    }

    private void TriggerHapticFeedback(float intensity, float duration)
    {
        if (rightHandDevice.isValid)
        {
            HapticCapabilities capabilities;
            if (rightHandDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                rightHandDevice.SendHapticImpulse(0, intensity, duration);
            }
        }

        if (leftHandDevice.isValid)
        {
            HapticCapabilities capabilities;
            if (leftHandDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                leftHandDevice.SendHapticImpulse(0, intensity, duration);
            }
        }
    }

    private float CalculateImpactIntensity()
    {
        float verticalSpeed = Mathf.Abs(hexaBodyController.monoballRb.velocity.y);
        float normalizedIntensity = Mathf.Clamp01(verticalSpeed / 10.0f);

        // Assicura che l'intensità sia almeno pari a minLandHapticIntensity
        return Mathf.Max(normalizedIntensity * maxLandHapticIntensity, minLandHapticIntensity);
    }
}
