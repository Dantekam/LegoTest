using UnityEngine;
using Oculus.Interaction;

public class GrabbableSnapBridge : MonoBehaviour
{
    private SnapManager snapManager;

    void Start()
    {
        snapManager = GetComponent<SnapManager>();
    }

    public void TriggerSnap()
    {
        if (snapManager != null)
        {
            Debug.Log($"{gameObject.name} released — triggering snap.");
            snapManager.TrySnap();
        }
    }
}
