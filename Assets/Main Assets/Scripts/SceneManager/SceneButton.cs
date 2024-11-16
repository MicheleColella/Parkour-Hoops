using UnityEngine;
using UnityEngine.UI;

public class SceneButton : MonoBehaviour
{
    [Tooltip("Nome della scena da caricare")]
    public string sceneName; // Nome della scena da caricare
    private Button button; // Riferimento al componente Button

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

            // Usa SceneController per caricare la scena specificata
            SceneController.Instance.LoadScene(sceneName);
            Debug.Log($"Button clicked! Loading scene: {sceneName}");
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
    }
}
