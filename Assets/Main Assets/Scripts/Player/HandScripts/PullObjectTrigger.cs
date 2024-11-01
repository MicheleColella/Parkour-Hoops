using System.Collections.Generic;
using UnityEngine;

public class PullObjectTrigger : MonoBehaviour
{
    public float attractionSpeed = 5f;
    public LayerMask targetLayer;
    public Transform handOrigin;
    public bool isAttracting;
    public bool attractOBJ;

    private Rigidbody attractedObject;
    private List<Rigidbody> objectsInTrigger = new List<Rigidbody>();

    void Update()
    {
        if (attractOBJ && objectsInTrigger.Count > 0 && !isAttracting)
        {
            // Trova l'oggetto più vicino all'HandOrigin
            attractedObject = GetClosestObject();
            if (attractedObject != null)
            {
                // Controlla se il Rigidbody non è kinematic
                if (!attractedObject.isKinematic)
                {
                    isAttracting = true;
                    attractedObject.useGravity = false;

                    Debug.Log("Inizio ad attirare l'oggetto: " + attractedObject.name);
                }
                else
                {
                    Debug.Log("L'oggetto " + attractedObject.name + " è kinematic, non può essere attirato.");
                    attractedObject = null;
                }
            }
        }

        // Debug visivo
        if (isAttracting && attractedObject != null)
        {
            Debug.DrawLine(attractedObject.position, handOrigin.position, Color.red);
        }
    }

    void FixedUpdate()
    {
        if (isAttracting && attractedObject != null)
        {
            // Calcola la direzione verso l'HandOrigin
            Vector3 direction = (handOrigin.position - attractedObject.position).normalized;

            // Imposta la velocità del Rigidbody per muoverlo
            attractedObject.velocity = direction * attractionSpeed;

            // Se l'oggetto è molto vicino all'HandOrigin, ferma l'attrazione
            if (Vector3.Distance(attractedObject.position, handOrigin.position) < 0.1f)
            {
                isAttracting = false;
                attractedObject.useGravity = true;
                attractedObject.velocity = Vector3.zero; // Ferma il movimento

                Debug.Log("L'oggetto " + attractedObject.name + " è arrivato all'HandOrigin.");
                attractedObject = null;
            }
        }
        else if (attractedObject != null)
        {
            // Se non stiamo più attirando, assicuriamoci che la velocità sia zero
            attractedObject.velocity = Vector3.zero;
        }
    }

    private Rigidbody GetClosestObject()
    {
        Rigidbody closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Rigidbody obj in objectsInTrigger)
        {
            float distance = Vector3.Distance(obj.position, handOrigin.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = obj;
            }
        }

        return closest;
    }

    void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto è nel layer target
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                objectsInTrigger.Add(rb);
                Debug.Log("Oggetto entrato nel trigger: " + other.name);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && objectsInTrigger.Contains(rb))
        {
            objectsInTrigger.Remove(rb);
            Debug.Log("Oggetto uscito dal trigger: " + other.name);

            // Se l'oggetto che stiamo attirando esce dal trigger, fermiamo l'attrazione
            if (rb == attractedObject)
            {
                isAttracting = false;
                attractedObject.useGravity = true;
                attractedObject.velocity = Vector3.zero; // Ferma il movimento
                attractedObject = null;
                Debug.Log("L'oggetto attirato è uscito dal trigger.");
            }
        }
    }
}