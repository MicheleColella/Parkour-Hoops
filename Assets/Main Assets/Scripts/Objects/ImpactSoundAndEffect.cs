using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ImpactSoundAndEffect : MonoBehaviour
{
    public List<AudioClip> impactSounds;       // Lista di suoni da usare per gli impatti
    public GameObject impactFXPrefab;          // Prefab dell'effetto di impatto (opzionale)
    public float impactThreshold = 1.0f;       // Soglia minima per attivare l'effetto visivo
    public float effectLifetime = 2.0f;        // Durata dell'effetto visivo
    public float impactFXSize = 1.0f;          // Dimensione massima dell'effetto di impatto

    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource non assegnato. Verrà ignorato il suono di impatto.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Calcola la velocità relativa al momento dell'impatto
        float impactVelocity = collision.relativeVelocity.magnitude;

        // Riproduce il suono in base all'intensità dell'impatto
        PlayImpactSound(impactVelocity);

        // Se la velocità di impatto è minore della soglia, non fare nulla
        if (impactVelocity < impactThreshold) return;

        // Instanzia l'effetto di impatto in base alla velocità
        InstantiateImpactFX(collision.contacts[0].point, impactVelocity);
    }

    private void PlayImpactSound(float impactVelocity)
    {
        if (audioSource == null || impactSounds.Count == 0)
            return;

        // Calcola l'indice del suono in base alla velocità di impatto
        int soundIndex = Mathf.Clamp((int)(impactVelocity / impactThreshold * impactSounds.Count), 0, impactSounds.Count - 1);
        AudioClip selectedClip = impactSounds[soundIndex];

        // Calcola il volume in base alla velocità e al numero di suoni nella lista
        float volume = Mathf.Clamp01(impactVelocity / impactThreshold) / impactSounds.Count;

        // Riproduce il suono selezionato con il volume calcolato
        audioSource.PlayOneShot(selectedClip, volume);
    }

    private void InstantiateImpactFX(Vector3 position, float impactVelocity)
    {
        if (impactFXPrefab == null)
            return;

        // Calcola la scala dell'effetto basata sull'intensità dell'impatto
        float scale = Mathf.Clamp(impactVelocity / impactThreshold, 0.1f, 1.0f) * impactFXSize;

        // Instanzia l'effetto visivo all'impatto e imposta la scala calcolata
        GameObject impactEffect = Instantiate(impactFXPrefab, position, Quaternion.identity);
        impactEffect.transform.localScale = Vector3.one * scale;

        // Distrugge l'effetto visivo dopo un tempo definito
        Destroy(impactEffect, effectLifetime);
    }
}
