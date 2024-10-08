using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPhysics : MonoBehaviour
{
    public OVRInput.Button grabButton = OVRInput.Button.PrimaryHandTrigger; // Usa il pulsante del grilletto della mano
    public float radius = 0.1f;
    public LayerMask grabLayer;

    private ConfigurableJoint configurableJoint;  // Usare ConfigurableJoint per più controllo
    private bool isGrabbing = false;
    private Rigidbody grabbedObjectRb; // Per salvare il Rigidbody dell'oggetto afferrato

    // Parametri del joint esposti nell'Inspector
    public float positionSpring = 500f;  // Puoi modificare questo valore dall'Inspector
    public float positionDamper = 10f;   // Valore per il damper
    public float maximumForce = 1000f;   // Forza massima del joint

    void FixedUpdate()
    {
        bool isGrabButtonPressed = OVRInput.Get(grabButton);

        if (isGrabButtonPressed && !isGrabbing)
        {
            // Controlla se c'è un oggetto vicino da afferrare
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius, grabLayer, QueryTriggerInteraction.Ignore);

            if (nearbyColliders.Length > 0)
            {
                Rigidbody nearbyRigidBody = nearbyColliders[0].attachedRigidbody;

                if (nearbyRigidBody)
                {
                    // Assegna il Rigidbody dell'oggetto afferrato
                    grabbedObjectRb = nearbyRigidBody;

                    // Crea il ConfigurableJoint per l'oggetto
                    configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
                    configurableJoint.autoConfigureConnectedAnchor = false;
                    configurableJoint.connectedBody = nearbyRigidBody;

                    // Imposta il ConfigurableJoint per limitare l'influenza sull'oggetto
                    configurableJoint.anchor = transform.InverseTransformPoint(grabbedObjectRb.transform.position);
                    configurableJoint.xMotion = ConfigurableJointMotion.Locked;
                    configurableJoint.yMotion = ConfigurableJointMotion.Locked;
                    configurableJoint.zMotion = ConfigurableJointMotion.Locked;

                    configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
                    configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
                    configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;

                    // Imposta i limiti per impedire che l'oggetto influenzi troppo il player
                    JointDrive drive = new JointDrive();
                    drive.positionSpring = positionSpring;  // Usa il valore esposto nell'Inspector
                    drive.positionDamper = positionDamper;  // Usa il valore esposto nell'Inspector
                    drive.maximumForce = maximumForce;      // Usa il valore esposto nell'Inspector

                    configurableJoint.xDrive = drive;
                    configurableJoint.yDrive = drive;
                    configurableJoint.zDrive = drive;

                    isGrabbing = true;
                }
            }
        }
        else if (!isGrabButtonPressed && isGrabbing)
        {
            // Rilascia l'oggetto
            isGrabbing = false;

            if (configurableJoint)
            {
                Destroy(configurableJoint);
            }

            grabbedObjectRb = null;
        }
    }
}
