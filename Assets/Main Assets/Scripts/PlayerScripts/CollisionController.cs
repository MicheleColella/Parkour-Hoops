using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollisionController : MonoBehaviour
{
    public VRLocomotionManager locomotionManager;
    public Rigidbody playerRigidbody;

    private Vector3 currentWallNormal = Vector3.zero;

    void OnCollisionStay(Collision collision)
    {
        if (!IsGrounded() && IsWall(collision))
        {
            currentWallNormal = GetWallNormal(collision);
            float dot = Vector3.Dot(playerRigidbody.velocity, currentWallNormal);
            
            // Se il player è in volo e colpisce un muro, rimuove la componente di velocità lungo il muro e applica lo scivolamento
            if (dot > 0)
            {
                playerRigidbody.velocity -= dot * currentWallNormal;
            }
            
            // Aggiungi una forza costante verso il basso per simulare lo scivolamento lungo il muro
            Vector3 slideForce = new Vector3(0, -1f, 0);  // Forza di scivolamento verso il basso
            playerRigidbody.AddForce(slideForce, ForceMode.Acceleration);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (IsWall(collision))
        {
            currentWallNormal = Vector3.zero;
        }
    }

    bool IsGrounded()
    {
        // Implementa qui il controllo per entrambi i collider (corpo e testa) se necessario
        CapsuleCollider playerCollider = GetComponent<CapsuleCollider>();
        Vector3 groundCheckPos = playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0);
        return Physics.OverlapSphere(groundCheckPos, locomotionManager.groundCheckRadius, locomotionManager.groundLayers).Length > 0;
    }

    bool IsWall(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Verifica se il contatto non è con il suolo, quindi è un muro
            if (Mathf.Abs(Vector3.Dot(contact.normal, Vector3.up)) < 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    Vector3 GetWallNormal(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Mathf.Abs(Vector3.Dot(contact.normal, Vector3.up)) < 0.5f)
            {
                return contact.normal;
            }
        }
        return Vector3.zero;
    }
}
