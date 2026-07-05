using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class GrabableEffectiveChildOf : MonoBehaviour {
    
    public Transform original_parent;
    public Holdable holdable_type;
    public Transform attach_transform;
    public Transform attach_transform_left;
    private Vector3 original_local_pos;
    private Quaternion original_local_rot;

    public event Action<bool /*was_grabbed_left*/> event_on_grabbed; 
    public event Action event_on_dropped;

    public bool is_grabbed_left = false;
    public bool is_grabbed_right = false;

    private void Awake() {
        Debug.Assert( holdable_type != Holdable.None);

        original_local_pos = this.transform.localPosition;
        original_local_rot = this.transform.localRotation;
    }

    public void HardReset() {
        this.transform.SetParent(original_parent, false);
        this.transform.localPosition = original_local_pos;
        this.transform.localRotation = original_local_rot;
        is_grabbed_left  = false;
        is_grabbed_right = false;
    }

    public bool IsGrabbed(){
        return is_grabbed_left || is_grabbed_right;
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
        
        Transform _attach_transform = is_left ? attach_transform_left : attach_transform;

        // QUICK HACK should remove later!
        if (_attach_transform == null){
            _attach_transform = attach_transform;
        }

        receiver.PushInteractable(this.transform, _attach_transform, holdable_type);
        
        if (is_left){
            is_grabbed_left = true;
        }

        if (is_right){
            is_grabbed_right = true;
        }

        HapticDevice device = is_left ? HapticDevice.Left : HapticDevice.Right;
        GameManager.instance.HapticImpulse(0.2f, 0.1f, device);


        if (event_on_grabbed != null){
            event_on_grabbed.Invoke(is_left);
        }
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
        is_grabbed_left  = false;
        is_grabbed_right = false;

        if (event_on_dropped != null) {
            event_on_dropped.Invoke();
        }
    }
}