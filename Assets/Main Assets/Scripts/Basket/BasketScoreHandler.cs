using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketScoreHandler : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Il tag dell'oggetto che può attivare il trigger.")]
    public string targetTag = "Ball";

    [Tooltip("Tempo di cooldown in secondi durante il quale il trigger è inattivo.")]
    public float triggerCooldown = 2f;

    private bool isTriggerOnCooldown = false; // Indica se il trigger è in cooldown

    [Header("Visual Effects")]
    [Tooltip("Prefab da istanziare all'attivazione del trigger.")]
    public GameObject VFXToInstantiate;
    [Tooltip("Posizione e trasformazioni del VFX da istanziare.")]
    public Transform vfxPosition;
    [Tooltip("Tempo dopo il quale il VFX verrà distrutto.")]
    public float destroyDelay = 2f;

    [Header("Audio Settings")]
    [Tooltip("AudioSource per il suono da riprodurre quando il trigger viene attivato.")]
    public AudioSource audioSource;

    [Space]
    [Tooltip("AudioSource per il fischio riprodotto all'inizio.")]
    public AudioSource whistleAudioSource;
    [Tooltip("Se true, riproduce un fischio quando l'oggetto viene attivato.")]
    public bool playWhistleOnStart = false;

    [Header("References")]
    [Tooltip("Gestisce il punteggio della scena.")]
    private PointManager pointManager; // Riferimento al PointManager che gestisce il punteggio
    [Tooltip("Riferimento al BasketSectionManager, se presente.")]
    private BasketSectionManager sectionManager; // Riferimento opzionale al BasketSectionManager

    private void Awake()
    {
        // Trova automaticamente l'istanza di PointManager nella scena
        pointManager = FindObjectOfType<PointManager>();

        // Trova il BasketSectionManager nella scena, se esiste
        sectionManager = FindObjectOfType<BasketSectionManager>();

        // Verifica se sono stati trovati
        if (pointManager == null)
        {
            Debug.LogError("PointManager non trovato nella scena! Assicurati che esista un oggetto con lo script PointManager.");
        }
    }

    private void OnEnable()
    {
        // Riproduce il fischio se il bool è true e l'audio source è assegnato
        if (playWhistleOnStart && whistleAudioSource != null)
        {
            whistleAudioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggerOnCooldown) return; // Blocca l'attivazione se in cooldown

        if (other.CompareTag(targetTag))
        {
            // Avvia le azioni del trigger e inizia il cooldown
            StartCoroutine(HandleTriggerCooldown());
        }
    }

    private IEnumerator HandleTriggerCooldown()
    {
        // Attiva il cooldown
        isTriggerOnCooldown = true;

        // Aggiorna il punteggio
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

        // Notifica il BasketSectionManager solo se esiste
        sectionManager?.OnPrefabTriggerActivated();

        // Aspetta il tempo di cooldown
        yield return new WaitForSeconds(triggerCooldown);

        // Disattiva il cooldown
        isTriggerOnCooldown = false;
    }
}
