using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;


public class GameManager : MonoBehaviour {

    public GameObject weight_grabable_prefab;

    public GameObject past_tunnel_go;
    public GameObject future_tunnel_go;
    public GameObject entrance_area_go;
    
    public Clock clock;

    public Transform future_weight_spawn;
    public Transform past_weight_spawn;

    private ClockWeight grab_weight_1;
    private ClockWeight grab_weight_2;

    private ProbeReferenceVolume probe_ref_volume;
    
    
    private void Awake() {
        Debug.Assert(clock != null);
        Debug.Assert(future_weight_spawn != null);
        Debug.Assert(past_weight_spawn != null);
        Debug.Assert(weight_grabable_prefab != null);
        
        if (future_tunnel_go == null) {
            Debug.LogWarning("GAME_MANAGER: Future tunnel gameobject is not assigned!");
        }

        if (past_tunnel_go == null) {
            Debug.LogWarning("GAME_MANAGER: Past tunnel gameobject is not assigned!");
        }
        
        if (entrance_area_go == null) {
            Debug.LogWarning("GAME_MANAGER: Entrance Area gameobject is not assigned!");
        }

        probe_ref_volume = UnityEngine.Rendering.ProbeReferenceVolume.instance;

        // INIT weights.
        GameObject grab_weight_1_go = GameObject.Instantiate(weight_grabable_prefab, Vector3.zero, Quaternion.identity);
        grab_weight_1 = grab_weight_1_go.GetComponent<ClockWeight>();
        Debug.Assert(grab_weight_1 != null);
        grab_weight_1.ID = 1;

        GameObject grab_weight_2_go = GameObject.Instantiate(weight_grabable_prefab, Vector3.zero, Quaternion.identity);
        grab_weight_2 = grab_weight_2_go.GetComponent<ClockWeight>();
        Debug.Assert(grab_weight_2 != null);
        grab_weight_2.ID = 2;

        clock.event_on_clock_broken.AddListener(OnClockIsBrokenCallback);
        clock.event_on_clock_fixed.AddListener(OnClockFixedCallback);
        
    }

    private void Start() {
        HardReset();
    }

    public void HardReset() {

        if (future_tunnel_go != null) {
            future_tunnel_go.SetActive(false);
        }

        if (past_tunnel_go != null) {
            past_tunnel_go.SetActive(false);
        }

        if (entrance_area_go != null) {
            entrance_area_go.SetActive(true);
        }
        
        probe_ref_volume.lightingScenario = "ClockWorking";
        
        grab_weight_1.Despawn();
        grab_weight_2.Despawn();
        
        clock.HardReset();
    }

    // Clock Callbacks
    public void OnClockIsBrokenCallback() {
        
        grab_weight_1.SpawnAt(future_weight_spawn);
        grab_weight_2.SpawnAt(past_weight_spawn);
        
        // Activate Gameobject.
        // TODO: effects and stuff.
        if (future_tunnel_go != null) {
            future_tunnel_go.SetActive(true);
        }

        if (past_tunnel_go != null) {
            past_tunnel_go.SetActive(true);
        }
        
        if (entrance_area_go != null) {
            entrance_area_go.SetActive(false);
        }
        
        
        //probe_ref_volume.lightingScenario = "ClockBroken";
        probe_ref_volume.BlendLightingScenario("ClockBroken", 1.0f);
    }

    public void OnClockFixedCallback() {
        
        Debug.Assert(!grab_weight_1.IsSpawned());
        Debug.Assert(!grab_weight_2.IsSpawned());
        
        // TODO: effects and stuff.
        if (future_tunnel_go != null) {
            future_tunnel_go.SetActive(false);
        }

        if (past_tunnel_go != null) {
            past_tunnel_go.SetActive(false);
        }
        
        if (entrance_area_go != null) {
            entrance_area_go.SetActive(true);
        }
        HardReset();
    }
    
}
