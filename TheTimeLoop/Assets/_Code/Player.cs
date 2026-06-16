using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;


// TODO: make some clean polling system.

//public enum BtnInputEventType {
//    MenuBtnPressed = 0,
//    LeftFlashlightPressed,
//    RightFlashlightPressed,
//}

//public struct BtnInputEvent {
//    public InputAction action;
//    public float last_value;
//}


public class Player : MonoBehaviour {
    
    public MenuManager menu_manager;
    public Transform camera_transform;
    
    public InputActionAsset input_action_asset;
    
    private InputAction menu_input_action;
    private float last_menu_input_value;
    
    public Light left_spot_light;
    public Light right_spot_light;
    
    private InputAction left_flashlight_toggle_action;
    private InputAction right_flashlight_toggle_action;
    private float last_flashlight_input_left;
    private float last_flashlight_input_right;

    public XROrigin xr_origin;
    private Vector3 spawn_cam_pos;
    private Vector3 spawn_cam_forward;

    public TeleportationProvider teleport_provider;
    public DynamicMoveProvider  move_provider;
    public JumpProvider jump_provider;
    public ClimbProvider climp_provider;
    
    // Note: set be game manager during Awake!
    [HideInInspector] public bool DEV_skip_menu_on_load;
    private void Awake()
    {

        Debug.Assert(menu_manager != null);
        Debug.Assert(camera_transform != null);
        Debug.Assert(xr_origin != null);

        spawn_cam_pos = camera_transform.transform.position;
        spawn_cam_forward = camera_transform.transform.forward;

        menu_input_action = input_action_asset.FindAction("Menu");
        Debug.Assert(menu_input_action != null);
        last_menu_input_value = 0.0f;
        
        //input_action_asset.FindAction("LeftFlashlightToggle")
        left_flashlight_toggle_action = input_action_asset.FindAction("LeftFlashlightToggle");
        right_flashlight_toggle_action = input_action_asset.FindAction("RightFlashlightToggle");
        Debug.Assert(left_flashlight_toggle_action != null);
        Debug.Assert(right_flashlight_toggle_action != null);

        last_flashlight_input_left = 0.0f;
        last_flashlight_input_right = 0.0f;

        left_spot_light.enabled = false;
        left_spot_light.enabled = false;

        jump_provider.enabled = false;
        climp_provider.enabled = false;
    }

    private void Start() {
        if (!DEV_skip_menu_on_load) {
            menu_manager.OpenMenu(true);
        }
    }

    private void Update()
    {
        // Check Main Menu btn
        {
            
            float menu_input_value = menu_input_action.ReadValue<float>();

            if (menu_input_value > 0.8f && last_menu_input_value < 0.2f) {
                //Debug.Log("MENU BTN PRESSED");

                if (menu_manager.IsMenuOpen()) {
                    menu_manager.CloseMenu();
                }
                else {
                    menu_manager.OpenMenu();
                }
            }

            last_menu_input_value = menu_input_value;
        }
        
        
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
            Debug.Log("FLASHLIGHT BTN");
            left_spot_light.enabled = !left_spot_light.enabled;
        }

        if (right_flashlight_btn_release) {
            Debug.Log("FLASHLIGHT BTN");
            right_spot_light.enabled = !right_spot_light.enabled;
        }
        
    }


    public void EnableInGameInputs() {
        teleport_provider.enabled = true;
        move_provider.enabled = true;
    }

    public void DisableInGameInputs() {
        teleport_provider.enabled = false;
        move_provider.enabled = false;
    }
    
    public void ResetTransformsToSpawn() {
        xr_origin.MoveCameraToWorldLocation(spawn_cam_pos);
        xr_origin.MatchOriginUpCameraForward(Vector3.up,spawn_cam_forward);
    }
}
