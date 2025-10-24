using System.Collections.Generic;
using UnityEngine;

public class SnapManager : MonoBehaviour
{
    [Header("Snapping Settings")]
    public float snapDistance = 0.05f;
    public LayerMask snapPointLayer;
    public Transform snapReceivers;

    private List<Transform> snapReceiverList;

    private void Start()
    {
        snapReceiverList = GetReceivers();
    }

    public void TrySnap()
    {
        Transform bestReceiver = null;
        Transform bestSnapPoint = null;
        float closestDistance = float.MaxValue;

        List<Transform> snapPoints = new List<Transform>();

        foreach (Transform receiver in snapReceiverList)
        {
            Collider[] hits = Physics.OverlapSphere(receiver.position, snapDistance, snapPointLayer);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("SnapPoint"))
                {
                    float distance = Vector3.Distance(receiver.position, hit.transform.position);

                    snapPoints.Add(hit.transform);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        bestReceiver = receiver;
                        bestSnapPoint = hit.transform;
                    }
                }
            }
        }

        if (bestReceiver != null && bestSnapPoint != null && snapPoints.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (Transform item in snapPoints)
            {
                sum += item.position;
                Debug.Log($"{item.name} is snapped to {this.name}.");
            }

            Vector3 averagePosition = sum / snapPoints.Count;

            SnapWithFixedJoint(bestReceiver, bestSnapPoint, averagePosition);
        }
    }

    void SnapWithFixedJoint(Transform receiver, Transform snapPoint, Vector3 averagePosition)
    {
        Transform otherBrick = snapPoint.transform.root;

        // Get snapPoint’s position relative to its brick root
        Vector3 localPos = otherBrick.InverseTransformPoint(snapPoint.position);
        float offsetDist = 0.4f;

        Vector3 snapOffset = Vector3.zero;

        // Detect which side/corner this snap point is on the bottom brick ideally
        if (localPos.x > 0.2f)  snapOffset.x = offsetDist;   // right half
        if (localPos.x < -0.2f) snapOffset.x = -offsetDist;  // left half
        if (localPos.z > 0.2f)  snapOffset.z = offsetDist;   // top half
        if (localPos.z < -0.2f) snapOffset.z = -offsetDist;  // bottom half

        snapOffset = -snapOffset;

        // Convert to world-space offset based on the bottom brick’s orientation recommendation from online
        Vector3 worldOffset = otherBrick.TransformDirection(snapOffset);

        // Calculate target position
        Vector3 targetPos = averagePosition + worldOffset + new Vector3(0f, 0.01f, 0f);

        // Align top brick hopefully
        transform.position = targetPos;
        transform.rotation = otherBrick.rotation;

        Rigidbody thisRb = GetComponent<Rigidbody>();
        Rigidbody otherRb = otherBrick.GetComponent<Rigidbody>();

        if (thisRb == null || otherRb == null)
        {
            Debug.LogWarning("Both bricks must have Rigidbody components to snap properly.");
            return;
        }

        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = otherRb;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;

        Debug.Log($"{gameObject.name} snapped to {otherBrick.name} using FixedJoint with corrected offset.");
    }

    List<Transform> GetReceivers()
    {
        List<Transform> receivers = new List<Transform>();
        if (snapReceivers != null)
        {
            foreach (Transform child in snapReceivers)
                receivers.Add(child);
        }
        return receivers;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (snapReceivers != null)
        {
            foreach (Transform receiver in snapReceivers)
                Gizmos.DrawWireSphere(receiver.position, snapDistance);
        }
    }
}
