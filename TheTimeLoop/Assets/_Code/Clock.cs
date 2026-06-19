using System;
using Unity.Android.Gradle;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public enum ClockState {
    NeverOpenedIsWorking = 0,
    IsBreaking,
    WasOpenedIsBroken,
    WasOpenedIsWorking,
}

public enum DoorState {
    None = 0,
    ForceOpen,
    ForceClose,
}

public class Clock : MonoBehaviour {
    
    private GameEventManager game_event_manager;
    
    public ClockState state;

    [HideInInspector] public bool left_socket_is_attached;
    [HideInInspector] public bool right_socket_is_attached;
    
    public XRSocketInteractor left_socket_ptr;
    public XRSocketInteractor right_socket_ptr;
    
    public ClockAnimator clock_animator;
    
    public GameObject door_gameobject;
    public XRGrabInteractable door_grab_interactable;
    private Rigidbody door_rigidbody;
    private DoorState door_state;
    
    public WeightDissolve right_weight_dissolver;
    public WeightDissolve left_weight_dissolver;
    private void Awake() {
        
        game_event_manager = GameObject.FindWithTag("GameEventManager").GetComponent<GameEventManager>();
        
        Debug.Assert(left_socket_ptr != null);       
        Debug.Assert(right_socket_ptr != null);
        Debug.Assert(right_weight_dissolver != null);
        Debug.Assert(left_weight_dissolver != null);
        Debug.Assert(clock_animator != null);
        Debug.Assert(door_gameobject != null);
        Debug.Assert(door_grab_interactable != null);
        
        door_rigidbody = door_gameobject.GetComponent<Rigidbody>();
        Debug.Assert(door_rigidbody != null);
        
        clock_animator.event_break_anim_finished.AddListener(OnBreakAnimFinishedPlay);
    }
    
    public void HardReset() { // Called by game manager!
        
        Debug.Log("Clock Hard Reset");
        // Make sure actual door physics mesh is also visually closed!
        door_gameobject.SetActive(true);
        door_gameobject.transform.localRotation = Quaternion.identity;
        door_rigidbody.angularVelocity = Vector3.zero;
        door_rigidbody.linearVelocity = Vector3.zero;
        door_state = DoorState.None;
        
        clock_animator.Reset();
        
        state = ClockState.NeverOpenedIsWorking;
        left_socket_is_attached = true;
        right_socket_is_attached = true;
        
        left_socket_ptr.enabled = false;
        right_socket_ptr.enabled = false;
        
        left_weight_dissolver.Reset();
        right_weight_dissolver.Reset();
    }
    
    
    // GAME END CONDITION!
    public void WeightGotAttached(GameObject grabbable_weight_go) {

        ClockWeight weight = grabbable_weight_go.GetComponent<ClockWeight>();
        Debug.Assert(weight != null);
        weight.Despawn();


        if (!left_socket_is_attached || !right_socket_is_attached) {
            return;
        }
        
        Debug.Log("CLOCK: Clock is repaired!");
        state = ClockState.WasOpenedIsWorking;

        // TODO: deactivating socket causes the weight to to be dropped!
        // But we dont want player to be able to remove it again
        //left_socket_ptr.socketActive = false;
        //right_socket_ptr.socketActive = false;
    
        SetDoorState(DoorState.ForceClose);
        //event_on_clock_fixed.Invoke();
    }

    private void SetDoorState(DoorState new_door_state) {

        if (new_door_state == this.door_state) {
            return;
        }

        if (new_door_state == DoorState.None) {
            this.door_state = DoorState.None;
    
            door_rigidbody.isKinematic = false;
            door_rigidbody.angularVelocity = Vector3.zero;
            door_rigidbody.linearVelocity = Vector3.zero;
            door_rigidbody.rotation = door_gameobject.transform.rotation;
            door_rigidbody.WakeUp();
            return;
        }
        
        door_rigidbody.angularVelocity = Vector3.zero;
        door_rigidbody.linearVelocity = Vector3.zero;
        door_rigidbody.Sleep();
        door_rigidbody.isKinematic = true;
        
        this.door_state = new_door_state;
    }
    
