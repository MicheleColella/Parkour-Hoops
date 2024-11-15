using UnityEngine;

public class ElementIdentifier : MonoBehaviour
{
    public string elementID; // Assegna un ID univoco a ogni elemento nell'Inspector
    public bool useGameObjectName = false; // Bool per usare il nome del GameObject, default false

    private void Awake()
    {
        // Se il bool è attivo, usa il nome del GameObject come elementID
        if (useGameObjectName)
        {
            elementID = gameObject.name;
        }
    }
}
