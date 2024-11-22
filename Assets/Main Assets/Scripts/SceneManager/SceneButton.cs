using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI; // For TrackedDeviceGraphicRaycaster
using System.Collections.Generic; // For List

public class SceneButton : MonoBehaviour
{
    [Tooltip("Nome della scena da caricare")]
    public string sceneName; // Nome della scena da caricare
    private Button button; // Riferimento al componente Button

    [Tooltip("Lista dei TrackedDeviceGraphicRaycaster da disabilitare")]
    public List<TrackedDeviceGraphicRaycaster> raycastersToDisable;

    [Tooltip("Lista di GameObject da disabilitare quando il pulsante è cliccato")]
    public List<GameObject> objectsToDisable; // Lista di GameObject da disabilitare

    private void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Il componente Button non è stato trovato su questo GameObject!");
        }
    }

    public void OnButtonClick()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // Disabilita il pulsante per evitare clic multipli
            button.interactable = false;

            BlinkValueControl.ToggleBlink(false);

            // Disabilita i TrackedDeviceGraphicRaycaster nella lista
            if (raycastersToDisable != null)
            {
                foreach (var raycaster in raycastersToDisable)
                {
                    if (raycaster != null)
                    {
                        raycaster.enabled = false;
                    }
                }
            }

            // Disabilita gli oggetti della lista objectsToDisable
            if (objectsToDisable != null)
            {
                foreach (var obj in objectsToDisable)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false); // Disabilita il GameObject
                    }
                }
            }

            // Usa SceneController per caricare la scena specificata
            SceneController.Instance.LoadScene(sceneName);
            //Debug.Log($"Button clicked! Loading scene: {sceneName}");
        }
        else
        {
            Debug.LogError("Il nome della scena non è stato impostato nel ButtonHandler!");
        }
    }

    private void OnEnable()
    {
        // Opzionalmente, riattiva il pulsante quando il GameObject viene attivato
        if (button != null)
        {
            button.interactable = true;
        }

        // Riattiva gli oggetti disabilitati quando il GameObject viene attivato
        if (objectsToDisable != null)
        {
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(true); // Riattiva il GameObject
                }
            }
        }
    }
}
