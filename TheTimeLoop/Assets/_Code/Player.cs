using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Player : MonoBehaviour {
    
    public bool DEV_skip_menu_on_load;
    public bool DEV_using_device_simulator;
    public GameObject DEV_device_simulator_prefab;
    [Space]
    

    public MenuManager menu_manager;
    public Transform camera_transform;
    public Transform camera_offset_transform;

    public InputEvent menu_input_event;
    public GameSettingsSO game_settings;


    public XROrigin xr_origin;

    private Vector3 xr_origin_spawn_pos;
    private Vector3 spawn_cam_pos;
    private Vector3 spawn_cam_forward;

    // @Note: we need all this to turn them of during menu screen since disabling inputs directly seems very buggy with the xr stuff..
    public ControllerInputActionManager right_ctrl_input_manager;
    public ControllerInputActionManager left_ctrl_input_manager;
    public TeleportationProvider teleport_provider;
    public DynamicMoveProvider  move_provider;
    

    public float step_sound_dist_threshold = 0.2f;
    private Vector3 prev_pos;
    public float walked_distance = 0.0f;

    private void Awake() {

        Debug.Assert(menu_manager != null);
        Debug.Assert(camera_transform != null);
        Debug.Assert(xr_origin != null);
        Debug.Assert(right_ctrl_input_manager != null);
        Debug.Assert(left_ctrl_input_manager != null);

        xr_origin_spawn_pos = xr_origin.transform.position;
        spawn_cam_pos = camera_transform.transform.position;
        spawn_cam_forward = camera_transform.transform.forward;

        Debug.Assert(menu_input_event != null);
        
        menu_input_event.Reset();

        teleport_provider.locomotionEnded += OnTeleported;

        menu_manager.dev_skip_menu_on_load = DEV_skip_menu_on_load;
        walked_distance = 0.0f;

        prev_pos = camera_transform.position;
        prev_pos.y = 0.0f;
    }

    public void Start() {
        if (DEV_using_device_simulator){
            GameObject.Instantiate(DEV_device_simulator_prefab);
            ApplyHeadHeightOffset(1.73f);
        }
        menu_manager.Init();
    }

    private void Update() {

        if (menu_input_event.Poll(InputPollMode.OnRelease)) {
            
            if (menu_manager.IsMenuOpen()) {
                menu_manager.CloseMenu();
            }
            else {
                menu_manager.OpenMenu();
            }
        }

    }

    private void FixedUpdate(){
        Vector3 pos_now = camera_transform.position;
        pos_now.y = 0.0f; // only consider 2d distance.

        walked_distance += Vector3.Distance(pos_now, prev_pos);

        prev_pos = pos_now;

        if (walked_distance >= step_sound_dist_threshold){

            Vector3 sound_pos = camera_transform.position;
            sound_pos.y -= GetCurrentVirtualHeadHeight();

            SoundManager.instance.PlaySoundAt(SoundID.Step, sound_pos, 0,0, 6.0f);
            walked_distance = 0.0f;
        }
    }
    
    public void EnableInGameInputs() {
        teleport_provider.enabled = true;
        move_provider.enabled = true;
        right_ctrl_input_manager.enabled = true;
        left_ctrl_input_manager.enabled = true;
    }

    public void DisableInGameInputs() {
        teleport_provider.enabled = false;
        move_provider.enabled = false;
        // This feels so much like a hack but i guess it works
        right_ctrl_input_manager.enabled = false;
        left_ctrl_input_manager.enabled = false;
    }
    
    public void ResetTransformsToSpawn() {
        


        //xr_origin.MoveCameraToWorldLocation(spawn_cam_pos);
        xr_origin.transform.position = xr_origin_spawn_pos;
        xr_origin.MatchOriginUpCameraForward(Vector3.up,spawn_cam_forward);
    }
    
    // @Note (Fulcrum):
    // When we press restart in the menu the camera orientation will likely not be the same as the initial spawn orientation
    // which would cause a snap for the menu cubemap since we are resetting the posiotn AND orientation of the player while still in the menu.
    // To avoid this we calculate the angle between those to orientations here so we can pass it to the shader so apply an additional skybox rotation offset so the transition is seamless.
    public float CalculateCameraAngleBetweenForwardNowAndForwardSpawn() {
        Vector3 forward_now = camera_transform.forward;
        forward_now.y = 0.0f;
        forward_now.Normalize();

        Vector3 forward_spawn = spawn_cam_forward;
        forward_spawn.y = 0.0f;
        forward_spawn.Normalize();
        
        float angle = Vector3.SignedAngle( forward_spawn,forward_now, Vector3.up);
        return angle * Mathf.Deg2Rad;
    }

    public void OnTeleported(LocomotionProvider provider) {
        walked_distance = 0.0f;
        Vector3 pos_now =  camera_transform.position;
        pos_now.y = 0.0f;
        prev_pos = pos_now;

        Vector3 sound_pos = camera_transform.position;
        sound_pos.y -= GetCurrentVirtualHeadHeight();

        SoundManager.instance.PlaySoundAt(SoundID.Step, sound_pos, 0.01f, 0.05f);
    }


    public float GetCurrentVirtualHeadHeight() {

        float floor_y = xr_origin.transform.position.y;
        float camera_y = camera_transform.position.y;
        return camera_y - floor_y;
    }

    public void ApplyHeadHeightOffset(float offset) {        

        float offset_to_actually_use = offset;

        float current_height = GetCurrentVirtualHeadHeight();
        float target_height = current_height + offset;

        // if lower than floor clamp it to floor
        if (target_height < 0.02f){
            offset_to_actually_use += Mathf.Abs(target_height) + 0.03f;
        }

        // you are not allowed be be taller than 4 meters.
        if (target_height > 4.0f){
            offset_to_actually_use -= (target_height - 4.0f);
        }


        Vector3 local_pos = camera_offset_transform.localPosition;
        local_pos.y += offset_to_actually_use;
        camera_offset_transform.localPosition = local_pos;
    }
}
