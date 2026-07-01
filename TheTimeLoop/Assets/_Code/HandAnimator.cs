using System;
using Flcrm;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;


public enum HandAnimLayer {
    Idle = 0,
    Grab = 1,
    Trigger = 2,
    Primary = 3,
    Secondary = 4,
    Home = 5,
    Axis = 6,
    Count,
}


public class HandAnimator : MonoBehaviour {

    public Animator animator;
    public GameObject controller_go;
    
    public InputActionReference grab_action_ref;
    public InputActionReference trigger_action_ref;
    public InputActionReference primary_action_ref;
    public InputActionReference secondary_action_ref;
    public InputActionReference home_action_ref;
    public InputActionReference axis_action_ref;

    public float fade_in_duration = 0.25f;
    public float fade_out_duration = 0.25f;
    public EasingFunction fade_in_easing_function = EasingFunction.Linear;
    public EasingFunction fade_out_easing_function = EasingFunction.Linear;

    private PollInputEvent[] input_events = new PollInputEvent[(int)HandAnimLayer.Count];
    private float[] curr_layer_weight = new float[(int)HandAnimLayer.Count];
    private float[] curr_layer_fade_time = new float[(int)HandAnimLayer.Count];
    private FadeState[] layer_fade_sate = new FadeState[(int)HandAnimLayer.Count];

    private Vector2 polled_axis_value;
    
    private Holdable curr_holding = Holdable.None;

    private int anim_idle       = Animator.StringToHash("right_idle");
    private int anim_tochgrab   = Animator.StringToHash("right_torchgrab");
    private int anim_lightgrab  = Animator.StringToHash("right_flashlightgrab");
    private int anim_weightgrab = Animator.StringToHash("right_weightgrab");

    private void Awake() {

        Debug.Assert(animator != null, "HandAnimator: animator not assigned");
        
        for (int layer_index = 0; layer_index < (int)HandAnimLayer.Count; layer_index++) {
            HandAnimLayer anim_layer = (HandAnimLayer)layer_index;
            if (anim_layer == HandAnimLayer.Idle) {
                SetLayerWeight(anim_layer, 1.0f);
            }
            else {
                SetLayerWeight(anim_layer, 0.0f);
            }
            curr_layer_fade_time[layer_index] = 0.0f;
            layer_fade_sate[layer_index] = FadeState.DoNothing;
        }

        input_events[(int)HandAnimLayer.Idle]      = null;
        input_events[(int)HandAnimLayer.Grab]      = new PollInputEvent(grab_action_ref);
        input_events[(int)HandAnimLayer.Trigger]   = new PollInputEvent(trigger_action_ref);
        input_events[(int)HandAnimLayer.Primary]   = new PollInputEvent(primary_action_ref);
        input_events[(int)HandAnimLayer.Secondary] = new PollInputEvent(secondary_action_ref);
        input_events[(int)HandAnimLayer.Home]      = new PollInputEvent(home_action_ref);
        input_events[(int)HandAnimLayer.Axis]      = new PollInputEvent(axis_action_ref);
        
    }

    public void HardReset() {
        controller_go.SetActive(true);
        animator.CrossFade(anim_idle, 0.25f, (int)HandAnimLayer.Idle);
        curr_holding = Holdable.None;
    }

