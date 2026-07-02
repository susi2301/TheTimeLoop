using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using Flcrm;

public enum MenuState {
    Inactive = 0,
    Main,
    MainPause,
    Settings,
    Credits,
    Controls,
}

public enum HeightAdjustingState {
    IsApplied = 0,
    FadeIn,
    FadeOut,
    Adjusting,
}

public class MenuManager : MonoBehaviour {
    [HideInInspector] public GameEventManager game_event_manager;
    public Player player;
    
    public Transform camera_transform;
    public Transform ui_orientation_transform;
    public Canvas canvas;
    public ShaderFloatAnimator vignette_animator;

    public InputActionReference reset_orientation_action_ref;
    public InputActionReference menu_back_action_ref;
    
    private PollInputEvent reset_orientation_input;
    private PollInputEvent menu_back_input;

    //public InputEvent menu_reorient_input;

    public float canvas_distance = 1.5f;
    public float canvas_height_offset = 0.0f;
    public float smooth_orient_speed = 3.5f;

    public bool dev_skip_menu_on_load = false;
    
    public GameObject back_btn_go;
    
    [Header("State Refs")]
    public MenuState curr_state = MenuState.Inactive;
    public UIState_MainMenu uistate_main_menu;
    public UIState_Settings uistate_settings;
    public UIState_Credits  uistate_credits;
    public UIState_Controls uistate_controls;



    // Height Adjustment State Tracking
    public HeightAdjustingState height_adjust_state = HeightAdjustingState.IsApplied;
    
    public float adjust_fade_in_duration  = 0.5f;
    public float adjust_fade_out_duration = 0.5f;
    public float adjust_fade_out_delay    = 5.0f;

    private int hash_alpha = Shader.PropertyToID("_AlphaMultiply");
    private int hash_cubeBlend = Shader.PropertyToID("_CubemapBlend");


    private float curr_alpha_value;
    private float curr_cube_blend_value;

    private Coroutine fade_in_adjust_state_coroutine;
    private Coroutine fade_out_adjust_state_coroutine;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        Debug.Assert(camera_transform != null);
        Debug.Assert(vignette_animator != null);

        GameObject game_event_manager_go = GameObject.FindWithTag("GameEventManager");
        if (game_event_manager_go == null){
            Debug.LogWarning("Could Not Find GameEventManager in this scene.");
        } else {
            game_event_manager = game_event_manager_go.GetComponent<GameEventManager>();
        }

        
        vignette_animator.on_fade_in_value_updated = OnFadeInValueChanged;
        vignette_animator.on_fade_out_value_updated = OnFadeOutValueChanged;
        vignette_animator.event_on_fully_faded_out.AddListener(OnMenuFullyClosed);
        
        vignette_animator.Reset();
        
        menu_back_input = new PollInputEvent(menu_back_action_ref);
        reset_orientation_input = new PollInputEvent(reset_orientation_action_ref);
        menu_back_input.Reset();
        reset_orientation_input.Reset();

