using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class GrabableEffectiveChildOf : MonoBehaviour
{
    public Transform original_parent;
    public Holdable holdable_type;
    public Transform attach_transform;
    private Vector3 original_local_pos;
    private Quaternion original_local_rot;

    private void Awake() {
        Debug.Assert( holdable_type != Holdable.None);
        
        original_local_pos = this.transform.localPosition;
        original_local_rot = this.transform.localRotation;
    }

    public void HardReset() {
        this.transform.SetParent(original_parent, false);
        this.transform.localPosition = original_local_pos;
        this.transform.localRotation = original_local_rot;
    }
    
    public void OnGrabbed(SelectEnterEventArgs args) {
        string obj_tag =  args.interactorObject.transform.gameObject.tag;
        
        bool is_left  = obj_tag == "LeftController";
        bool is_right = obj_tag == "RightController";

        if (!is_left && !is_right) {
            return;
        }
        
        GrabReceiver receiver = args.interactorObject.transform.gameObject.GetComponent<GrabReceiver>();

        if (receiver == null) {
            Debug.LogWarning("GrabableEffectiveChildOF: Failed To get receiver component");
            return; 
        }
    
        receiver.PushInteractable(this.transform, attach_transform, holdable_type);
        /*
        * 
        if (attach_transform != null) {
            // @Note (fulcrum): Computing the local positon of the visual underneith the attach transform, which we then use as new local transform underneith the parrent obj.
            Vector3 attachToVisualPos = attach_transform.InverseTransformPoint(this.transform.position);
            Quaternion attachToVisualRot = Quaternion.Inverse(attach_transform.rotation) * this.transform.rotation;

            this.transform.localPosition = attachToVisualPos;
            this.transform.localRotation = attachToVisualRot;
        }
        
        this.transform.SetParent(receiver.attach_parent_transform, false);
 */
    }

    public void OnDropped(SelectExitEventArgs args)
    {
        string obj_tag =  args.interactorObject.transform.gameObject.tag;
        
        bool is_left  = obj_tag == "LeftController";
        bool is_right = obj_tag == "RightController";

        if (!is_left && !is_right) {
            return;
        }
        
        GrabReceiver receiver = args.interactorObject.transform.gameObject.GetComponent<GrabReceiver>();

        if (receiver == null) {
            Debug.LogWarning("GrabableEffectiveChildOF: Failed To get receiver component");
        }
        
        receiver.PopInteractable();
        this.transform.SetParent(original_parent, false);
        this.transform.localPosition = original_local_pos;
        this.transform.localRotation = original_local_rot;
    }
}