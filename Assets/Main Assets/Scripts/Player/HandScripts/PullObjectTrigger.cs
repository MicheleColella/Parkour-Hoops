using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PullObjectTrigger : MonoBehaviour
{
    [Header("Pull Settings")]
    public float attractionSpeed = 5f;
    public LayerMask targetLayer;
    public Transform handOrigin;

    [Header("Input Action")]
    public InputActionProperty pullInputAction;

    [Header("Hand Grabbing Reference")]
    public GrabPhysics grabPhysics; // Riferimento allo script GrabPhysics della mano

    [Header("Pull Restrictions")]
    [Tooltip("Massa massima degli oggetti che possono essere tirati.")]
    public float maxPullableMass = 10f; // Imposta il valore desiderato

    [Header("Prefab Settings")]
    public GameObject prefabToInstantiate;
    public Vector3 prefabScale = Vector3.one;
    public float prefabFollowSpeed = 100f;

    [Header("Debug")]
    public bool isAttracting = false;

    [HideInInspector]
    public Rigidbody attractedObject;
    private List<Rigidbody> objectsInTrigger = new List<Rigidbody>();

    private Rigidbody currentCandidateObject = null;
    private GameObject instantiatedPrefab = null;

    void OnEnable()
    {
        pullInputAction.action.Enable();
    }

    void OnDisable()
    {
        pullInputAction.action.Disable();
    }

    void Update()
    {
        // Controlla se la mano sta attualmente afferrando un oggetto
        if (grabPhysics != null && grabPhysics.isGrabbing)
        {
            // La mano sta afferrando un oggetto, quindi non attirare altri oggetti
            if (isAttracting)
            {
                StopAttracting();
            }
            DestroyInstantiatedPrefab();
            return;
        }

        bool pullButtonHeld = pullInputAction.action.ReadValue<float>() > 0.1f;

        // Aggiorna l'oggetto candidato attuale
        UpdateCurrentCandidateObject();

        if (pullButtonHeld)
        {
            if (currentCandidateObject != null && !isAttracting)
            {
                // Inizia ad attirare l'oggetto candidato
                attractedObject = currentCandidateObject;
                isAttracting = true;

                // Disabilita la gravità
                attractedObject.useGravity = false;

                // Distruggi il prefab poiché stiamo ora tirando l'oggetto
                DestroyInstantiatedPrefab();

                Debug.Log("Iniziato ad attirare l'oggetto: " + attractedObject.name);
            }
        }
        else
        {
            // Se il pulsante di tiro non è premuto, smetti di attirare
            if (isAttracting)
            {
                StopAttracting();
            }
        }

        // Aggiorna la posizione del prefab per seguire l'oggetto candidato
        if (instantiatedPrefab != null && currentCandidateObject != null)
        {
            float step = prefabFollowSpeed * Time.deltaTime;
            instantiatedPrefab.transform.position = Vector3.MoveTowards(
                instantiatedPrefab.transform.position,
                currentCandidateObject.position,
                step
            );
        }
    }

    void FixedUpdate()
    {
        if (isAttracting && attractedObject != null)
        {
            // Controlla se l'oggetto viene afferrato durante l'attrazione
            GrabbableObject grabbable = attractedObject.GetComponentInParent<GrabbableObject>();
            if (grabbable != null && grabbable.isGrabbed)
            {
                StopAttracting();
                return;
            }

            // Muovi l'oggetto verso l'origine della mano
            Vector3 direction = (handOrigin.position - attractedObject.position);
            attractedObject.velocity = direction.normalized * attractionSpeed;

            // Se l'oggetto è abbastanza vicino, smetti di attirare
            if (direction.magnitude < 0.1f)
            {
                Debug.Log("Oggetto raggiunto l'origine della mano: " + attractedObject.name);
                StopAttracting();
            }
        }
        else if (attractedObject != null)
        {
            // Assicurati che l'oggetto smetta di muoversi se non viene attirato
            attractedObject.velocity = Vector3.zero;
        }
    }

    private void UpdateCurrentCandidateObject()
    {
        Rigidbody closestObject = GetClosestCandidateObject();

        if (closestObject != currentCandidateObject)
        {
            // L'oggetto candidato è cambiato
            DestroyInstantiatedPrefab();

            currentCandidateObject = closestObject;

            if (currentCandidateObject != null)
            {
                // Instanzia il prefab alla posizione dell'oggetto candidato
                InstantiatePrefabAtCandidateObject();
            }
        }
        else if (currentCandidateObject != null)
        {
            // Verifica se l'oggetto candidato è ancora valido
            GrabbableObject grabbable = currentCandidateObject.GetComponentInParent<GrabbableObject>();
            if (grabbable == null || !grabbable.canBePulled || grabbable.isGrabbed || currentCandidateObject.mass > maxPullableMass)
            {
                // L'oggetto candidato non è più valido
                DestroyInstantiatedPrefab();
                currentCandidateObject = null;
            }
        }
    }

    private Rigidbody GetClosestCandidateObject()
    {
        Rigidbody closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Rigidbody obj in objectsInTrigger)
        {
            GrabbableObject grabbable = obj.GetComponentInParent<GrabbableObject>();
            if (grabbable == null || !grabbable.canBePulled || grabbable.isGrabbed || obj.mass > maxPullableMass)
            {
                continue; // Salta gli oggetti che non possono essere tirati o sono già afferrati
            }

            if (isAttracting && obj == attractedObject)
            {
                continue; // Salta l'oggetto attualmente in attrazione
            }

            float distance = Vector3.Distance(obj.position, handOrigin.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = obj;
            }
        }

        return closest;
    }

    private void InstantiatePrefabAtCandidateObject()
    {
        if (prefabToInstantiate != null && currentCandidateObject != null)
        {
            instantiatedPrefab = Instantiate(prefabToInstantiate, currentCandidateObject.position, Quaternion.identity);
            instantiatedPrefab.transform.localScale = prefabScale;
        }
    }

    private void DestroyInstantiatedPrefab()
    {
        if (instantiatedPrefab != null)
        {
            Destroy(instantiatedPrefab);
            instantiatedPrefab = null;
        }
    }

    public void StopAttracting()
    {
        if (attractedObject != null)
        {
            isAttracting = false;
            attractedObject.useGravity = true;
            attractedObject.velocity = Vector3.zero;
            attractedObject = null;
            Debug.Log("Smetti di attirare l'oggetto.");
        }
    }

    public bool HasObjectsInTrigger()
    {
        // Ritorna true se c'è almeno un oggetto nel trigger che soddisfa i criteri
        foreach (Rigidbody obj in objectsInTrigger)
        {
            GrabbableObject grabbable = obj.GetComponentInParent<GrabbableObject>();
            if (grabbable != null && grabbable.canBePulled && !grabbable.isGrabbed && obj.mass <= maxPullableMass)
            {
                return true;
            }
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto è nel layer target
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Rigidbody rb = other.attachedRigidbody; // Usa attachedRigidbody per ottenere il Rigidbody associato al Collider
            if (rb != null && !objectsInTrigger.Contains(rb))
            {
                objectsInTrigger.Add(rb);
                Debug.Log("Oggetto entrato nel trigger: " + other.name);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && objectsInTrigger.Contains(rb))
        {
            // Se l'oggetto che stiamo attirando esce dal trigger, smetti di attirare
            if (rb == attractedObject)
            {
                StopAttracting();
                Debug.Log("Oggetto attirato uscito dal trigger.");
            }

            // Se l'oggetto è il candidato attuale, resettalo e distruggi il prefab
            if (rb == currentCandidateObject)
            {
                DestroyInstantiatedPrefab();
                currentCandidateObject = null;
            }

            objectsInTrigger.Remove(rb);
            Debug.Log("Oggetto uscito dal trigger: " + other.name);
        }
    }
}
