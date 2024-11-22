using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    // Riferimento all'AudioSource
    public AudioSource audioSource;

    // Riferimento allo Slider
    public Slider volumeSlider;

    // Metodo per inizializzare lo slider con il volume corrente dell'AudioSource
    void Start()
    {
        // Imposta lo slider all'inizio in base al volume corrente dell'AudioSource
        if (volumeSlider != null)
        {
            volumeSlider.value = audioSource.volume;
            volumeSlider.onValueChanged.AddListener(UpdateVolume);
        }
    }

    // Metodo per aggiornare il volume dell'AudioSource
    public void UpdateVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
