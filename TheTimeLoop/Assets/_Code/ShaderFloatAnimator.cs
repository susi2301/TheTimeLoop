using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Flcrm;

// @Note (fulcrum):
// Use this to animate a float property of a shader between 0..1 range
// Call FadeIn to animate the value up to 1.
// Call FadeOut to animate the value down to 0.
// Call Reset to reset the value to 0 which corresponds to a fully faded out state.
// If you call FadeIn while its currently already FadingOut. It will fade in from its current value which 
// means it will take less time than specified in fade_in_duration.

public enum FadeState {
    DoNothing,
    FadeIn,
    FadeOut,
}

public class ShaderFloatAnimator : MonoBehaviour {

    public MeshRenderer mesh_renderer;
    
    public string float_property_name;

    public EasingFunction fade_in_easing_function = EasingFunction.Linear;
    public float fade_in_duration;
    public EasingFunction fade_out_easing_function = EasingFunction.Linear;
    public float fade_out_duration;
    
    public bool deactive_meshrenderer_when_fully_faded_out = false;
    
    [Header("Callbacks")]
    public UnityEvent event_on_fully_faded_in;
    public UnityEvent event_on_fully_faded_out;

    
    private int float_property_id;

    private FadeState fade_state;
    private float curr_linear_value;
    private float curr_fade_time;

    // callbacks for when value get updated.
    // first float is linear value, second float is with easing function applied.
    public Action<float,float> on_fade_in_value_updated  = null;
    public Action<float,float> on_fade_out_value_updated = null;
    
    private void Awake() {
        Debug.Assert(mesh_renderer != null, "ShaderFloatAnimator: MeshRenderer not Assigned");
        float_property_id = Shader.PropertyToID(float_property_name);
        
        Reset();
    }

    public void Reset() {
        curr_linear_value = 0.0f;
        mesh_renderer.material.SetFloat(float_property_id, curr_linear_value);
        fade_state = FadeState.DoNothing;
        curr_fade_time = 0.0f;
        if (deactive_meshrenderer_when_fully_faded_out) {
            mesh_renderer.enabled = false;
        }
    }

    public void SetNewPropertyName(string property_name) {
        float_property_name = property_name;
        float_property_id = Shader.PropertyToID(property_name);
    }

    public void FadeIn() {
        if (deactive_meshrenderer_when_fully_faded_out) {
            if (!mesh_renderer.enabled) {
                mesh_renderer.enabled = true;
            }
        }
        curr_fade_time = fade_in_duration * curr_linear_value;
        fade_state = FadeState.FadeIn;
    }

    public void FadeOut() {
        curr_fade_time = fade_out_duration * (1.0f - curr_linear_value);
        fade_state = FadeState.FadeOut;
    }

    public void SetFloatManualNow(float value_0_to_1) {
        float x = Mathf.Clamp(value_0_to_1, 0.0f, 1.0f);
        curr_linear_value = x;
        mesh_renderer.material.SetFloat(float_property_id, curr_linear_value);
        
        if (deactive_meshrenderer_when_fully_faded_out) {
            if (curr_linear_value == 0.0f) {
                mesh_renderer.enabled = false;
            } else if (curr_linear_value == 1.0f) {
                mesh_renderer.enabled = true;
            }
        }
    }

    public void JustSetThisValueAndDontAskAnyQuestions(float value, string property_name) {
        mesh_renderer.material.SetFloat(property_name, value);
    }
    
    private void Update() {

        if (fade_state == FadeState.DoNothing) {
            return;
        }

        if (fade_state == FadeState.FadeIn)
        {
            bool finished_fading = curr_fade_time >= fade_in_duration;
            
            float fade_percentage = curr_fade_time / fade_in_duration;
            curr_linear_value = Math.Clamp(fade_percentage, 0.0f, 1.0f);
            float value_easing_applied = Mathy.ease_float(curr_linear_value, fade_in_easing_function);
            mesh_renderer.material.SetFloat(float_property_id, value_easing_applied);
            curr_fade_time += Time.deltaTime;

            if (on_fade_in_value_updated != null) {
                on_fade_in_value_updated.Invoke(curr_linear_value,value_easing_applied);
            }
            
            if (finished_fading) {
                fade_state = FadeState.DoNothing;
                event_on_fully_faded_in.Invoke();
            }
            
            return;
        }
        
        if (fade_state == FadeState.FadeOut)
        {
            bool finished_fading = curr_fade_time >= fade_out_duration;
            
            float fade_percentage = curr_fade_time / fade_out_duration;
            curr_linear_value = 1.0f - Math.Clamp(fade_percentage, 0.0f, 1.0f);
            float value_easing_applied = Mathy.ease_float(curr_linear_value, fade_out_easing_function);
            mesh_renderer.material.SetFloat(float_property_id, value_easing_applied);
            curr_fade_time += Time.deltaTime;

            if (on_fade_out_value_updated != null) {
                on_fade_out_value_updated.Invoke(curr_linear_value,value_easing_applied);
            }
            
            if (finished_fading) {
                fade_state = FadeState.DoNothing;
                event_on_fully_faded_out.Invoke();
                if (deactive_meshrenderer_when_fully_faded_out) {
                    mesh_renderer.enabled = false;
                }
            }
            return;
        }
        
    }
}