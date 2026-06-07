using System;
using UnityEditor.SceneManagement;
using UnityEngine;


public class GameManager : MonoBehaviour {

    public GameObject weight_grabable_prefab;

    public GameObject past_tunnel_go;
    public GameObject future_tunnel_go;
    public GameObject entrance_area_go;
    
    public Transform future_weight_spawn;
    public Transform past_weight_spawn;

    public Clock clock;
    
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

        clock.event_on_clock_broken.AddListener(OnClockIsBrokenCallback);
        clock.event_on_clock_fixed.AddListener(OnClockFixedCallback);
        
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
    }

    // Clock Callbacks
    public void OnClockIsBrokenCallback() {
        // spawn weights.
        GameObject.Instantiate(weight_grabable_prefab, future_weight_spawn.position, future_weight_spawn.rotation);
        GameObject.Instantiate(weight_grabable_prefab, past_weight_spawn.position  , past_weight_spawn.rotation);
        
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
    }

    public void OnClockFixedCallback() {
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
    }
    
}
