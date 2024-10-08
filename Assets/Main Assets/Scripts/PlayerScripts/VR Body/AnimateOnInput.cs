using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OVRAnimationInput
{
    public string animationPropertyName;
    public OVRInput.Axis1D ovrAxis; // Utilizzeremo Axis1D per ottenere un valore float (da 0 a 1)
}

public class AnimateOnInput : MonoBehaviour
{
    public List<OVRAnimationInput> animationInputs;
    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        foreach (var item in animationInputs)
        {
            // Legge il valore dell'input dall'Oculus controller per l'asse specificato
            float actionValue = OVRInput.Get(item.ovrAxis);
            animator.SetFloat(item.animationPropertyName, actionValue);
        }
    }
}
