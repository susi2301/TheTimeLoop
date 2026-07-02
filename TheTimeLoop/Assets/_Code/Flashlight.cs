using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Flashlight : MonoBehaviour
{
    public Light spot_light;
    public bool enable_light_at_startup = false;
    
    public InputEvent left_flashlight_input_event;
    public InputEvent right_flashlight_input_event;
    
    public GrabableEffectiveChildOf child_of;
    public XRGrabInteractable xr_interactable;
    
    public ShaderFloatAnimator mat_animator;
    public float mat_emission_strength = 5.0f;
    
    public MenuMatSwitcher menu_mat_switcher;

    private bool listen_to_input = false;
    private Vector3 spawn_pos;
    private Quaternion spawn_rot;

    private void Awake() {
        spawn_pos = this.transform.position;
        spawn_rot = this.transform.rotation;
    }

    private void Start(){
        child_of.event_on_grabbed += OnGrabbed;
        HardReset();
    }

    public void HardReset() {

        if (child_of.IsGrabbed()) {
            // @Note: This forces the interactable to be dropped.
            // otherwise it would stay in the hand after a restart.
            xr_interactable.enabled = false;
            xr_interactable.enabled = true;
        }

        spot_light.enabled = enable_light_at_startup;
        if (enable_light_at_startup) {
            mat_animator.JustSetThisValueAndDontAskAnyQuestions(mat_emission_strength,"_EmissionStrength");
        } else {
            mat_animator.JustSetThisValueAndDontAskAnyQuestions(0.0f,"_EmissionStrength");
        }
        
        this.transform.position = spawn_pos;
        this.transform.rotation = spawn_rot;
        
        left_flashlight_input_event.Reset();
        right_flashlight_input_event.Reset();
        
        child_of.HardReset();
    }

    private void Update() {
        
        if (!listen_to_input || !child_of.IsGrabbed()) {
            return;
        }

        // Poll Input
        bool left_was_pressed  = left_flashlight_input_event.Poll(InputPollMode.OnPressed);
        bool right_was_pressed = right_flashlight_input_event.Poll(InputPollMode.OnPressed);
        
        if (child_of.is_grabbed_left && left_was_pressed) {
            Debug.Assert(!child_of.is_grabbed_right);

            ToggleLight();
        }
        
        if (child_of.is_grabbed_right && right_was_pressed) {
            Debug.Assert(!child_of.is_grabbed_left);
            
            ToggleLight();
        }
    }

    public void OnGrabbed(bool was_grabbed_left) {
        left_flashlight_input_event.Reset();
        right_flashlight_input_event.Reset();
        Debug.Log("Flashlight grabbed!");
    }
    
    public void ToggleLight() {
        spot_light.enabled = !spot_light.enabled;
        if (spot_light.enabled) {
            mat_animator.JustSetThisValueAndDontAskAnyQuestions(mat_emission_strength,"_EmissionStrength");
        } else {
            mat_animator.JustSetThisValueAndDontAskAnyQuestions(0.0f,"_EmissionStrength");
        }
    }

    // called when going out of menu
    public void EnableInputs(){
        
        menu_mat_switcher.SwitchToInGameMat();
        listen_to_input = true;
    }

    // called when going into menu
    public void DisableInputs() {

        if (child_of.IsGrabbed()){
            menu_mat_switcher.SwitchToMenuMat();
        }

        listen_to_input = false;
    }
}
