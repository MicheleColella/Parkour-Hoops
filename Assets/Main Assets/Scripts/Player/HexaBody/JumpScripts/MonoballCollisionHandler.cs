using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoballCollisionHandler : MonoBehaviour
{
    public JumpController jumpController;  // Riferimento al JumpController

    private void OnCollisionEnter(Collision collision)
    {
        if (IsTouchingGround(collision))
        {
            jumpController.SetGrounded(true);  // Notifica al JumpController che è a terra
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsTouchingGround(collision))
        {
            jumpController.SetGrounded(false);  // Notifica al JumpController che ha lasciato il terreno
        }
    }

    // Funzione che verifica se la Monoball è effettivamente a contatto con il suolo
    private bool IsTouchingGround(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Considera il contatto come suolo se la normale del contatto punta verso l'alto
            if (contact.normal.y > 0.5f)
            {
                return true;
            }
        }
        return false;
    }
}
