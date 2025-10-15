using Oculus.Interaction;
using UnityEngine;

public class SnappingGrabbable : Grabbable
{
    public override void ProcessPointerEvent(PointerEvent evt)
    {
        base.ProcessPointerEvent(evt);

        if (evt.Type == PointerEventType.Unselect)
        {
            GetComponent<GrabbableSnapBridge>()?.TriggerSnap();
        }
    }
}
