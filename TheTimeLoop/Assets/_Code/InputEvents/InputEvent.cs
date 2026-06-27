using UnityEngine;
using UnityEngine.InputSystem;


public enum InputPollMode {
    OnPressed, // Assuming underlying input is a binary button state, aka value is 0 or 1.
    OnRelease, // Assuming underlying input is a binary button state, aka value is 0 or 1.
    OnChanged,
}

public enum PollEvent {
    Nothing,
    WasPressed,
    WasReleased,
}


public class PollInputEvent {
    public InputActionReference input_action;
    private float last_value;

    public PollInputEvent(InputActionReference action_ref) {
        input_action = action_ref;
        last_value = 0;
    }
    
    public void Reset() {
        last_value = 0.0f;
    }

    
    public PollEvent PollBtnWhatHappened(float press_threshold = 0.2f) {

        Debug.Assert(input_action != null, "InputEvent: Trying to pool an input event where the InputRefence is NULL");

        float val = input_action.action.ReadValue<float>();

        PollEvent poll_event = PollEvent.Nothing;
        
        if (val > press_threshold && last_value < press_threshold) {
            poll_event = PollEvent.WasPressed;
        }
        
        if (last_value > press_threshold && val < press_threshold) {
            poll_event = PollEvent.WasReleased;
        }
        
        last_value = val;
        
        return poll_event;
    }
    
    public PollEvent PollAnalog2DAxisWhatHappened(out Vector2 out_axis_value, float press_threshold = 0.1f) {

        Debug.Assert(input_action != null, "InputEvent: Trying to pool an input event where the InputRefence is NULL");

        Vector2 axis = input_action.action.ReadValue<Vector2>();
        float val = axis.magnitude;
        
        PollEvent poll_event = PollEvent.Nothing;
        
        if (val >= press_threshold && last_value < press_threshold) {
            poll_event = PollEvent.WasPressed;
        }
        
        if (last_value >= press_threshold && val < press_threshold) {
            poll_event = PollEvent.WasReleased;
        }
        
        last_value = val;

        out_axis_value = axis;
        return poll_event;
    }
}


[CreateAssetMenu(fileName = "InputEvent", menuName = "Scriptable Objects/InputEvent")]
public class InputEvent : ScriptableObject {
    public InputActionReference input_action;
    private float last_value;
    public float press_threshold = 0.2f;

    public InputEvent(InputActionReference action_ref) {
        input_action = action_ref;
        last_value = 0;
    }
    
    public void Reset() {
        last_value = 0.0f;
    }
    
    // Poll the underlying input and return true if the InputPollMode is satisfied.
    public bool Poll(InputPollMode mode) {
        
        Debug.Assert(input_action != null, "InputEvent: Trying to pool an input event where the InputRefence is NULL");

        float val = input_action.action.ReadValue<float>();

        bool poll_event_happend = false;
        

        switch (mode) {
            case InputPollMode.OnPressed:
                if (val > press_threshold && last_value < press_threshold) {
                    poll_event_happend = true;
                }
                break;
            case InputPollMode.OnRelease:
                if (last_value > press_threshold && val < press_threshold) {
                    poll_event_happend = true;
                }
                break;
            case InputPollMode.OnChanged:
                if (val != last_value) {
                    poll_event_happend = true;
                }
                break;
        }
 
        
        last_value = val;
        return poll_event_happend;
    }
    
    public PollEvent PollBtnWhatHappened() {

        Debug.Assert(input_action != null, "InputEvent: Trying to pool an input event where the InputRefence is NULL");

        float val = input_action.action.ReadValue<float>();

        PollEvent poll_event = PollEvent.Nothing;
        
        if (val > press_threshold && last_value < press_threshold) {
            poll_event = PollEvent.WasPressed;
        }
        
        if (last_value > press_threshold && val < press_threshold) {
            poll_event = PollEvent.WasReleased;
        }
        
        last_value = val;
        
        return poll_event;
    }
    
    // Must be called after Calling Poll
    public float GetValue() {
        return last_value;
    }
}
