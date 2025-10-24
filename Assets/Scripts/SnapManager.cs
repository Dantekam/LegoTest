using System.Collections.Generic;
using UnityEngine;

public class SnapManager : MonoBehaviour
{
    [Header("Snapping Settings")]
    public float snapDistance = 0.05f;
    public LayerMask snapPointLayer;
    public Transform snapReceivers;
    public Transform model;

    private List<Transform> snapReceiverList;
    private List<Transform> usedReceivers;
    private List<Transform> otherBrickSnap;

    private void Start()
    {
        snapReceiverList = GetReceivers();
        usedReceivers = new List<Transform>();
    }

    public void TrySnap()
    {
        Transform bestReceiver = null;
        Transform bestSnapPoint = null;
        float closestDistance = float.MaxValue;

        List<Transform> otherBrickSnap = new List<Transform>();

        foreach (Transform receiver in snapReceiverList)
        {
            Collider[] hits = Physics.OverlapSphere(receiver.position, snapDistance, snapPointLayer);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("SnapPoint"))
                {
                    float distance = Vector3.Distance(receiver.position, hit.transform.position);

                    otherBrickSnap.Add(hit.transform);
                    usedReceivers.Add(receiver);

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
            Vector3 averagePosition = Vector3.zero;
            Vector3 averageLocalPostion = Vector3.zero;

            Vector3 sum = Vector3.zero;
            Vector3 sumLocal = Vector3.zero;
            foreach (Transform item in otherBrickSnap)
            {
                sum += item.position;
                sumLocal += item.localPosition;

                Debug.Log($"{item.name} is snapped to {this.name}.");
            }

            averagePosition = sum / otherBrickSnap.Count;
            averageLocalPostion = sumLocal / otherBrickSnap.Count;

            SnapWithFixedJoint(bestReceiver, otherBrickSnap, averagePosition, averageLocalPostion);
        }
    }

    void SnapWithFixedJoint(Transform receiver, List<Transform> snapPointList, Vector3 averagePosition, Vector3 averageLocalPosition)
    {
        Transform otherBrick = snapPointList[0].transform.root;

        model.localPosition = (averagePosition.x > 0? 1 : -1 ) * new Vector3(averageLocalPosition.x, 0, averageLocalPosition.z);
        transform.position = averagePosition + new Vector3(0f, 0.01f, 0f);

        transform.rotation = snapPointList[0].rotation;

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

        foreach (var receivers in usedReceivers)
        {
            receivers.gameObject.SetActive(false);
        }

        foreach (var snap in snapPointList)
        {
            snap.gameObject.SetActive(false);
        }
    }

    List<Transform> GetReceivers()
    {
        List<Transform> receivers = new List<Transform>();
        foreach (Transform child in snapReceivers)
        {
            receivers.Add(child);
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
