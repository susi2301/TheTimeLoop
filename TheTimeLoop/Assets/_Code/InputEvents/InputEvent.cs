using UnityEngine;
using UnityEngine.InputSystem;


public enum InputPollMode {
    OnPressed, // Assuming underlying input is a binary button state, aka value is 0 or 1.
    OnRelease, // Assuming underlying input is a binary button state, aka value is 0 or 1.
    OnChanged,
}


[CreateAssetMenu(fileName = "InputEvent", menuName = "Scriptable Objects/InputEvent")]
public class InputEvent : ScriptableObject {
    public InputActionReference input_action;
    private float last_value;

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
                if (val > 0.8f && last_value < 0.2f) {
                    poll_event_happend = true;
                }
                break;
            case InputPollMode.OnRelease:
                if (last_value > 0.8 && val < 0.2f) {
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
    
    // Must be called after Calling Poll
    public float GetValue() {
        return last_value;
    }
}
