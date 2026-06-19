using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Flashlight : MonoBehaviour
{
    public InputEvent left_flashlight_input_event;
    public InputEvent right_flashlight_input_event;
    
    public XRGrabInteractable xr_interactable;
    
    public Light spot_light;
    
    public bool enable_light_at_startup = false;
    
    public bool is_grabbed_left = false;
    public bool is_grabbed_right = false;

    private Vector3 spawn_pos;
    private Quaternion spawn_rot;

    public GrabableEffectiveChildOf child_of;
    
    private void Awake() {
        spawn_pos = this.transform.position;
        spawn_rot = this.transform.rotation;
        
        HardReset();
    }

    public void HardReset() {

        if (IsGrabbed()) {
            // @Note: This forces the interactable to be dropped.
            // otherwise it would stay in the hand after a restart.
            xr_interactable.enabled = false;
            xr_interactable.enabled = true;
        }

        spot_light.enabled = enable_light_at_startup;
        is_grabbed_left = false;
        is_grabbed_right = false;

        this.transform.position = spawn_pos;
        this.transform.rotation = spawn_rot;
        
        left_flashlight_input_event.Reset();
        right_flashlight_input_event.Reset();
        
        child_of.HardReset();
    }

    private void Update() {
        
        if (!IsGrabbed()) {
            return;
        }

        bool left_was_pressed = left_flashlight_input_event.Poll(InputPollMode.OnPressed);
        bool right_was_pressed = right_flashlight_input_event.Poll(InputPollMode.OnPressed);
        
        // Poll inputs
       // bool toggle_flashlight = false;
        
        if (is_grabbed_left && left_was_pressed) {
            Debug.Assert(!is_grabbed_right);

            ToggleLight();
        }
        
        if (is_grabbed_right && right_was_pressed) {
            Debug.Assert(!is_grabbed_left);
            
            ToggleLight();
        }
    }

    public bool IsGrabbed() {
        return is_grabbed_left || is_grabbed_right;
    }

    public void OnGrabbed(SelectEnterEventArgs args) {
        string obj_tag =  args.interactorObject.transform.gameObject.tag;
        
        is_grabbed_left  = obj_tag == "LeftController";
        is_grabbed_right = obj_tag == "RightController";
        
        left_flashlight_input_event.Reset();
        right_flashlight_input_event.Reset();
    }

    public void OnUngrabbed(SelectExitEventArgs args) {

        is_grabbed_left = false;
        is_grabbed_right = false;
    }
    
    public void ToggleLight() {
        spot_light.enabled = !spot_light.enabled;
    }
}
