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
            if (dot > 0)
            {
                // Rimuove la componente di velocità che spinge il player nel muro
                playerRigidbody.velocity -= dot * currentWallNormal;
            }

            // Applica una piccola forza verso il basso solo se il player è a terra
            if (IsGrounded())
            {
                Vector3 slideForce = new Vector3(0, -0.2f, 0);  // Forza verticale ridotta per meno scivolamento
                playerRigidbody.AddForce(slideForce, ForceMode.Acceleration);
            }
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
        CapsuleCollider playerCollider = GetComponent<CapsuleCollider>();
        Vector3 groundCheckPos = playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0);
        return Physics.OverlapSphere(groundCheckPos, locomotionManager.groundCheckRadius, locomotionManager.groundLayers).Length > 0;
    }

    bool IsWall(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
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
