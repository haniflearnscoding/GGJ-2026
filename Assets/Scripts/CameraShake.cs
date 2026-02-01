using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        Instance = this;

        // Add impulse source to this object
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
        {
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        Debug.Log("CameraShake: Ready with Cinemachine Impulse");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T pressed - test shake");
            ShakeHeavy();
        }
    }

    public void ShakeLight()
    {
        impulseSource.GenerateImpulse(0.3f);
    }

    public void ShakeMedium()
    {
        impulseSource.GenerateImpulse(0.6f);
    }

    public void ShakeHeavy()
    {
        impulseSource.GenerateImpulse(1f);
    }
}
