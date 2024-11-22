using UnityEngine;

public class BlinkValueControl : MonoBehaviour
{
    // Riferimento al materiale da modificare
    public Material material;

    // ID del valore da modificare nello shader
    public string valueID = "_Value";

    // Valori minimi e massimi per il range del valore
    public float minValue = 0f;
    public float maxValue = 1f;

    // Velocità con cui il valore cambia
    public float changeSpeed = 0.5f;

    // Variabile per gestire il blink (accensione/spegnimento)
    public bool blink;

    // Variabili per tenere traccia del valore attuale
    private float currentValue;

    // Variabile statica per accedere al controllo blink da altri script
    private static BlinkValueControl _instance;

    private void Start()
    {
        // Inizializza il valore corrente dallo shader
        if (material != null)
        {
            currentValue = material.GetFloat(valueID);
        }

        // Memorizza l'istanza corrente della classe per accedere alla funzione statica
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Update()
    {
        // Aumenta o diminuisce il valore a seconda dello stato di blink
        if (blink)
        {
            IncreaseValue();
        }
        else
        {
            DecreaseValue();
        }

        // Assicurati che il materiale venga aggiornato con il nuovo valore
        if (material != null)
        {
            material.SetFloat(valueID, Mathf.Lerp(material.GetFloat(valueID), currentValue, changeSpeed * Time.deltaTime));
        }
    }

    // Funzione per aumentare il valore
    public void IncreaseValue()
    {
        // Incrementa il valore corrente, rispettando il range
        currentValue = Mathf.Clamp(currentValue + Time.deltaTime * changeSpeed, minValue, maxValue);
    }

    // Funzione per diminuire il valore
    public void DecreaseValue()
    {
        // Decrementa il valore corrente, rispettando il range
        currentValue = Mathf.Clamp(currentValue - Time.deltaTime * changeSpeed, minValue, maxValue);
    }

    // Funzione statica per attivare/disattivare il blink da altri script
    public static void ToggleBlink(bool status)
    {
        if (_instance != null)
        {
            _instance.blink = status;
        }
    }
}
