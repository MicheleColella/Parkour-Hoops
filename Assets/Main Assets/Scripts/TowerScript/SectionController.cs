using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;

public class SectionController : MonoBehaviour
{
    // List of interactive element categories
    public List<ElementCategory> categories;

    // Probability to avoid immediate repetition of the same elements in the same section
    [Range(0f, 1f)]
    public float nonRepetitionProbability = 1f; // 1 means always avoid immediate repetition

    // List of IDs of elements activated in this section
    private List<string> activatedElementIDs = new List<string>();

    void Awake()
    {
        // Deactivate all elements at the start
        DeactivateAllElements();
    }

    public void ActivateRandomElements(List<string> previousActivatedElementIDs, string sectionType)
    {
        activatedElementIDs.Clear(); // Initialize the list for this section

        // Activate a random number of elements in each category
        foreach (var category in categories)
        {
            ActivateElementsInCategory(category, previousActivatedElementIDs);
        }
    }

    void ActivateElementsInCategory(ElementCategory category, List<string> previousActivatedElementIDs)
    {
        List<GameObject> elements = category.elements;

        // Check if the category should be activated based on activation frequency
        if (Random.value > category.activationFrequency)
        {
            //Debug.Log("Category " + category.categoryName + " not activated due to appearance frequency.");
            return;
        }

        // Check that there are elements in the category
        if (elements.Count == 0)
        {
            Debug.LogWarning("No elements available in category " + category.categoryName);
            return;
        }

        // Maximum number of elements we can activate (no more than the number of available elements)
        int maxAllowed = Mathf.Min(category.maxToActivate, elements.Count);

        // Random number of elements to activate, between minToActivate and maxAllowed
        int elementsToActivate = GetRandomInt(category.minToActivate, maxAllowed + 1);

        // Create a list of available indices
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < elements.Count; i++)
        {
            availableIndices.Add(i);
        }

        // List to hold indices to potentially remove
        List<int> indicesToRemove = new List<int>();

        // Identify indices to remove based on nonRepetitionProbability
        if (previousActivatedElementIDs != null && previousActivatedElementIDs.Count > 0)
        {
            for (int i = 0; i < availableIndices.Count; i++)
            {
                GameObject element = elements[availableIndices[i]];
                ElementIdentifier identifier = element.GetComponent<ElementIdentifier>();
                if (identifier != null && previousActivatedElementIDs.Contains(identifier.elementID))
                {
                    float chance = Random.value;
                    if (chance < nonRepetitionProbability)
                    {
                        indicesToRemove.Add(i); // Index in availableIndices list
                    }
                }
            }

            // Calculate potential available count after removal
            int potentialAvailableCount = availableIndices.Count - indicesToRemove.Count;

            if (potentialAvailableCount >= category.minToActivate)
            {
                // Remove all indices safely
                RemoveIndicesFromAvailable(indicesToRemove, availableIndices);
            }
            else
            {
                // Remove only as many indices as possible while keeping at least minToActivate elements
                int indicesWeCanRemove = availableIndices.Count - category.minToActivate;

                if (indicesWeCanRemove > 0)
                {
                    // Shuffle indicesToRemove
                    Shuffle(indicesToRemove);

                    // Keep only the allowed number of indices to remove
                    indicesToRemove = indicesToRemove.GetRange(0, indicesWeCanRemove);

                    // Remove selected indices
                    RemoveIndicesFromAvailable(indicesToRemove, availableIndices);
                }
                // Else, we cannot remove any indices
            }
        }

        // Update elementsToActivate to be within minToActivate and availableIndices.Count
        elementsToActivate = Mathf.Clamp(elementsToActivate, category.minToActivate, availableIndices.Count);

        // Activate the desired number of elements
        for (int i = 0; i < elementsToActivate; i++)
        {
            int randomIndex = GetRandomInt(0, availableIndices.Count);
            int elementIndex = availableIndices[randomIndex];
            elements[elementIndex].SetActive(true);

            // Register the activated element
            ElementIdentifier identifier = elements[elementIndex].GetComponent<ElementIdentifier>();
            if (identifier != null)
            {
                activatedElementIDs.Add(identifier.elementID);
            }
            else
            {
                Debug.LogWarning("ElementIdentifier not found on " + elements[elementIndex].name);
            }

            // Remove the selected index to avoid duplication
            availableIndices.RemoveAt(randomIndex);
        }
    }

    // Helper method to remove indices from availableIndices
    void RemoveIndicesFromAvailable(List<int> indicesToRemove, List<int> availableIndices)
    {
        // Sort indices in descending order to avoid shifting issues
        indicesToRemove.Sort((a, b) => b.CompareTo(a));
        foreach (int idx in indicesToRemove)
        {
            availableIndices.RemoveAt(idx);
        }
    }

    // Helper method to shuffle a list
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = GetRandomInt(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // Method to get a random integer using RandomNumberGenerator
    int GetRandomInt(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
        {
            throw new System.ArgumentOutOfRangeException("minValue must be less than maxValue");
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

    // Method to get the IDs of elements activated in this section
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

    // Parameter to control the appearance frequency of the category
    [Range(0f, 1f)]
    public float activationFrequency = 1f; // 1 means the category will always appear, 0 never
}