    public void FixedUpdate() {


        switch (door_state) {
            case DoorState.None: return;
            case DoorState.ForceOpen:
            {
                Quaternion target_rot = Quaternion.AngleAxis(-140, Vector3.up);
                Quaternion curr_rot = door_gameobject.transform.localRotation;
                
                float angle_between = Quaternion.Angle(curr_rot, target_rot);

                if (angle_between > 5.0f)
                {
                    float slerp_speed = 0.75f * Time.fixedDeltaTime;
                    Quaternion new_rot = Quaternion.Slerp(curr_rot, target_rot, slerp_speed);
                    door_gameobject.transform.localRotation = new_rot;
                    //Debug.Log("Force Animating: Angle: " + angle_between+ "AngularVel: " + door_rigidbody.linearVelocity);
                }
                else {
                    SetDoorState(DoorState.None);
                }
                
                break;
            }
            case DoorState.ForceClose:
            {
                // TODO: Not Tested Yet
                
                Quaternion target_rot = Quaternion.identity;
                Quaternion curr_rot = door_gameobject.transform.localRotation;
                
                float angle_between = Quaternion.Angle(curr_rot, target_rot);

                if (angle_between > 2.0f)
                {
                    //Debug.Log("FORCE CLOSING DOOR: Angle: " + angle_between);
                    float slerp_speed = 0.75f * Time.fixedDeltaTime;
                    Quaternion new_rot = Quaternion.Slerp(curr_rot, target_rot, slerp_speed);
                    door_gameobject.transform.localRotation = new_rot;
                    //Debug.Log("Force Animating: Angle: " + angle_between+ "AngularVel: " + door_rigidbody.linearVelocity);
                }
                else {
                    //door_gameobject.transform.localRotation = Quaternion.identity;
                    SetDoorState(DoorState.None);
                    game_event_manager.event_clock_fixed.Invoke();
                }
             
                break;
            }
        }
    }

    // =================================
    // Event Callbacks
    // =================================
    
    public void OnDoorGrabbedAndReleased() {

        // TODO: trigger some animation and transform the world. Clock is now brocken
        // TODO: it is possible that the door is not fully opened or was snapped back to closed state. We must handle this. Maybe Force open the door first one first time ?
        if (state == ClockState.NeverOpenedIsWorking) {
            Debug.Log("CLOCK: FirstTimeOpen! Clock is now BROKEN!");
            state = ClockState.IsBreaking;
            clock_animator.PlayAnim(ClockAnim.Breaking);
            
            SetDoorState(DoorState.ForceOpen);
            door_grab_interactable.enabled = false;
        }
    }

    public void OnBreakAnimFinishedPlay() {
        
        // TODO: Start Env Tranformation here
        
        state = ClockState.WasOpenedIsBroken;

        left_socket_is_attached = false;
        right_socket_is_attached = false;
        left_socket_ptr.enabled = true;
        right_socket_ptr.enabled = true;
        door_grab_interactable.enabled = true;
        
        clock_animator.PlayAnim(ClockAnim.BrokenIdle);
        game_event_manager.event_clock_broken.Invoke();
        
        left_weight_dissolver.StartDissolve();
        right_weight_dissolver.StartDissolve();
    }

    public void OnWeightAttachedToLeftSocket(SelectEnterEventArgs args) {
        //Debug.Log("CLOCK: Attached weight to left socket");
        left_socket_is_attached = true;
        left_weight_dissolver.Reset();
        
        clock_animator.PlayAnim(ClockAnim.LeftRepairing);
        
        GameObject weight_grabbable = args.interactableObject.transform.gameObject;
        WeightGotAttached(weight_grabbable);
        left_socket_ptr.enabled = false;
    }

    public void OnWeightAttachedToRightSocket(SelectEnterEventArgs args) {
        //Debug.Log("CLOCK: Attached weight to right socket");
        right_socket_is_attached = true;
        right_weight_dissolver.Reset();
        
        clock_animator.PlayAnim(ClockAnim.RightRepairing);
        
        GameObject weight_grabbable = args.interactableObject.transform.gameObject;
        WeightGotAttached(weight_grabbable);
        right_socket_ptr.enabled = false;
    }
}
