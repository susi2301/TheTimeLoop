using UnityEngine;
using UnityEngine.Events;

public enum ClockAnim {
    WorkingIdle = 0,
    Breaking = 1,
    BrokenIdle = 2,
    LeftRepairing = 3,
    RightRepairing = 4,
    ClockRepairing = 5,
    Count, // Enum Element Count!
}

public class ClockAnimator : MonoBehaviour {

    public Animator animator;
    
    [HideInInspector] public UnityEvent event_break_anim_finished;
    [HideInInspector] public UnityEvent event_left_and_right_repaird_finished;
    
    private int[] hash_keys_enum_array; // Fixed Sized arrays are only allowed in structs..
    
    
    public bool left_repaired_has_finished;
    public bool right_repaired_has_finished;
    
    private void Awake() {
        
        Debug.Assert(animator != null, "ClockAnimator: animator component not assigned");

        hash_keys_enum_array = new int[(int)ClockAnim.Count];
        hash_keys_enum_array[(int)ClockAnim.WorkingIdle]    = Animator.StringToHash("clock_working_idle");
        hash_keys_enum_array[(int)ClockAnim.Breaking]       = Animator.StringToHash("clock_breaking");
        hash_keys_enum_array[(int)ClockAnim.BrokenIdle]     = Animator.StringToHash("clock_broken_idle");
        hash_keys_enum_array[(int)ClockAnim.LeftRepairing]  = Animator.StringToHash("clock_left_repairing");
        hash_keys_enum_array[(int)ClockAnim.RightRepairing] = Animator.StringToHash("clock_right_repairing");
        hash_keys_enum_array[(int)ClockAnim.ClockRepairing] = Animator.StringToHash("clock_repairing");
    }
    
    public void Reset() {
        animator.SetLayerWeight(1, 0.0f);
        animator.SetLayerWeight(2, 0.0f);
        //animator.Play(hash_keys_enum_array[(int)ClockAnim.BrokenIdle], 1);
        //animator.Play(hash_keys_enum_array[(int)ClockAnim.BrokenIdle], 2);
        left_repaired_has_finished  = false;
        right_repaired_has_finished = false;
        animator.ResetControllerState();
        PlayAnim(ClockAnim.WorkingIdle);
    }

    public void PlayAnim(ClockAnim anim, float fade_duration = 0.2f) {
        Debug.Assert(anim != ClockAnim.Count, "ClockAnimator: ClockAnim.Count is not an animation. It is the number of enum elements");
        int anim_hash = hash_keys_enum_array[(int)anim];
        //Debug.Log("Playing Clock Anim: " + anim.ToString() + " with hash: " + anim_hash.ToString());

        if (anim == ClockAnim.LeftRepairing) {
            left_repaired_has_finished = false;
            animator.SetLayerWeight(1, 1.0f);
            animator.Play(anim_hash, 1);
            return;
        } else if (anim == ClockAnim.RightRepairing) {
            right_repaired_has_finished = false;
            animator.SetLayerWeight(2, 1.0f);
            animator.Play(anim_hash, 2);
            return;
        } else if (anim == ClockAnim.ClockRepairing) {
            animator.SetLayerWeight(1, 0.0f);
            animator.SetLayerWeight(2, 0.0f);
            animator.Play(anim_hash);
            return;
        }
        
        animator.CrossFade(anim_hash,fade_duration);
    }
    
    public void OnBreakingAnimFinishedPlay(int _) {
        event_break_anim_finished.Invoke();
    }

    public void OnRepairedSideAnimFinished(int repaired_side) {
        // if repaird_side == 0. its left side
        // if repeird_side == 1. its right side
        
        if (repaired_side == 0) {
            left_repaired_has_finished = true;
            //animator.SetLayerWeight(1, 0.0f);
        } else if (repaired_side == 1) {
            right_repaired_has_finished = true;
            //animator.SetLayerWeight(2, 0.0f);
        }

        if (left_repaired_has_finished && right_repaired_has_finished) {
            event_left_and_right_repaird_finished.Invoke();
        }
    }
}
