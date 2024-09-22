using UnityEngine;

public class VibrationController : MonoBehaviour
{
    public static VibrationController Instance { get; private set; }

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartVibration(float frequency, float amplitude)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
    }

    public void UpdateVibration(float frequency, float amplitude)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
    }

    public void StopVibration()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}
