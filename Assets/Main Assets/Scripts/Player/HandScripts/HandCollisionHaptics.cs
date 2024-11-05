using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandCollisionHaptics : MonoBehaviour
{
    private Rigidbody handRigidbody;

    public XRNode handNode = XRNode.LeftHand;
    private InputDevice targetDevice;

    [Header("Parametri Aptici")]
    public float minimumCollisionSpeed = 0.1f;
    public float maximumCollisionSpeed = 5.0f;
    public float maximumHapticIntensity = 1.0f;
    public float hapticDuration = 0.1f;

    [Header("Parametri Suono di Collisione")]
    [Tooltip("Lista di AudioClip per la collisione, dal suono più leggero al più intenso")]
    public List<AudioClip> collisionSounds;  // Lista di suoni di collisione, ordinati dal più leggero al più forte
    public AudioSource collisionSoundSource;
    public float collisionSoundThreshold = 0.5f;    // Tempo minimo tra i suoni in secondi

    [Header("Range del Pitch Casuale")]
    [Tooltip("Valore minimo per il pitch casuale")]
    public float minPitchValue = 0.8f;
    [Tooltip("Valore massimo per il pitch casuale")]
    public float maxPitchValue = 1.2f;

    private float lastCollisionSoundTime;

    void Start()
    {
        handRigidbody = GetComponent<Rigidbody>();
        TryInitialize();
    }

    void TryInitialize()
    {
        targetDevice = InputDevices.GetDeviceAtXRNode(handNode);
    }

    void Update()
    {
        if (!targetDevice.isValid)
        {
            TryInitialize();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == gameObject)
            return;

        float collisionSpeed = collision.relativeVelocity.magnitude;
        if (collisionSpeed < minimumCollisionSpeed)
            return;

        // Calcola l'intensità per la vibrazione aptica
        float intensity = Mathf.Clamp01(collisionSpeed / maximumCollisionSpeed) * maximumHapticIntensity;

        if (targetDevice.isValid)
        {
            HapticCapabilities capabilities;
            if (targetDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                targetDevice.SendHapticImpulse(0, intensity, hapticDuration);
            }
        }

        // Selezione e riproduzione del suono di collisione con il controllo del tempo di ripetizione
        if (collisionSounds.Count >= 3 && collisionSoundSource != null && Time.time - lastCollisionSoundTime >= collisionSoundThreshold)
        {
            // Determina l'indice del suono in base all'intensità
            int soundIndex;
            if (intensity <= 0.33f)
                soundIndex = 0;  // Suono più leggero
            else if (intensity <= 0.66f)
                soundIndex = 1;  // Suono intermedio
            else
                soundIndex = 2;  // Suono più intenso

            // Imposta il clip selezionato nell'AudioSource
            collisionSoundSource.clip = collisionSounds[soundIndex];

            // Imposta il volume proporzionalmente all'intensità
            float collisionVolume = intensity;
            collisionSoundSource.volume = collisionVolume;

            // Applica un valore di pitch casuale entro il range specificato
            collisionSoundSource.pitch = Random.Range(minPitchValue, maxPitchValue);

            // Riproduce il suono di collisione
            collisionSoundSource.Play();
            lastCollisionSoundTime = Time.time;
        }
    }
}
