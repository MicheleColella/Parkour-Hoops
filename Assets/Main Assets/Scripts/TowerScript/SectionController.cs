using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;

public class SectionController : MonoBehaviour
{
    // Lista delle categorie di elementi interattivi
    public List<ElementCategory> categories;

    // Probabilità di evitare la ripetizione immediata degli stessi elementi nella stessa sezione
    [Range(0f, 1f)]
    public float nonRepetitionProbability = 1f; // 1 significa evitare sempre la ripetizione immediata

    // Lista degli ID degli elementi attivati in questa sezione
    private List<string> activatedElementIDs = new List<string>();

    void Awake()
    {
        // Disattiva tutti gli elementi all'inizio
        DeactivateAllElements();
    }

    public void ActivateRandomElements(List<string> previousActivatedElementIDs, string sectionType)
    {
        activatedElementIDs.Clear(); // Inizializza la lista per questa sezione

        // Attiva un numero casuale di elementi in ciascuna categoria
        foreach (var category in categories)
        {
            ActivateElementsInCategory(category, previousActivatedElementIDs);
        }
    }

    void ActivateElementsInCategory(ElementCategory category, List<string> previousActivatedElementIDs)
    {
        List<GameObject> elements = category.elements;

        // Verifica se la categoria deve essere attivata in base alla frequenza di attivazione
        if (Random.value > category.activationFrequency)
        {
            Debug.Log("Categoria " + category.categoryName + " non attivata a causa della frequenza di apparizione.");
            return;
        }

        // Verifica che ci siano elementi nella categoria
        if (elements.Count == 0)
        {
            Debug.LogWarning("Nessun elemento disponibile nella categoria " + category.categoryName);
            return;
        }

        // Numero massimo di elementi che possiamo attivare (non più del numero di elementi disponibili)
        int maxAllowed = Mathf.Min(category.maxToActivate, elements.Count);

        // Numero casuale di elementi da attivare, tra minToActivate e maxAllowed
        int elementsToActivate = GetRandomInt(category.minToActivate, maxAllowed + 1);

        // Creiamo una lista degli indici disponibili
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < elements.Count; i++)
        {
            availableIndices.Add(i);
        }

        // Rimuovi gli elementi che sono stati attivati l'ultima volta in questa stessa sezione in base a nonRepetitionProbability
        if (previousActivatedElementIDs != null && previousActivatedElementIDs.Count > 0)
        {
            for (int i = availableIndices.Count - 1; i >= 0; i--)
            {
                GameObject element = elements[availableIndices[i]];
                ElementIdentifier identifier = element.GetComponent<ElementIdentifier>();
                if (identifier != null && previousActivatedElementIDs.Contains(identifier.elementID))
                {
                    float chance = Random.value;
                    if (chance < nonRepetitionProbability)
                    {
                        // Rimuovi questo indice per evitare la ripetizione immediata
                        availableIndices.RemoveAt(i);
                    }
                }
            }
        }

        // Aggiorna elementsToActivate nel caso abbiamo meno indici disponibili
        elementsToActivate = Mathf.Min(elementsToActivate, availableIndices.Count);

        // Attiva il numero desiderato di elementi, scegliendo indici casuali dalla lista degli indici disponibili
        for (int i = 0; i < elementsToActivate; i++)
        {
            int randomIndex = GetRandomInt(0, availableIndices.Count);
            int elementIndex = availableIndices[randomIndex];
            elements[elementIndex].SetActive(true);

            // Registra l'elemento attivato
            ElementIdentifier identifier = elements[elementIndex].GetComponent<ElementIdentifier>();
            if (identifier != null)
            {
                activatedElementIDs.Add(identifier.elementID);
            }
            else
            {
                Debug.LogWarning("ElementIdentifier non trovato su " + elements[elementIndex].name);
            }

            // Rimuovi l'indice selezionato per evitare duplicazioni
            availableIndices.RemoveAt(randomIndex);
        }
    }

    // Metodo per ottenere un intero casuale utilizzando RandomNumberGenerator
    int GetRandomInt(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
        {
            throw new System.ArgumentOutOfRangeException("minValue deve essere minore di maxValue");
        }

        long diff = (long)maxValue - minValue;
        byte[] uint32Buffer = new byte[4];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            while (true)
            {
                rng.GetBytes(uint32Buffer);
                uint rand = System.BitConverter.ToUInt32(uint32Buffer, 0);

                long max = (1 + (long)uint.MaxValue);
                long remainder = max % diff;

                if (rand < max - remainder)
                {
                    return (int)(minValue + (rand % diff));
                }
            }
        }
    }

    void DeactivateAllElements()
    {
        foreach (var category in categories)
        {
            foreach (GameObject element in category.elements)
            {
                element.SetActive(false);
            }
        }
    }

    // Metodo per ottenere gli ID degli elementi attivati in questa sezione
    public List<string> GetActivatedElementIDs()
    {
        return activatedElementIDs;
    }
}

[System.Serializable]
public class ElementCategory
{
    public string categoryName;
    public List<GameObject> elements;
    public int minToActivate = 1;
    public int maxToActivate = 1;

    // Nuovo parametro per controllare la frequenza di apparizione della categoria
    [Range(0f, 1f)]
    public float activationFrequency = 1f; // 1 significa che la categoria apparirà sempre, 0 mai
}
