using System.Collections.Generic;
using UnityEngine;

public class SnapManager : MonoBehaviour
{
    [Header("Snapping Settings")]
    public float snapDistance = 0.05f;
    public LayerMask snapPointLayer;

    public void TrySnap()
    {
        List<Transform> receivers = GetReceivers();
        Transform bestReceiver = null;
        Transform bestSnapPoint = null;
        float closestDistance = float.MaxValue;

        foreach (Transform receiver in receivers)
        {
            Collider[] hits = Physics.OverlapSphere(receiver.position, snapDistance, snapPointLayer);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("SnapPoint"))
                {
                    float distance = Vector3.Distance(receiver.position, hit.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        bestReceiver = receiver;
                        bestSnapPoint = hit.transform;
                    }
                }
            }
        }

        if (bestReceiver != null && bestSnapPoint != null)
        {
            SnapWithFixedJoint(bestReceiver, bestSnapPoint);
        }
    }

    void SnapWithFixedJoint(Transform receiver, Transform snapPoint)
    {
        Transform otherBrick = snapPoint.transform.root;

        // Align receiver to snapPoint without changing scale
        Vector3 receiverOffset = transform.position - receiver.position;
        transform.position = snapPoint.position + receiverOffset;
        transform.rotation = snapPoint.rotation;

        // Preserve scale — do NOT reset to Vector3.one
        // Avoid parenting — use FixedJoint instead

        Rigidbody thisRb = GetComponent<Rigidbody>();
        Rigidbody otherRb = otherBrick.GetComponent<Rigidbody>();

        if (thisRb == null || otherRb == null)
        {
            Debug.LogWarning("Both bricks must have Rigidbody components.");
            return;
        }

        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = otherRb;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;

        Debug.Log($"{gameObject.name} snapped to {otherBrick.name} using FixedJoint.");
    }

    List<Transform> GetReceivers()
    {
        List<Transform> receivers = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("SnapReceiver"))
            {
                receivers.Add(child);
            }
        }
        return receivers;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (Transform receiver in GetReceivers())
        {
            Gizmos.DrawWireSphere(receiver.position, snapDistance);
        }
    }
}
