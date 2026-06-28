using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public enum ClockState {
    NeverOpenedIsWorking = 0,
    IsBreaking,
    WasOpenedIsBroken,
    WasOpenedIsWorking,
    IsRepairing,
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

    public LoopingSoundPlayer ticking_soundplayer;
    
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
        
    }

    public void Init() {
        clock_animator.event_break_anim_finished.AddListener(OnBreakAnimFinishedPlay);
        HardReset();
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
        
        door_grab_interactable.enabled = true;
        state = ClockState.NeverOpenedIsWorking;
        left_socket_is_attached = true;
        right_socket_is_attached = true;
        
        left_socket_ptr.enabled = false;
        right_socket_ptr.enabled = false;
        
        left_weight_dissolver.Reset();
        right_weight_dissolver.Reset();
        
        ticking_soundplayer.PlaySound();
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
            door_grab_interactable.enabled = true;
            door_rigidbody.WakeUp();
            return;
        }
        
        door_grab_interactable.enabled = false;
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
            //Debug.Log("CLOCK: FirstTimeOpen! Clock is now BROKEN!");
            state = ClockState.IsBreaking;
            SetDoorState(DoorState.ForceOpen);
            SoundManager.instance.PlaySoundAt(SoundID.ClockDoorOpen, this.transform.position);
            door_grab_interactable.enabled = false;
            StartCoroutine(BreakTransition());
        }
    }

    public IEnumerator BreakTransition() {
        //while (door_state == DoorState.ForceOpen) {
        //    yield return null;
       // }

       yield return new WaitForSeconds(1.6f);
       
        clock_animator.PlayAnim(ClockAnim.Breaking);

        SoundManager.instance.PlaySoundAt(SoundID.ClockBreak, this.transform.position);
        ticking_soundplayer.StopSound(0.8f, 0.5f);
        game_event_manager.event_break_transition_start.Invoke();
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

    // GAME END CONDITION!
    public void WeightGotAttached(GameObject grabbable_weight_go) {

        ClockWeight weight = grabbable_weight_go.GetComponent<ClockWeight>();
        Debug.Assert(weight != null);
        weight.Despawn();


        if (!left_socket_is_attached || !right_socket_is_attached) {
            return;
        }
        
        
        state = ClockState.IsRepairing;
        
        StartCoroutine(FixTransition());
    }
    
    public IEnumerator FixTransition() {
        
        while (!clock_animator.left_repaired_has_finished || !clock_animator.right_repaired_has_finished)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);

        SoundManager.instance.PlaySoundAt(SoundID.ClockReactivated, ticking_soundplayer.transform.position);
        clock_animator.PlayAnim(ClockAnim.ClockRepairing);
        
        yield return new WaitForSeconds(7.0f);
        
        SetDoorState(DoorState.ForceClose);
        ticking_soundplayer.PlaySound(1.5f, 1.2f);

        while (door_state == DoorState.ForceClose) {
            yield return null;
        }
        state = ClockState.WasOpenedIsWorking;
        SoundManager.instance.PlaySoundAt(SoundID.ClockDoorClose, this.transform.position);
        
        game_event_manager.event_clock_fixed.Invoke();
    }



    public void OnWeightAttachedToLeftSocket(SelectEnterEventArgs args) {
        //Debug.Log("CLOCK: Attached weight to left socket");
        left_socket_is_attached = true;
        left_weight_dissolver.Reset();
        
        SoundManager.instance.PlaySoundAt(SoundID.ClockFixA, this.transform.position, 0.05f, 0.05f);
        clock_animator.PlayAnim(ClockAnim.LeftRepairing);
        
        GameObject weight_grabbable = args.interactableObject.transform.gameObject;
        WeightGotAttached(weight_grabbable);
        left_socket_ptr.enabled = false;
    }

    public void OnWeightAttachedToRightSocket(SelectEnterEventArgs args) {
        //Debug.Log("CLOCK: Attached weight to right socket");
        right_socket_is_attached = true;
        right_weight_dissolver.Reset();
        
        SoundManager.instance.PlaySoundAt(SoundID.ClockFixB, this.transform.position,0.05f, 0.05f);
        clock_animator.PlayAnim(ClockAnim.RightRepairing);
        
        GameObject weight_grabbable = args.interactableObject.transform.gameObject;
        WeightGotAttached(weight_grabbable);
        right_socket_ptr.enabled = false;
    }
}
