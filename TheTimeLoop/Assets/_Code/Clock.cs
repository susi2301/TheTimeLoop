using System;
using Unity.Android.Gradle;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public enum ClockState {
    NeverOpenedIsWorking = 0,
    WasOpenedIsBroken,
    WasOpenedIsWorking,
}


public class Clock : MonoBehaviour {
    public ClockState state;

    [HideInInspector] public bool left_socket_is_attached;
    [HideInInspector] public bool right_socket_is_attached;
    
    public XRSocketInteractor left_socket_ptr;
    public XRSocketInteractor right_socket_ptr;
    
    public Animator animator;
    public ClockAnimEvents anim_events;
    
    public MeshRenderer left_weight_meshren;
    public MeshRenderer right_weight_meshren;

    public GameObject door_gameobject;

    public UnityEvent event_on_clock_broken;
    public UnityEvent event_on_clock_fixed;
    
    private void Awake() {
        Debug.Assert(left_socket_ptr != null, "CLOCK: left socket is not assigned!");       
        Debug.Assert(right_socket_ptr != null, "CLOCK: right socket is not assigned!" );
        Debug.Assert(left_weight_meshren != null, "CLOCK: left weight meshren is not assigned!" );
        Debug.Assert(right_weight_meshren != null, "CLOCK: right weight meshren is not assigned!" );
        Debug.Assert(animator != null, "CLOCK: animator not assigned");
        Debug.Assert(anim_events != null, "CLOCK: anim events not assigned");
        
        anim_events.event_break_anim_finished.AddListener(OnBreakAnimFinishedPlay);
        
        HardReset();
    }
    
    public void HardReset() {
        if (door_gameobject != null) {
            door_gameobject.SetActive(true);
        }
        
        // TODO: make sure actual door physics mesh is also visually closed!
        door_gameobject.transform.rotation = quaternion.identity;
        
        // reset anim to idle
        animator.CrossFade("clock_strings_working_idle", 0.2f);
        
        state = ClockState.NeverOpenedIsWorking;
        left_socket_is_attached = true;
        right_socket_is_attached = true;
        
        left_socket_ptr.socketActive  = false;
        right_socket_ptr.socketActive = false;

        left_weight_meshren.enabled = true;
        right_weight_meshren.enabled = true;
        
    }
    
    
    // GAME END CONDITION!
    public void WeightGotAttached() {
        
        if (!left_socket_is_attached || !right_socket_is_attached) {
            return;
        }
        
        Debug.Log("CLOCK: Clock is repaired!");
        state = ClockState.WasOpenedIsWorking;
        
        // TODO: deactivating socket causes the weight to to be dropped!
        // But we dont want player to be able to remove it again
        //left_socket_ptr.socketActive = false;
        //right_socket_ptr.socketActive = false;
        
        // TODO: Reset Everything!
        event_on_clock_fixed.Invoke();
        
    }
    

    // Event Callbacks
    public void OnDoorGrabbedAndReleased() {

        // TODO: trigger some animation and transform the world. Clock is now brocken
        // TODO: it is possible that the door is not fully opened or was snapped back to closed state. We must handle this. Maybe Force open the door first one first time ?
        if (state == ClockState.NeverOpenedIsWorking) {
            Debug.Log("CLOCK: FirstTimeOpen! Clock is now BROKEN!");
            state = ClockState.WasOpenedIsBroken;
            left_socket_is_attached = false;
            right_socket_is_attached = false;
            
            left_socket_ptr.socketActive = true;
            right_socket_ptr.socketActive = true;
            
            //break_timeline_director.Play();
            animator.CrossFade("clock_strings_breaking_anim",0.2f);
        }
    }
    
    public void OnBreakAnimFinishedPlay() {
        Debug.Log("CLOCK: BreakTimeline FNISHED");
        right_weight_meshren.enabled = false;
        left_weight_meshren.enabled = false;
        
        event_on_clock_broken.Invoke();
    }
    public void OnBreakTimelineFinishedPlay() {
        Debug.Log("CLOCK: BreakTimeline FNISHED");
        right_weight_meshren.enabled = false;
        left_weight_meshren.enabled = false;
        
        event_on_clock_broken.Invoke();
    }
    
    public void OnWeightAttachedToLeftSocket() {
        left_socket_is_attached = true;
        Debug.Log("CLOCK: Attached weight to left socket");
        WeightGotAttached();    
    }

    public void OnWeightDetachedFromLeftSocket() {
        left_socket_is_attached = false;
        Debug.Log("CLOCK: Detached weight from left socket");
    }
    
    public void OnWeightAttachedToRightSocket() {
        right_socket_is_attached = true;
        Debug.Log("CLOCK: Attached weight to right socket");
        WeightGotAttached();
    }

    public void OnWeightDetachedFromRightSocket() {
        right_socket_is_attached = false;
        Debug.Log("CLOCK: Detached weight from right socket");
    }
}
