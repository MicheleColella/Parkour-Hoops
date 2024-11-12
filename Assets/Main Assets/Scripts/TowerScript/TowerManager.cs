using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    public List<GameObject> sectionPrefabs;
    public int maxActiveSections = 10;
    public float generationThreshold = 0.75f;
    public float sectionHeight = 10f; // Altezza di una singola sezione

    private List<GameObject> activeSections = new List<GameObject>();
    private Transform player;

    // Tiene traccia dei dati degli elementi attivati per tipo di sezione
    private SectionActivationData sectionActivationData = new SectionActivationData();

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        // Genera le sezioni iniziali
        for (int i = 0; i < maxActiveSections; i++)
        {
            GenerateSection();
        }
    }

    void Update()
    {
        // Controlla se è necessario generare una nuova sezione
        if (player.position.y > GetGenerationPoint())
        {
            GenerateSection();

            if (activeSections.Count > maxActiveSections)
            {
                RemoveOldestSection();
            }
        }
    }

    void GenerateSection()
    {
        Vector3 newPosition = transform.position;

        if (activeSections.Count > 0)
        {
            newPosition = activeSections[activeSections.Count - 1].transform.position + new Vector3(0, sectionHeight, 0);
        }

        // Seleziona casualmente un prefab dalla lista
        int randomIndex = Random.Range(0, sectionPrefabs.Count);
        GameObject selectedPrefab = sectionPrefabs[randomIndex];
        string sectionType = selectedPrefab.name;

        GameObject newSection = Instantiate(selectedPrefab, newPosition, Quaternion.identity);
        activeSections.Add(newSection);

        // Attiva elementi casuali nella nuova sezione, passando i dati di attivazione per quel tipo di sezione
        SectionController sectionController = newSection.GetComponent<SectionController>();
        if (sectionController != null)
        {
            // Ottiene gli elementi attivati l'ultima volta per questo tipo di sezione
            List<string> previousActivatedElementIDs = sectionActivationData.GetActivatedElementsForSectionType(sectionType);

            // Passa i dati al SectionController
            sectionController.ActivateRandomElements(previousActivatedElementIDs, sectionType);

            // Aggiorna sectionActivationData con i dati della sezione corrente
            sectionActivationData.RecordActivation(sectionType, sectionController.GetActivatedElementIDs());
        }

        //Debug.Log("Sezione generata in posizione: " + newPosition);
    }

    void RemoveOldestSection()
    {
        GameObject oldestSection = activeSections[0];
        activeSections.RemoveAt(0);
        Destroy(oldestSection);

        //Debug.Log("Sezione rimossa dalla posizione: " + oldestSection.transform.position);
    }

    float GetGenerationPoint()
    {
        if (activeSections.Count > 0)
        {
            GameObject lastSection = activeSections[activeSections.Count - 1];
            return lastSection.transform.position.y - (1 - generationThreshold) * sectionHeight;
        }
        else
        {
            return transform.position.y;
        }
    }

    void OnDrawGizmos()
    {
        if (activeSections.Count > 0)
        {
            // Posizione di Generazione
            Gizmos.color = Color.green;
            GameObject lastSection = activeSections[activeSections.Count - 1];
            Gizmos.DrawSphere(lastSection.transform.position + new Vector3(0, sectionHeight, 0), 1f);

            // Posizione di Eliminazione
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(activeSections[0].transform.position, 1f);
        }
    }
}
