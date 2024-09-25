using UnityEngine;

public class ControllerCollisionDetector : MonoBehaviour
{
    public bool isTouchingSurface = false;

    void OnTriggerEnter(Collider collision)
    {
        isTouchingSurface = true;
    }

    void OnTriggerExit(Collider collision)
    {
        isTouchingSurface = false;
    }

    void OnTriggerStay(Collider other)
    {
        isTouchingSurface = true;
    }
}
