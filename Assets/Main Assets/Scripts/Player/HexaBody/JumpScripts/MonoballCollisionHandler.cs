using UnityEngine;

public class MonoballCollisionHandler : MonoBehaviour
{
    [Header("References")]
    public JumpController jumpController;

    [Header("Ground Check Settings")]
    public float groundNormalThreshold = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        if (IsTouchingGround(collision))
        {
            jumpController?.SetGrounded(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        jumpController?.SetGrounded(false);
    }

    /// <summary>
    /// Determines if the collision qualifies as touching the ground based on contact normals.
    /// </summary>
    /// <param name="collision">Collision data.</param>
    /// <returns>True if touching ground; otherwise, false.</returns>
    private bool IsTouchingGround(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > groundNormalThreshold)
            {
                return true;
            }
        }
        return false;
    }
}
