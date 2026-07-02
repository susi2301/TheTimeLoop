using UnityEngine;
using UnityEngine.Rendering;


public class GameManager : MonoBehaviour {
    
    public GameObject weight_grabable_prefab;
    public GameEventManager event_manager;

    public GameObject normal_scene_go;
    public GameObject transformed_scene_go;

    public GameSettingsSO game_settings;

    public Transform future_weight_spawn;
    public Transform past_weight_spawn;

    private ClockWeight grab_weight_1;
    private ClockWeight grab_weight_2;

    private ProbeReferenceVolume probe_ref_volume;

    private string clock_working_light_scenario = "NormalEnv";
    private string clock_broken_light_scenario = "TransformedEnv";
    
    private void Awake() {
        Debug.Assert(future_weight_spawn != null);
        Debug.Assert(past_weight_spawn != null);
        Debug.Assert(weight_grabable_prefab != null);
        
        Debug.Assert(normal_scene_go != null);
        Debug.Assert(transformed_scene_go != null);
        
        game_settings.ResetAllToDefaults();

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
        
        normal_scene_go.SetActive(true);
        transformed_scene_go.SetActive(false);
        
        probe_ref_volume.lightingScenario = clock_working_light_scenario;
        probe_ref_volume.PerformPendingOperations();
        
        grab_weight_1.Despawn();
        grab_weight_2.Despawn();
    }

    // Clock Callbacks
    public void OnClockIsBrokenCallback() {
        
        grab_weight_1.SpawnAt(future_weight_spawn);
        grab_weight_2.SpawnAt(past_weight_spawn);
        
        // Activate Gameobject.
        // TODO: effects and stuff.
       
        normal_scene_go.SetActive(false);
        transformed_scene_go.SetActive(true);
        
        probe_ref_volume.lightingScenario = clock_broken_light_scenario;
        //probe_ref_volume.BlendLightingScenario("ClockBroken", 1.0f);
    }

    public void OnClockFixedCallback() {
        
        Debug.LogWarning("GameManager: clock is fixeed");
        
        Debug.Assert(!grab_weight_1.IsSpawned());
        Debug.Assert(!grab_weight_2.IsSpawned());
        
        HardReset();
    }

    public void QuitGameNow() {
        Application.Quit();
    }

    private void OnDisable() {
        probe_ref_volume.lightingScenario = clock_working_light_scenario;
    }
}
