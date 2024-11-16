using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private static SceneController instance;

    [Header("Configurazione")]
    [Tooltip("Tempo di ritardo prima del cambio di scena")]
    public float sceneLoadDelay = 1.0f; // Variabile per il delay regolabile dall'Inspector

    public static SceneController Instance
    {
        get
        {
            if (instance == null)
            {
                // Crea dinamicamente il SceneController se non esiste
                GameObject newController = new GameObject("SceneController");
                instance = newController.AddComponent<SceneController>();
                DontDestroyOnLoad(newController);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mantiene il GameObject tra le scene
        }
        else
        {
            Destroy(gameObject); // Elimina il duplicato
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        yield return new WaitForSeconds(2f); // Simula caricamento aggiuntivo
        asyncLoad.allowSceneActivation = true;
    }
}
