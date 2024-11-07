using UnityEngine;
using UnityEngine.Events;

public class LeverController : MonoBehaviour
{
    [Header("Lever Settings")]
    public Transform leverTransform;
    public Vector2 lowerRange = new Vector2(45f, 90f);
    public Vector2 upperRange = new Vector2(-90f, -45f);
    public Rigidbody leverRigidbody;

    [Header("Gizmo Settings")]
    public Transform gizmoCenter;

    [Header("Slider Settings")]
    public float sliderMinValue = 0f;
    public float sliderMaxValue = 100f;
    public float sliderValue;

    [Header("Lever Events")]
    public UnityEvent onLowerRangeReached;
    public UnityEvent onUpperRangeReached;

    [Header("Current Rotation (Read-Only)")]
    [SerializeField] private float currentRotationValue;

    [Header("Velocity Damping")]
    public float velocityDamping = 0.9f;

    private bool lowerRangeTriggered = false;
    private bool upperRangeTriggered = false;
    private bool gravityDisabled = false;

    private void Update()
    {
        if (leverTransform == null || leverRigidbody == null) return;

        float currentRotation = leverTransform.localEulerAngles.x;
        if (currentRotation > 180f)
        {
            currentRotation -= 360f;
        }
        currentRotationValue = currentRotation;

        sliderValue = Mathf.InverseLerp(upperRange.x, lowerRange.y, currentRotation) * (sliderMaxValue - sliderMinValue) + sliderMinValue;

        if (currentRotation >= lowerRange.x && currentRotation <= lowerRange.y)
        {
            if (!lowerRangeTriggered)
            {
                lowerRangeTriggered = true;
                upperRangeTriggered = false;
                onLowerRangeReached.Invoke();
            }
            DisableGravityAndDampenVelocity();
        }
        else if (currentRotation >= upperRange.x && currentRotation <= upperRange.y)
        {
            if (!upperRangeTriggered)
            {
                upperRangeTriggered = true;
                lowerRangeTriggered = false;
                onUpperRangeReached.Invoke();
            }
            DisableGravityAndDampenVelocity();
        }
        else
        {
            lowerRangeTriggered = false;
            upperRangeTriggered = false;
            EnableGravity();
        }

        if (gravityDisabled)
        {
            leverRigidbody.velocity *= velocityDamping;
        }
    }

    private void DisableGravityAndDampenVelocity()
    {
        leverRigidbody.useGravity = false;
        gravityDisabled = true;
    }

    private void EnableGravity()
    {
        leverRigidbody.useGravity = true;
        gravityDisabled = false;
    }

    private void OnDrawGizmos()
    {
        if (gizmoCenter == null) return;

        Vector3 centerPosition = gizmoCenter.position;
        Quaternion gizmoRotation = gizmoCenter.rotation * Quaternion.Euler(0, 0, 90); // Rotazione addizionale di 90 gradi sull'asse Y

        DrawFilledArc(centerPosition, gizmoRotation, lowerRange.x, lowerRange.y, Color.red);
        DrawFilledArc(centerPosition, gizmoRotation, upperRange.x, upperRange.y, Color.green);
    }


    private void DrawFilledArc(Vector3 position, Quaternion rotation, float startAngle, float endAngle, Color color)
    {
        Gizmos.color = color;
        int segments = 30;
        float radius = 0.25f;

        Vector3 previousPoint = position + rotation * (Quaternion.AngleAxis(startAngle, Vector3.up) * Vector3.right * radius);

        for (int i = 1; i <= segments; i++)
        {
            float lerpAngle = Mathf.Lerp(startAngle, endAngle, i / (float)segments);
            Vector3 nextPoint = position + rotation * (Quaternion.AngleAxis(lerpAngle, Vector3.up) * Vector3.right * radius);

            Gizmos.DrawLine(previousPoint, nextPoint);
            Gizmos.DrawLine(position, nextPoint);

            previousPoint = nextPoint;
        }
    }
}
