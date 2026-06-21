using UnityEngine;
using UnityEngine.Events;


public enum Holdable {
    None = 0,
    ClockWeight,
    Flashlight,
    Torch,
}

public class GrabReceiver : MonoBehaviour {
    public Transform attach_parent_transform;

    public Holdable curr_holding = Holdable.None;

    public UnityEvent<Holdable> event_on_grabbed_holdable;
    public UnityEvent<Holdable> event_on_dropped_holdable;


    public void HardReset() {
        curr_holding = Holdable.None;
    }
    
    public void PushInteractable(Transform interactable_transform, Transform interactable_attach_transform, Holdable holdable) {

        Debug.Assert(curr_holding == Holdable.None);
        Debug.Assert(holdable != Holdable.None);
        
        if (interactable_attach_transform != null) {
            // @Note (fulcrum): Computing the local positon of the visual underneith the attach transform, which we then use as new local transform underneith the parrent obj.
            Vector3 attachToVisualPos = interactable_attach_transform.InverseTransformPoint(interactable_transform.position);
            Quaternion attachToVisualRot = Quaternion.Inverse(interactable_attach_transform.rotation) * interactable_transform.rotation;

            interactable_transform.localPosition = attachToVisualPos;
            interactable_transform.localRotation = attachToVisualRot;
        }

        curr_holding = holdable;
        interactable_transform.SetParent(attach_parent_transform, false);
        event_on_grabbed_holdable.Invoke(curr_holding);
    }

    public void PopInteractable() {
        Debug.Assert(curr_holding != Holdable.None);
        event_on_dropped_holdable.Invoke(curr_holding);
        curr_holding = Holdable.None;
    }
}
