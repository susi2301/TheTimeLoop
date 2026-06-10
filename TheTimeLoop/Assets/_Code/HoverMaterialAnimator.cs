using System;
using UnityEngine;
using UnityEngine.Events;


public enum HoverFadeState {
    DoNothing,
    FadeIn,
    FadeOut,
}

public class HoverMaterialAnimator : MonoBehaviour {

    public MeshRenderer mesh_renderer;

    public UnityEvent event_on_faded_in;
    public UnityEvent event_on_faded_out;
    
    public string float_property_name;
    private int float_property_id;

    public float fade_in_duration;
    public float fade_out_duration;

    private HoverFadeState fade_state;
    private float curr_value;
    private float curr_fade_time;
    
    private void Awake() {
        Debug.Assert(mesh_renderer != null);
        float_property_id = Shader.PropertyToID(float_property_name);
        
        Reset();
    }

    public void Reset() {
        curr_value = 0.0f;
        mesh_renderer.material.SetFloat(float_property_id, curr_value);
        fade_state = HoverFadeState.DoNothing;
        curr_fade_time = 0.0f;
    }

    public void FadeIn() {
        curr_fade_time = fade_in_duration * curr_value;
        fade_state = HoverFadeState.FadeIn;
    }

    public void FadeOut() {
        curr_fade_time = fade_out_duration * (1.0f - curr_value);
        fade_state = HoverFadeState.FadeOut;
    }
    
    private void Update() {

        if (fade_state == HoverFadeState.DoNothing) {
            return;
        }

        if (fade_state == HoverFadeState.FadeIn)
        {
            bool finished_fading = curr_fade_time >= fade_in_duration;
            
            float fade_percentage = curr_fade_time / fade_in_duration;
            curr_value = Math.Clamp(fade_percentage, 0.0f, 1.0f);
            mesh_renderer.material.SetFloat(float_property_id, curr_value);
            curr_fade_time += Time.deltaTime;

            if (finished_fading) {
                fade_state = HoverFadeState.DoNothing;
                event_on_faded_in.Invoke();
            }
            
            return;
        }
        
        if (fade_state == HoverFadeState.FadeOut)
        {
            bool finished_fading = curr_fade_time >= fade_out_duration;
            
            float fade_percentage = curr_fade_time / fade_in_duration;
            curr_value = 1.0f - Math.Clamp(fade_percentage, 0.0f, 1.0f);
            mesh_renderer.material.SetFloat(float_property_id, curr_value);
            curr_fade_time += Time.deltaTime;
            
            if (finished_fading) {
                fade_state = HoverFadeState.DoNothing;
                event_on_faded_out.Invoke();
            }
            return;
        }
        
    }
}