    private void Update() {
        
        for (int layer_index = 1; layer_index < (int)HandAnimLayer.Count; layer_index++) {

            HandAnimLayer layer = (HandAnimLayer)layer_index;            
            
            if (layer == HandAnimLayer.Axis) {
                
                PollEvent analog_poll_event = input_events[layer_index].PollAnalog2DAxisWhatHappened(out Vector2 analog_axis, 0.1f);

                polled_axis_value = analog_axis;
                
                if (analog_poll_event == PollEvent.WasPressed) {
                    layer_fade_sate[layer_index] = FadeState.FadeIn;
                    curr_layer_fade_time[layer_index] = fade_in_duration * curr_layer_weight[layer_index];
                
                } else if (analog_poll_event == PollEvent.WasReleased) {
                    layer_fade_sate[layer_index] = FadeState.FadeOut;
                    curr_layer_fade_time[layer_index] = fade_out_duration * (1.0f - curr_layer_weight[layer_index]);
                }
                
                continue;
            }
            
            
            PollEvent poll_event = input_events[layer_index].PollBtnWhatHappened();
            
            if (poll_event == PollEvent.WasPressed) {
                layer_fade_sate[layer_index] = FadeState.FadeIn;
                curr_layer_fade_time[layer_index] = fade_in_duration * curr_layer_weight[layer_index];
                
            } else if (poll_event == PollEvent.WasReleased) {
                layer_fade_sate[layer_index] = FadeState.FadeOut;
                curr_layer_fade_time[layer_index] = fade_out_duration * (1.0f - curr_layer_weight[layer_index]);
            }               
        }


        if (curr_holding != Holdable.None){

             for (int layer_index = 1; layer_index < (int)HandAnimLayer.Count; layer_index++){
                


                HandAnimLayer layer = (HandAnimLayer)layer_index;
                FadeState fade_state = layer_fade_sate[layer_index];

                if (curr_holding == Holdable.Flashlight && layer == HandAnimLayer.Primary){
                    continue;
                }

                bool force_fade_out_layer = fade_state == FadeState.FadeIn || (fade_state == FadeState.DoNothing && curr_layer_weight[layer_index] > 0.0f);

                if (force_fade_out_layer) {
                    layer_fade_sate[layer_index] = FadeState.FadeOut;
                    curr_layer_fade_time[layer_index] = fade_out_duration * (1.0f - curr_layer_weight[layer_index]);
                }
             }
        }



        // update fading
        for (int layer_index = 1; layer_index < (int)HandAnimLayer.Count; layer_index++) {
            
            HandAnimLayer layer = (HandAnimLayer)layer_index;

            FadeState fade_state = layer_fade_sate[layer_index];

            if (fade_state == FadeState.FadeIn) {
                
                bool finished_fading = curr_layer_fade_time[layer_index] >= fade_in_duration;
            
                float fade_percentage = curr_layer_fade_time[layer_index] / fade_in_duration;
                
                float curr_linear_value = Math.Clamp(fade_percentage, 0.0f, 1.0f);
                float value_easing_applied = Mathy.ease_float(curr_linear_value, fade_in_easing_function);
                
                SetLayerWeight(layer, value_easing_applied);
                curr_layer_fade_time[layer_index] += Time.deltaTime;
                

                if (finished_fading) {
                    layer_fade_sate[layer_index] = FadeState.DoNothing;
                    SetLayerWeight(layer, 1.0f);
                }
            }

            if (fade_state == FadeState.FadeOut) {
                
                bool finished_fading = curr_layer_fade_time[layer_index] >= fade_out_duration;
                float fade_percentage = curr_layer_fade_time[layer_index] / fade_out_duration;
                float curr_linear_value = 1.0f - Math.Clamp(fade_percentage, 0.0f, 1.0f);
                float value_easing_applied = Mathy.ease_float(curr_linear_value, fade_out_easing_function);
                SetLayerWeight(layer, value_easing_applied);
                
                curr_layer_fade_time[layer_index] += Time.deltaTime;

                if (finished_fading) {
                    layer_fade_sate[layer_index] = FadeState.DoNothing;
                    SetLayerWeight(layer, 0.0f);
                }
            }
            
            if (layer == HandAnimLayer.Axis) {
                if (curr_layer_weight[layer_index] > 0.0f) {
                    
                    animator.SetFloat("axis_x", polled_axis_value.x);
                    animator.SetFloat("axis_y", polled_axis_value.y);
                }
                continue;
            }
        }
    }

    public void OnGrabbedHoldable(Holdable holdable) {

        Debug.Assert(holdable != Holdable.None);

        controller_go.SetActive(false);

        float fade_dur = 0.05f;

        if (holdable == Holdable.Torch) {
            animator.CrossFade(anim_tochgrab, fade_dur, (int)HandAnimLayer.Idle);
        } else if (holdable == Holdable.Flashlight) {
            animator.CrossFade(anim_lightgrab, fade_dur, (int)HandAnimLayer.Idle);
        } else if (holdable == Holdable.ClockWeight) {
            animator.CrossFade(anim_weightgrab, fade_dur, (int)HandAnimLayer.Idle);
        }

        curr_holding = holdable;
    }

    public void OnDroppedHoldable(Holdable holdable) {
        controller_go.SetActive(true);

        curr_holding = Holdable.None;

        animator.CrossFade(anim_idle, 0.05f, (int)HandAnimLayer.Idle);
    }
    
    
    private void SetLayerWeight(HandAnimLayer layer, float weight) {
        int index = (int)layer;
        curr_layer_weight[index] = weight;
        animator.SetLayerWeight(index, weight);
    }
    
}
