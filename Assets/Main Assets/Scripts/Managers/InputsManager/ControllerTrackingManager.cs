using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ControllerTrackingManager : MonoBehaviour
{
    [Header("Left Hand Components")]
    public Collider[] leftHandColliders;
    public SkinnedMeshRenderer leftHandSkinnedMeshRenderer;

    [Header("Right Hand Components")]
    public Collider[] rightHandColliders;
    public SkinnedMeshRenderer rightHandSkinnedMeshRenderer;

    void Update()
    {
        // Controlla lo stato di tracciamento della mano sinistra
        CheckHandTracking(InputDeviceCharacteristics.Left, leftHandColliders, leftHandSkinnedMeshRenderer);

        // Controlla lo stato di tracciamento della mano destra
        CheckHandTracking(InputDeviceCharacteristics.Right, rightHandColliders, rightHandSkinnedMeshRenderer);
    }

    private void CheckHandTracking(InputDeviceCharacteristics handType, Collider[] handColliders, SkinnedMeshRenderer handSkinnedMeshRenderer)
    {
        // Trova i dispositivi con le caratteristiche specificate (destra o sinistra)
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(handType | InputDeviceCharacteristics.Controller, devices);

        bool isTracked = false;

        foreach (var device in devices)
        {
            if (device.isValid && device.TryGetFeatureValue(CommonUsages.isTracked, out isTracked) && isTracked)
            {
                break; // Se almeno uno dei dispositivi è tracciato, interrompi il ciclo
            }
        }

        // Modifica lo stato del trigger del Collider in base allo stato di tracciamento
        if (handColliders != null)
        {
            foreach (var collider in handColliders)
            {
                if (collider != null)
                {
                    collider.isTrigger = !isTracked; // Imposta il trigger quando non è tracciato
                }
            }
        }

        // Attiva o disattiva lo Skinned Mesh Renderer in base allo stato di tracciamento
        if (handSkinnedMeshRenderer != null)
        {
            handSkinnedMeshRenderer.enabled = isTracked;
        }
    }
}
