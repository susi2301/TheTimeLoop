using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public enum MenuState {
    Inactive = 0,
    Main,
    MainPause,
    Settings,
    Credits,
}

public class MenuManager : MonoBehaviour {
    [HideInInspector] public GameEventManager game_event_manager;
    public Player player;
    
    public Transform camera_transform;
    public Transform ui_orientation_transform;
    public Canvas canvas;
    public ShaderFloatAnimator vignette_animator;

    public InputEvent menu_reorient_input;

    public float canvas_distance = 1.5f;
    public float canvas_height_offset = 0.0f;
    public float smooth_orient_speed = 3.5f;


    public GameObject back_btn_go;
    
    [Header("State Refs")]
    public MenuState curr_state = MenuState.Inactive;
    public UIState_MainMenu uistate_main_menu;
    public UIState_Settings uistate_settings;
    public UIState_Credits uistate_credits;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        Debug.Assert(camera_transform != null);
        Debug.Assert(vignette_animator != null);

        game_event_manager = GameObject.FindWithTag("GameEventManager").GetComponent<GameEventManager>();
        Debug.Assert(game_event_manager != null);
        
        
        vignette_animator.on_fade_in_value_updated = OnFadeInValueChanged;
        vignette_animator.on_fade_out_value_updated = OnFadeOutValueChanged;
        vignette_animator.event_on_fully_faded_out.AddListener(OnMenuFullyClosed);
        
        vignette_animator.Reset();
        menu_reorient_input.Reset();
        
        // Leave All states!
        uistate_settings.LeaveState();
        uistate_credits.LeaveState();
        uistate_main_menu.LeaveState();
        canvas.gameObject.SetActive(false);
        curr_state = MenuState.Inactive;
        
