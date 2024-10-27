using UnityEngine;

public class MonoballCollisionHandler : MonoBehaviour
{
    public JumpController jumpController;  // Riferimento al JumpController

    private int groundContactCount = 0;
     
    public bool isGrounded;

    private void OnCollisionEnter(Collision collision)
    {
        if (IsTouchingGround(collision))
        {
            groundContactCount++;
            jumpController.SetGrounded(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    { 
        if (IsTouchingGround(collision))
        {
            groundContactCount--;
            if (groundContactCount <= 0)
            {
                groundContactCount = 0;
                jumpController.SetGrounded(false);
            }
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
                isGrounded = true;
                return true;
            }
        }
        isGrounded = false;
        return false;
    }
}
