using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIVisibilityController : MonoBehaviour
{
    public Transform hand;      // Riferimento alla mano
    public Transform head;      // Riferimento alla testa (da cui parte il raycast)
    public CanvasGroup uiCanvasGroup; // Gruppo Canvas per gestire l'alfa e fare il fade

    public float rotationZMin; // Rotazione minima per attivare la UI sull'asse Z
    public float rotationZMax; // Rotazione massima per attivare la UI sull'asse Z
    public float maxRaycastDistance = 5.0f; // Massima distanza del raycast
    public float fadeDuration = 0.5f; // Durata del fade

    private Coroutine fadeCoroutine;
    private bool isVisible = false; // Stato corrente della visibilità della UI

    void Update()
    {
        // Disegna il raycast nel Scene View per il debug
        Debug.DrawRay(head.position, head.forward * maxRaycastDistance, Color.red);

        // Controlla se la rotazione della mano sull'asse Z è entro il range specificato
        if (IsHandRotationInRangeZ())
        {
            // Esegue un raycast dalla testa che attraversa tutti gli oggetti
            RaycastHit[] hits = Physics.RaycastAll(head.position, head.forward, maxRaycastDistance);

            // Scorre tutti gli oggetti colpiti dal raycast
            foreach (RaycastHit hit in hits)
            {
                // Debug per vedere tutti gli oggetti colpiti
                //Debug.Log("Raycast ha colpito: " + hit.transform.name);

                // Verifica se uno degli oggetti colpiti è la mano
                if (hit.transform == hand)
                {
                    // Attiva la UI con il fade
                    SetUIVisibility(true);
                    return;
                }
            }
        }

        // Disattiva la UI con il fade se le condizioni non sono soddisfatte
        SetUIVisibility(false);
    }

    private void SetUIVisibility(bool visible)
    {
        // Controlla se lo stato di visibilità è già quello desiderato
        if (isVisible == visible) return;

        // Aggiorna lo stato di visibilità
        isVisible = visible;

        // Interrompe il fade precedente se è ancora in corso
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // Avvia una nuova coroutine di fade verso il targetAlpha
        fadeCoroutine = StartCoroutine(FadeUI(visible ? 1 : 0));
    }

    private IEnumerator FadeUI(float targetAlpha)
    {
        float startAlpha = uiCanvasGroup.alpha;
        float time = 0;

        // Disabilita l'interazione e i raycast se si sta facendo il fade out
        uiCanvasGroup.interactable = targetAlpha > 0;
        uiCanvasGroup.blocksRaycasts = targetAlpha > 0;

        while (time < fadeDuration)
        {
            // Interpolazione dell'alfa
            uiCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        // Assicura che il valore finale sia esattamente il target
        uiCanvasGroup.alpha = targetAlpha;
    }

    private bool IsHandRotationInRangeZ()
    {
        float handRotationZ = NormalizeAngle(hand.localEulerAngles.z);

        // Verifica se la rotazione della mano sull'asse Z è entro i limiti definiti
        return handRotationZ >= rotationZMin && handRotationZ <= rotationZMax;
    }

    // Funzione per normalizzare l'angolo nel range 0-360
    private float NormalizeAngle(float angle)
    {
        while (angle < 0) angle += 360;
        while (angle > 360) angle -= 360;
        return angle;
    }
}
