using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerRig : MonoBehaviour
{
    public Transform playerHead;
    public Transform leftController;
    public Transform rightController;

    public ConfigurableJoint headJoint;
    public ConfigurableJoint leftHandJoint;
    public ConfigurableJoint rightHandJoint;

    public CapsuleCollider bodyCollider;

    public float bodyHeightMin = 0.5f;
    public float bodyHeightMax = 2f;

    [Header("Joint Settings")]
    public float jointPositionSpring = 5000f; // Increased to make joints respond faster
    public float jointPositionDamper = 300f; // Increased damping for quicker response
    public float jointMaximumForce = float.MaxValue;

    void Start()
    {
        ConfigureJoint(leftHandJoint);
        ConfigureJoint(rightHandJoint);
    }

    void FixedUpdate() 
    {
        float headHeight = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        if (!Mathf.Approximately(bodyCollider.height, headHeight))
        {
            bodyCollider.height = headHeight;
            bodyCollider.center = new Vector3(playerHead.localPosition.x, headHeight / 2, playerHead.localPosition.z);
        }

        UpdateJointTarget(leftHandJoint, leftController);
        UpdateJointTarget(rightHandJoint, rightController);
        UpdateJointTarget(headJoint, playerHead);
    }


    void ConfigureJoint(ConfigurableJoint joint)
    {
        if (joint != null)
        {
            JointDrive drive = new JointDrive
            {
                positionSpring = jointPositionSpring,
                positionDamper = jointPositionDamper,
                maximumForce = jointMaximumForce
            };
            joint.xDrive = drive;
            joint.yDrive = drive;
            joint.zDrive = drive;

            JointDrive angularDrive = new JointDrive
            {
                positionSpring = jointPositionSpring,
                positionDamper = jointPositionDamper,
                maximumForce = jointMaximumForce
            };
            joint.angularXDrive = angularDrive;
            joint.angularYZDrive = angularDrive;
        }
    }

    void UpdateJointTarget(ConfigurableJoint joint, Transform target)
    {
        if (joint != null && target != null)
        {
            joint.targetPosition = target.localPosition;
            joint.targetRotation = target.localRotation;
        }
    }
}
