using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

public class HeightTrigger : MonoBehaviour
{
    private MaxHeightTracker heightTracker;
    public List<MMF_Player> textFeel;
    public GameObject menuCanvas;
    public GameObject objectToNeverActivate; // Oggetto che non deve mai essere attivato
    public List<FollowPlayerYAxis> objectsToControl; // Lista degli oggetti con FollowPlayerYAxis

    private bool activatedTrigger = false;
    private bool triggerActivated = false; // Flag per controllare se il trigger è stato attivato

    void Start()
    {
        menuCanvas.SetActive(false);

        // Disattiva l'oggetto per sicurezza all'inizio
        if (objectToNeverActivate != null)
        {
            objectToNeverActivate.SetActive(false);
        }

        // Trova lo script MaxHeightTracker nella scena
        heightTracker = FindObjectOfType<MaxHeightTracker>();
    }

    void Update()
    {
        if (activatedTrigger)
        {
            // Controllo continuo per mantenere l'oggetto disattivato
            if (objectToNeverActivate != null && objectToNeverActivate.activeSelf)
            {
                objectToNeverActivate.SetActive(false);
                Debug.LogWarning($"{objectToNeverActivate.name} non deve essere attivato. È stato disattivato automaticamente.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggerActivated)
        {
            Debug.Log("PlayerHeight");
            triggerActivated = true; // Imposta il flag per indicare che il trigger è stato attivato

            // Chiama il metodo per fermare il calcolo dell'altezza massima
            heightTracker.StopTrackingHeight();

            menuCanvas.SetActive(true);
            activatedTrigger = true;

            // Disattiva il follow per tutti gli oggetti nella lista
            foreach (var obj in objectsToControl)
            {
                obj.shouldFollow = false;
            }

            foreach (var feels in textFeel)
            {
                feels.PlayFeedbacks();
            }
        }
    }
}
