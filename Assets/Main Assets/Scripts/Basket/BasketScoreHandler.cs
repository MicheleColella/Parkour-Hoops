using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketScoreHandler : MonoBehaviour
{
    public string targetTag = "Ball";  // Il tag dell'oggetto da rilevare
    public GameObject VFXToInstantiate;  // Prefab da instanziare
    public Transform vfxPosition;  // Posizione, rotazione e scala da usare per l'istanziamento
    public float destroyDelay = 2f;  // Tempo dopo il quale distruggere l'oggetto
    public AudioSource audioSource;  // AudioSource per il suono da riprodurre

    private PointManager pointManager;  // Riferimento al PointManager che gestisce il punteggio

    private void Awake()
    {
        // Trova automaticamente l'istanza di PointManager nella scena
        pointManager = FindObjectOfType<PointManager>();

        // Verifica se è stato trovato
        if (pointManager == null)
        {
            Debug.LogError("PointManager non trovato nella scena! Assicurati che esista un oggetto con lo script PointManager.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Richiama il metodo di aggiornamento del punteggio nel PointManager
            pointManager?.AddPoints(1);

            // Riproduce il suono dall'AudioSource
            if (audioSource != null)
            {
                audioSource.Play();
            }

            // Instanzia il prefab alla posizione, rotazione e scala dello spawnPoint
            if (VFXToInstantiate != null && vfxPosition != null)
            {
                GameObject instantiatedObject = Instantiate(
                    VFXToInstantiate,
                    vfxPosition.position,
                    vfxPosition.rotation
                );

                instantiatedObject.transform.localScale = vfxPosition.localScale;

                // Distruggi l'oggetto dopo un certo ritardo
                Destroy(instantiatedObject, destroyDelay);
            }
        }
    }
}
