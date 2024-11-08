using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ImpactSoundAndEffect : MonoBehaviour
{
    public List<AudioClip> impactSounds;
    public GameObject impactFXPrefab;
    public float impactThreshold = 1.0f;
    public float effectLifetime = 2.0f;
    public float impactFXSize = 1.0f;

    private Rigidbody rb;
    private AudioSource audioSource;

    public bool isAttracting = false;

    // Variabile per gestire il cooldown
    private float lastImpactTime = 0.0f;
    public float impactCooldown = 0.2f;  // Tempo minimo tra un impatto e l'altro in secondi

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
        // Salta l'esecuzione se l'oggetto è in stato di attrazione
        if (isAttracting) return;

        // Verifica se è trascorso abbastanza tempo dall'ultimo impatto
        if (Time.time - lastImpactTime < impactCooldown) return;

        float impactVelocity = collision.relativeVelocity.magnitude;

        // Riproduce il suono e l’effetto visivo solo se la velocità supera la soglia
        if (impactVelocity >= impactThreshold)
        {
            PlayImpactSound(impactVelocity);
            InstantiateImpactFX(collision.contacts[0].point, impactVelocity);
        }

        // Aggiorna il tempo dell'ultimo impatto
        lastImpactTime = Time.time;
    }

    private void PlayImpactSound(float impactVelocity)
    {
        if (audioSource == null || impactSounds.Count == 0)
            return;

        int soundIndex = Mathf.Clamp((int)(impactVelocity / impactThreshold * impactSounds.Count), 0, impactSounds.Count - 1);
        AudioClip selectedClip = impactSounds[soundIndex];
        float volume = Mathf.Clamp01(impactVelocity / impactThreshold) / impactSounds.Count;

        audioSource.PlayOneShot(selectedClip, volume);
    }

    private void InstantiateImpactFX(Vector3 position, float impactVelocity)
    {
        if (impactFXPrefab == null)
            return;

        float scale = Mathf.Clamp(impactVelocity / impactThreshold, 0.1f, 1.0f) * impactFXSize;
        GameObject impactEffect = Instantiate(impactFXPrefab, position, Quaternion.identity);
        impactEffect.transform.localScale = Vector3.one * scale;

        Destroy(impactEffect, effectLifetime);
    }
}
