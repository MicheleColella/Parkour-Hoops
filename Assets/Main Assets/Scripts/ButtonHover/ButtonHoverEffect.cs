using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;

public class XRButtonHoverEffect : MonoBehaviour
{
    public Animator animator;

    // Funzione chiamata quando il raycaster colpisce il pulsante
    public void OnHoverEnter()
    {
        if (animator != null)
        {
            Debug.Log("Hover Enter");
            animator.SetTrigger("Hover");
        }
    }

    // Funzione chiamata quando il raycaster lascia il pulsante
    public void OnHoverExit()
    {
        if (animator != null)
        {
            Debug.Log("Hover Exit");
            animator.SetTrigger("Normal");
        }
    }
}