        ReorientUI();
    }

    void Update() {
        if (curr_state == MenuState.Inactive) {
            return;
        }
        
        if (menu_reorient_input.Poll(InputPollMode.OnRelease)) {
            ReorientUI();
        }
        
        SmoothOrientToPlayer();
    }

    public void OpenMenu(bool first_time_load = false) {
        canvas.gameObject.SetActive(true);
        if (first_time_load) {
            
            vignette_animator.SetFloatManualNow(1.0f);
            canvas.transform.localRotation = quaternion.identity;
            SwitchState(MenuState.Main);
        }
        else {
            Quaternion start_rot = Quaternion.AngleAxis(+95, Vector3.up);
            canvas.transform.localRotation = start_rot;
            vignette_animator.FadeIn();
            SwitchState(MenuState.MainPause);
        }
        
        ReorientUI();
        
        game_event_manager.event_menu_opened.Invoke();
    }

    public void CloseMenu() {
        
        vignette_animator.FadeOut();
    }

    public void OnFadeOutValueChanged(float value_linear, float value_easing) {
        // value linear starts at 1 and goes down to 0.
        // Effectivly we make the entire lerp happen already in the first half of value linear going from 1 to 0.5f
        float x = Mathf.Clamp(value_linear, 0.5f, 1.0f);
        x = (x - 0.5f) * 2.0f; // remap to 0..1 range again;
        x = Mathy.EasingFunctions.ease_float(x, Mathy.EasingFunction.InCubic);
        float lerp_value = x;

        
        Quaternion start_rot = Quaternion.identity;
        Quaternion end_rot = Quaternion.AngleAxis(-95, Vector3.up);
        canvas.transform.localRotation = Quaternion.Slerp( end_rot,start_rot, lerp_value);
    }

    public void OnFadeInValueChanged(float value_linear, float value_easing) {
        // Value Linear start at 0 and goes to 0.5f;
        float x = Mathf.Clamp(value_linear, 0.5f, 1.0f);
        x = (x - 0.5f) * 2.0f; // remap to 0..1 range again;
        x =  Mathy.EasingFunctions.ease_float(x, Mathy.EasingFunction.OutCubic);
        float lerp_value = x;

        Quaternion start_rot = Quaternion.AngleAxis(+95, Vector3.up);
        Quaternion end_rot = Quaternion.identity;
        canvas.transform.localRotation = Quaternion.Slerp(start_rot, end_rot, lerp_value);
    }

    public void OnMenuFullyClosed() {
        // If we call this from outside of SwitchState() we make sure cleanup to first.
        if (curr_state != MenuState.Inactive) {
            LeaveCurrentState();
            curr_state = MenuState.Inactive;
        }
        canvas.gameObject.SetActive(false);
        vignette_animator.JustSetThisValueAndDontAskAnyQuestions(0.0f, "_CubemapRotationOffset");

        game_event_manager.event_menu_closed.Invoke();
    }

    public bool IsMenuOpen() {
        return curr_state != MenuState.Inactive;
    }

    public void SwitchState(MenuState state) {

        if (curr_state == state) {
            return;
        }

        // Leave current state
        LeaveCurrentState();
        
        curr_state = state;
        
        // Enter New State
        switch (state) {
            case MenuState.Main: 
                uistate_main_menu.EnterState();
                back_btn_go.SetActive(false);
                break;
            case MenuState.MainPause: 
                uistate_main_menu.EnterState();
                back_btn_go.SetActive(false);
                break;
            case MenuState.Settings: 
                uistate_settings.EnterState();
                back_btn_go.SetActive(true);
                break;
            case MenuState.Credits: 
                uistate_credits.EnterState();
                back_btn_go.SetActive(true);
                break;
            case MenuState.Inactive:
                back_btn_go.SetActive(false);
                CloseMenu();
                break;
        }
        
    }


    private void LeaveCurrentState() {
        switch (curr_state) {
            case MenuState.Main: uistate_main_menu.LeaveState();
                break;
            case MenuState.MainPause: uistate_main_menu.LeaveState();
                break;
            case MenuState.Settings: 
                uistate_settings.LeaveState();
                break;
            case MenuState.Credits: 
                uistate_credits.LeaveState();
                break;
            case MenuState.Inactive:
                break;
        }
    }
    
    public void OnBackBtnPressed() {
        Debug.Assert(curr_state != MenuState.Inactive && curr_state != MenuState.Main); // back btn shouldnt be pressable!
        // Currently we only have one menu to go back to.
        SwitchState(MenuState.Main);
    }

    public void ReorientUI() {
        Vector3 cam_pos = camera_transform.position;
        Vector3 cam_forward = camera_transform.forward;
        cam_forward.y = 0.0f;
        cam_forward = Vector3.Normalize(cam_forward);
        
        Vector3 new_pos = cam_pos + cam_forward * canvas_distance;
        //@Note (Fulcrum): for some reason the ui canvas is rotate 180 degrees by default ? so we dont invert the camera forward vector!
        Vector3 to_camera = cam_forward; 

        ui_orientation_transform.position = new_pos;
        ui_orientation_transform.rotation = Quaternion.LookRotation(to_camera, Vector3.up);
    }

    public void SmoothOrientToPlayer() {
        Vector3 cam_pos = camera_transform.position;
        Vector3 to_camera = cam_pos - ui_orientation_transform.position;
        to_camera.y = 0.0f;
        to_camera = Vector3.Normalize(to_camera);

        //@Note (Fulcrum): For some reason the ui canvas is rotate 180 degrees by default ? so we invert to_camera vector here.
        to_camera = -to_camera;

        
        Quaternion target_orientation = quaternion.LookRotation(to_camera, Vector3.up);
        Quaternion curr_orientation = ui_orientation_transform.rotation;
        
        float slerp_val = Mathf.Min(smooth_orient_speed * Time.unscaledDeltaTime, 0.95f);
        ui_orientation_transform.rotation = Quaternion.Slerp(curr_orientation, target_orientation, slerp_val);
    }
    
    
    // EDITOR TOOOLS
    [Space] [Header("DEV TOOLS")] 
    public bool dev_reorient_ui = false;

    private void OnValidate() {
        
        if (dev_reorient_ui) {
            ReorientUI();
            dev_reorient_ui = false;
        }
        
    }
}
