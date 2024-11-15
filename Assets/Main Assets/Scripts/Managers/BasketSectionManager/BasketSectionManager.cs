using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketSectionManager : MonoBehaviour
{
    public List<GameObject> prefabs; // Lista dei prefab presenti nella scena
    public float deactivateDelay = 2f; // Tempo dopo il quale disattivare il prefab attuale
    private GameObject currentActivePrefab; // Prefab attualmente attivo

    private void Start()
    {
        // Disattiva tutti i prefabs all'inizio
        foreach (var prefab in prefabs)
        {
            prefab.SetActive(false);
        }

        // Attiva un prefab random iniziale
        ActivateRandomPrefab();
    }

    public void OnPrefabTriggerActivated()
    {
        if (currentActivePrefab != null)
        {
            // Disattiva il prefab attuale con un delay
            StartCoroutine(DeactivateWithDelay(currentActivePrefab));
        }

        // Attiva un nuovo prefab random
        ActivateRandomPrefab();
    }

    private void ActivateRandomPrefab()
    {
        if (prefabs.Count == 0)
        {
            Debug.LogError("La lista dei prefabs è vuota! Aggiungi almeno un prefab alla lista.");
            return;
        }

        // Scegli un prefab random dalla lista
        int randomIndex = Random.Range(0, prefabs.Count);
        currentActivePrefab = prefabs[randomIndex];
        currentActivePrefab.SetActive(true);
    }

    private IEnumerator DeactivateWithDelay(GameObject prefab)
    {
        yield return new WaitForSeconds(deactivateDelay);
        if (prefab != null)
        {
            prefab.SetActive(false);
        }
    }
}
