using UnityEngine;
using UnityEngine.Rendering;


public class GameManager : MonoBehaviour {
    
    public static GameManager instance;

    public Player player;

    public GameObject weight_grabable_prefab;
    public GameEventManager event_manager;

    //public GameObject normal_scene_go;
    //public GameObject transformed_scene_go;

    public GameSettingsSO game_settings;

    public Transform future_weight_spawn;
    public Transform past_weight_spawn;

    private ClockWeight grab_weight_1;
    private ClockWeight grab_weight_2;

    //private ProbeReferenceVolume probe_ref_volume;

    // private string clock_working_light_scenario = "NormalEnv";
    // private string clock_broken_light_scenario = "TransformedEnv";
    
    private void Awake() {

        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this);
            return;
        }

        Debug.Assert(future_weight_spawn != null);
        Debug.Assert(past_weight_spawn != null);
        Debug.Assert(weight_grabable_prefab != null);
        
        
        game_settings.ResetAllToDefaults();

        GameObject player_go = GameObject.FindWithTag("Player");
        if (player_go != null){
            player = player_go.GetComponent<Player>();
        } else {
            Debug.LogError("Failed to Find 'Player' in this Scene");
        }
        
        // INIT weights.
        GameObject grab_weight_1_go = GameObject.Instantiate(weight_grabable_prefab, Vector3.zero, Quaternion.identity);
        grab_weight_1 = grab_weight_1_go.GetComponent<ClockWeight>();
        Debug.Assert(grab_weight_1 != null);
        grab_weight_1.ID = 1;


        GameObject grab_weight_2_go = GameObject.Instantiate(weight_grabable_prefab, Vector3.zero, Quaternion.identity);
        grab_weight_2 = grab_weight_2_go.GetComponent<ClockWeight>();
        Debug.Assert(grab_weight_2 != null);
        grab_weight_2.ID = 2;

        event_manager.event_menu_opened.game_event.AddListener(grab_weight_1.OnMenuOpen);
        event_manager.event_menu_opened.game_event.AddListener(grab_weight_2.OnMenuOpen);
        event_manager.event_menu_closed.game_event.AddListener(grab_weight_1.OnMenuClose);
        event_manager.event_menu_closed.game_event.AddListener(grab_weight_2.OnMenuClose);
    }

    private void Start() {
        HardReset();
        event_manager.event_init_game.Invoke();
    }

    public void HardReset() {
        
        grab_weight_1.Despawn();
        grab_weight_2.Despawn();
    }

    public void OnBreakTransitionFinished() {
        
        grab_weight_1.SpawnAt(future_weight_spawn);
        grab_weight_2.SpawnAt(past_weight_spawn);
    }

    public void OnRepairTransitionFinished(){

        
        Debug.Assert(!grab_weight_1.IsSpawned());
        Debug.Assert(!grab_weight_2.IsSpawned());
        
        HardReset();
    }

    // public void OnClockFixedCallback() {
        
    //     // Debug.LogWarning("GameManager: clock is fixeed");
        
    //     // Debug.Assert(!grab_weight_1.IsSpawned());
    //     // Debug.Assert(!grab_weight_2.IsSpawned());
        
    //     // HardReset();
    // }

    public void HapticImpulse(float amplitude, float duration, HapticDevice device_type = HapticDevice.Both){
        if (player != null){
            player.HapticImpulse(amplitude, duration, device_type);
        }
    }

    public void HapticImpulseLong(float amplitude, float duration, HapticDevice device_type = HapticDevice.Both){
        if (player != null){
            player.HapticImpulseLong(amplitude, duration, device_type);
        }
    }

    public void QuitGameNow() {
        Application.Quit();
    }
}
