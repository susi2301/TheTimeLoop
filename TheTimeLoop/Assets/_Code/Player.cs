using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour
{
    public InputActionAsset input_action_asset;

    public Light left_spot_light;
    public Light right_spot_light;
    
    private InputAction left_flashlight_toggle_action;
    private InputAction right_flashlight_toggle_action;

    private float last_flashlight_input_left;
    private float last_flashlight_input_right;

    
    private void Awake() {
        //input_action_asset.FindAction("LeftFlashlightToggle")
        left_flashlight_toggle_action = input_action_asset.FindAction("LeftFlashlightToggle");
        right_flashlight_toggle_action = input_action_asset.FindAction("RightFlashlightToggle");
        Debug.Assert(left_flashlight_toggle_action != null);
        Debug.Assert(right_flashlight_toggle_action != null);

        last_flashlight_input_left = 0.0f;
        last_flashlight_input_right = 0.0f;

        left_spot_light.enabled = false;
        left_spot_light.enabled = false;
    }

    private void Update()
    {
        bool left_flashlight_btn_released = false;
        bool right_flashlight_btn_release = false;
    
        // check if left flashligth btn was released;
        {
            float left_flashlight_input = left_flashlight_toggle_action.ReadValue<float>();

            if (last_flashlight_input_left > 0.8f && left_flashlight_input < 0.2f) {
                left_flashlight_btn_released = true;
            }

            last_flashlight_input_left = left_flashlight_input;
        }

        // check if right flashlight btn was released;
        {
            float right_flashlight_input = right_flashlight_toggle_action.ReadValue<float>();

            if (last_flashlight_input_right > 0.8f && right_flashlight_input < 0.2f) {
                right_flashlight_btn_release = true;
            }

            last_flashlight_input_right = right_flashlight_input;
        }
        
        
        
        if (left_flashlight_btn_released) {
            left_spot_light.enabled = !left_spot_light.enabled;
        }

        if (right_flashlight_btn_release) {
            right_spot_light.enabled = !right_spot_light.enabled;
        }
        
    }
}
