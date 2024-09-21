using UnityEngine;

public class SnapTurnController : MonoBehaviour
{
    public VRLocomotionManager locomotionManager;

    private float lastSnapTime;

    void FixedUpdate()
    {
        HandleSnapTurn();
    }

    void HandleSnapTurn()
    {
        Vector2 snapInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        if (Time.time - lastSnapTime > locomotionManager.snapTurnCooldown)
        {
            if (snapInput.x > 0.5f)
            {
                transform.Rotate(0, locomotionManager.snapTurnAngle, 0);
                lastSnapTime = Time.time;
            }
            else if (snapInput.x < -0.5f)
            {
                transform.Rotate(0, -locomotionManager.snapTurnAngle, 0);
                lastSnapTime = Time.time;
            }
        }
    }
}
