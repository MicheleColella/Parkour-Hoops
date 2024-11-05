using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandCollisionHaptics : MonoBehaviour
{
    private Rigidbody handRigidbody;

    // Nodo XR per identificare la mano (LeftHand o RightHand)
    public XRNode handNode = XRNode.LeftHand;

    // Dispositivo di input per il feedback aptico
    private InputDevice targetDevice;

    // Soglia minima di velocità per attivare l'aptica
    public float minimumCollisionSpeed = 0.1f;

    // Velocità massima considerata per l'aptica
    public float maximumCollisionSpeed = 5.0f;

    // Intensità massima dell'aptica (da 0.0 a 1.0)
    public float maximumHapticIntensity = 1.0f;

    // Durata dell'impulso aptico
    public float hapticDuration = 0.1f;

    void Start()
    {
        handRigidbody = GetComponent<Rigidbody>();

        // Inizializza il dispositivo di input
        TryInitialize();
    }

    void TryInitialize()
    {
        targetDevice = InputDevices.GetDeviceAtXRNode(handNode);
    }

    void Update()
    {
        // Se il dispositivo non è valido, tenta di inizializzarlo nuovamente
        if (!targetDevice.isValid)
        {
            TryInitialize();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignora le collisioni con le proprie parti del corpo
        if (collision.gameObject == gameObject)
            return;

        // Calcola la velocità della collisione
        float collisionSpeed = collision.relativeVelocity.magnitude;

        // Se la velocità è sotto la soglia minima, non attivare l'aptica
        if (collisionSpeed < minimumCollisionSpeed)
            return;

        // Mappa la velocità della collisione all'intensità aptica
        float intensity = Mathf.Clamp01(collisionSpeed / maximumCollisionSpeed) * maximumHapticIntensity;

        // Invia l'impulso aptico
        if (targetDevice.isValid)
        {
            HapticCapabilities capabilities;
            if (targetDevice.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
            {
                uint channel = 0;
                targetDevice.SendHapticImpulse(channel, intensity, hapticDuration);
            }
        }
    }
}