        canvas.gameObject.SetActive(false);
        curr_state = MenuState.Inactive;
    }
    
    public void Init(){

        // Leave All states!
        uistate_settings.ExitState();
        uistate_credits.ExitState();
        uistate_main_menu.ExitState();
        uistate_controls.ExitState();


        ReorientUI();

        if (!dev_skip_menu_on_load) {
            OpenMenu(true);
        } else{
            OnMenuFullyClosed();
        }

        StartCoroutine(ReorientUIDelayed(2.5f));
    }


    public IEnumerator ReorientUIDelayed(float delay){
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        ReorientUI();

        yield return new WaitForSeconds(delay);
        ReorientUI();
    }


    void Update() {
        if (curr_state == MenuState.Inactive) {
            return;
        }

        if (menu_back_input.Poll(InputPollMode.OnRelease)){
            OnBackBtnPressed();
        }
        
        if (reset_orientation_input.Poll(InputPollMode.OnRelease)) {
            ReorientUI();
        }
        
        SmoothOrientToPlayer();

        if (curr_state == MenuState.Settings){
            uistate_settings.UpdateState();
        }
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

        HeightAdjustStateResetNow();

        ReorientUI();
        
        if(game_event_manager != null) {
            game_event_manager.event_menu_opened.Invoke();
        }
    }

    public void CloseMenu() {
        if (height_adjust_state == HeightAdjustingState.FadeIn){
            StopCoroutine(fade_in_adjust_state_coroutine);
        }
        if (height_adjust_state == HeightAdjustingState.FadeOut){
            StopCoroutine(fade_out_adjust_state_coroutine);
        }

        vignette_animator.FadeOut();
    }

    public void OnFadeOutValueChanged(float value_linear, float value_easing) {
        // value linear starts at 1 and goes down to 0.
        // Effectivly we make the entire lerp happen already in the first half of value linear going from 1 to 0.5f
        float x = Mathf.Clamp(value_linear, 0.5f, 1.0f);
        x = (x - 0.5f) * 2.0f; // remap to 0..1 range again;
        x = Mathy.ease_float(x, EasingFunction.InCubic);
        float lerp_value = x;

        
        Quaternion start_rot = Quaternion.identity;
        Quaternion end_rot = Quaternion.AngleAxis(-95, Vector3.up);
        canvas.transform.localRotation = Quaternion.Slerp( end_rot,start_rot, lerp_value);
    }

    public void OnFadeInValueChanged(float value_linear, float value_easing) {
        // Value Linear start at 0 and goes to 0.5f;
        float x = Mathf.Clamp(value_linear, 0.5f, 1.0f);
        x = (x - 0.5f) * 2.0f; // remap to 0..1 range again;
        x =  Mathy.ease_float(x, EasingFunction.OutCubic);
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

        if (game_event_manager != null){
            
            game_event_manager.event_menu_closed.Invoke();
        }
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
        
        bool show_back_button = true;
        
        // Enter New State
        switch (state) {
            case MenuState.Main: 
                uistate_main_menu.EnterState();
                show_back_button = false;
                break;
            case MenuState.MainPause: 
                uistate_main_menu.EnterState();
                show_back_button = false;
                break;
            case MenuState.Settings: 
                uistate_settings.EnterState();
                break;
            case MenuState.Credits: 
                uistate_credits.EnterState();
                break;
            case MenuState.Controls: 
                uistate_controls.EnterState();
                break;
            case MenuState.Inactive:
                show_back_button = false;
                CloseMenu();
                break;
        }
        
        back_btn_go.SetActive(show_back_button);
    }


    private void LeaveCurrentState() {
        
        switch (curr_state) {
            case MenuState.Main: uistate_main_menu.ExitState();
                break;
            case MenuState.MainPause: uistate_main_menu.ExitState();
                break;
            case MenuState.Settings: 
                uistate_settings.ExitState();
                break;
            case MenuState.Credits: 
                uistate_credits.ExitState();
                break;
            case MenuState.Controls: 
                uistate_controls.ExitState();
                break;
            case MenuState.Inactive:
                break;
        }
    }
    
    public void OnBackBtnPressed() {

        Debug.Assert(curr_state != MenuState.Inactive); // back btn shouldnt be pressable!
        
        if (curr_state == MenuState.Main || curr_state == MenuState.MainPause) {
            CloseMenu();
            return;
        }

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


    private void HeightAdjustStateResetNow(){

        if (height_adjust_state == HeightAdjustingState.FadeIn){
            StopCoroutine(fade_in_adjust_state_coroutine);
        }

        if (height_adjust_state == HeightAdjustingState.FadeOut){
            StopCoroutine(fade_out_adjust_state_coroutine);
        }

        height_adjust_state = HeightAdjustingState.IsApplied;


        vignette_animator.JustSetThisValueAndDontAskAnyQuestions(1.0f, hash_alpha);
        vignette_animator.JustSetThisValueAndDontAskAnyQuestions(1.0f, hash_cubeBlend);

        curr_alpha_value = 1.0f;
        curr_cube_blend_value = 1.0f;
    }

    public void EnterHeightAdjustingState(){
            
        if (height_adjust_state == HeightAdjustingState.FadeIn){
            return;
        }

        if (height_adjust_state == HeightAdjustingState.FadeOut) {
            StopCoroutine(fade_out_adjust_state_coroutine);
        }

        height_adjust_state = HeightAdjustingState.FadeIn;

        fade_in_adjust_state_coroutine = StartCoroutine(FadeIntoAdjustingState());
    }

    public void ExitHeightAdjustingState(){
        
        if (height_adjust_state == HeightAdjustingState.FadeOut){
            return;
        }

        if (height_adjust_state == HeightAdjustingState.FadeIn) {
            StopCoroutine(fade_in_adjust_state_coroutine);
        }

        height_adjust_state = HeightAdjustingState.FadeOut;
        fade_out_adjust_state_coroutine = StartCoroutine(FadeOutAdjustingState());
    }


    private IEnumerator FadeIntoAdjustingState() {

        // TODO: what happens if we are currently fading out!

        
        float start_feather = vignette_animator.curr_linear_value;
        float start_alpha = curr_alpha_value; 
        float start_blend = curr_cube_blend_value; 

        float target_alpha = 0.5f;
        float target_blend = 0.0f;

        float time_accum = 0.0f;

        while (time_accum < adjust_fade_in_duration){

            float lerp_val = Mathf.Clamp(time_accum, 0.0f, adjust_fade_in_duration) / adjust_fade_in_duration;
            lerp_val = Mathy.ease_float(lerp_val, EasingFunction.InQuadratic);

            float alpha = Mathf.Lerp(start_alpha, target_alpha, lerp_val);
            float blend = Mathf.Lerp(start_blend, target_blend, lerp_val);
            vignette_animator.JustSetThisValueAndDontAskAnyQuestions(alpha, hash_alpha);
            vignette_animator.JustSetThisValueAndDontAskAnyQuestions(blend, hash_cubeBlend);

            curr_alpha_value = alpha;
            curr_cube_blend_value = blend;

            time_accum  += Time.deltaTime;
            yield return null;
        }

        vignette_animator.JustSetThisValueAndDontAskAnyQuestions(target_alpha, hash_alpha);
        vignette_animator.JustSetThisValueAndDontAskAnyQuestions(target_blend, hash_cubeBlend);
        curr_alpha_value = target_alpha;
        curr_cube_blend_value = target_blend;

        height_adjust_state = HeightAdjustingState.Adjusting;
    }

    private IEnumerator FadeOutAdjustingState() {
        
        yield return new WaitForSeconds(adjust_fade_out_delay);

        float start_alpha = curr_alpha_value; 
        float start_blend = curr_cube_blend_value; 

        float target_alpha = 1.0f;
        float target_blend = 1.0f;

        float time_accum = 0.0f;

        while (time_accum < adjust_fade_out_duration){

            float lerp_val = Mathf.Clamp(time_accum, 0.0f, adjust_fade_out_duration) / adjust_fade_out_duration;
            lerp_val = Mathy.ease_float(lerp_val, EasingFunction.InQuadratic);

            float alpha = Mathf.Lerp(start_alpha, target_alpha, lerp_val);
            float blend = Mathf.Lerp(start_blend, target_blend, lerp_val);

            vignette_animator.JustSetThisValueAndDontAskAnyQuestions(alpha, hash_alpha);
            vignette_animator.JustSetThisValueAndDontAskAnyQuestions(blend, hash_cubeBlend);

            curr_alpha_value = alpha;
            curr_cube_blend_value = blend;

            time_accum  += Time.deltaTime;
            yield return null;
        }


        vignette_animator.JustSetThisValueAndDontAskAnyQuestions(target_alpha, hash_alpha);
        vignette_animator.JustSetThisValueAndDontAskAnyQuestions(target_blend, hash_cubeBlend);
        
        curr_alpha_value = target_alpha;
        curr_cube_blend_value = target_blend;

        height_adjust_state = HeightAdjustingState.IsApplied;
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
