using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    // Modifica la fluidità per una risposta più rapida
    public float smoothness = 0.5f;  // Valore più alto per movimenti più veloci

    public void Map()
    {
        // Posizione interpolata (Lerp)
        Vector3 targetPosition = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.position = Vector3.Lerp(ikTarget.position, targetPosition, smoothness);

        // Rotazione interpolata (Slerp)
        Quaternion targetRotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
        ikTarget.rotation = Quaternion.Slerp(ikTarget.rotation, targetRotation, smoothness);
    }
}

public class IKTargetFollowVRRig : MonoBehaviour
{
    [Range(0, 1)]
    public float turnSmoothness = 0.5f;  // Velocità di rotazione del corpo aumentata

    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;

    // Update is called once per frame
    void LateUpdate()
    {
        // Fluidità per la posizione del corpo
        Vector3 targetPosition = head.ikTarget.position + headBodyPositionOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, turnSmoothness);

        // Fluidità per la rotazione del corpo
        float yaw = head.vrTarget.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSmoothness);

        // Aggiorna le posizioni e rotazioni interpolando
        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}