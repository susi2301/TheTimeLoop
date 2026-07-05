using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using Flcrm;
using BezierSolution;

public class TranformVfxHandler : MonoBehaviour
{
    
    private GameEventManager event_manager;

    public GameObject normal_scene_go;
    public GameObject transformed_scene_go;
    public Transform clock_center;

    public GameObject view_blocker_a;
    public GameObject view_blocker_b;

    public float view_blocker_max_dist = 100.0f;
    public float view_blocker_min_dist = 20.0f;

    public MeshRenderer tunnel_meshren;
    private int tunnel_dist_hash = Shader.PropertyToID("_transform_dist");
    private int sky_exposure_hash = Shader.PropertyToID("_Exposure");

    public float tunnel_min_dist = -5.0f;
    public float tunnel_mid_dist = 1.0f;
    public float tunnel_max_dist = 30.0f;
    public float tunnel_first_duration = 5.0f;
    public float tunnel_second_duration = 5.0f;
    public EasingFunction dist_easing_function;

    public float fog_normal_density = 0.07f;
    public float fog_max_density = 0.4f;

    public VisualEffect vfx_buildup;
    public VisualEffect vfx_burst;


    public float spirit_duration = 10.0f;

    public BezierAttachment spirit_bezier_1;
    public VisualEffect vfx_spirit_1;
    public BezierAttachment spirit_bezier_2;
    public VisualEffect vfx_spirit_2;

    private ProbeReferenceVolume probe_ref_volume;
    public string normal_light_scenario = "NormalEnv";
    public string transformed_light_scenario = "TransformedEnv";


    private void Awake() {

        Debug.Assert(tunnel_meshren != null);
        Debug.Assert(normal_scene_go != null);
        Debug.Assert(transformed_scene_go != null);
        
        GameObject event_man_go = GameObject.FindWithTag("GameEventManager");
        if (event_man_go != null){
            event_manager = event_man_go.GetComponent<GameEventManager>();
        } else {
            Debug.LogError("Failed To find GameEventManager in this scene");
        }

        probe_ref_volume = UnityEngine.Rendering.ProbeReferenceVolume.instance;
    }

    public void Start() {
        HardReset();
    }

    public void HardReset(){
        StopAllCoroutines();
        
        SetViewBlockerTransform(view_blocker_a.transform, clock_center.position, view_blocker_max_dist);
        SetViewBlockerTransform(view_blocker_b.transform, clock_center.position, view_blocker_max_dist);
        view_blocker_a.SetActive(false);
        view_blocker_b.SetActive(false);


        normal_scene_go.SetActive(true);
        transformed_scene_go.SetActive(false);
        vfx_buildup.Stop();
        vfx_burst.Stop();

        spirit_bezier_1.normalizedT = 0.0f;
        vfx_spirit_1.Stop();
        vfx_spirit_1.SetFloat("normt", 0.0f);

        vfx_spirit_2.Stop();
        vfx_spirit_2.SetFloat("normt", 0.0f);
        spirit_bezier_2.normalizedT = 0.0f;

        RenderSettings.skybox.SetFloat("_Exposure", 0.6f);

        tunnel_meshren.sharedMaterial.SetFloat(tunnel_dist_hash, tunnel_min_dist);
        RenderSettings.fogDensity = fog_normal_density;

        StartCoroutine(DelayedSwitchToNormalLighting());
    }

    public IEnumerator DelayedSwitchToNormalLighting() {
        yield return null;

        probe_ref_volume.BlendLightingScenario(normal_light_scenario, 1.0f);
        probe_ref_volume.lightingScenario = normal_light_scenario;
    }

    private void SetViewBlockerTransform(Transform blocker, Vector3 clock_center, float dist){

        Vector3 to_blocker = Vector3.Normalize(blocker.position - clock_center);

        Vector3 target_pos = clock_center + to_blocker * dist;
        Quaternion target_rot = Quaternion.LookRotation(-to_blocker, Vector3.up);

        blocker.SetPositionAndRotation(target_pos, target_rot);
    }

    private void OnDisable(){
        Debug.Log("ON DISABLE CALLED");
        //if (probe_ref_volume.lightingScenario == transformed_light_scenario){
            probe_ref_volume.BlendLightingScenario(normal_light_scenario, 1.0f);
            probe_ref_volume.lightingScenario = normal_light_scenario;
        //}
    }

    // ======== Break Transition ========
    public void BreakStartTransition(){
        StartCoroutine(BreakWallShaderFade());
        StartCoroutine(BreakTransitionFogFade());
    }

    public IEnumerator BreakWallShaderFade(){
        

        yield return null;
        yield return new WaitForSeconds(2.0f);

        float time_accum = 0.0f;

        while (time_accum < tunnel_first_duration){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, tunnel_first_duration) / tunnel_first_duration;
            float lerp_val = Flcrm.Mathy.ease_float(percentage, dist_easing_function);
            
            float transform_dist = Mathf.Lerp(tunnel_min_dist, tunnel_mid_dist, lerp_val);

            tunnel_meshren.sharedMaterial.SetFloat(tunnel_dist_hash, transform_dist);

            yield return null;
        }

        //yield return new WaitForSeconds(0.5f);

        time_accum = 0.0f;
        while (time_accum < tunnel_second_duration){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, tunnel_second_duration) / tunnel_second_duration;
            
            float lerp_val = Flcrm.Mathy.ease_float(percentage, EasingFunction.InQuadratic);
            float transform_dist = Mathf.Lerp(tunnel_mid_dist, tunnel_max_dist, lerp_val);

            tunnel_meshren.sharedMaterial.SetFloat(tunnel_dist_hash, transform_dist);

            yield return null;
        }

        yield return null;
        yield return null;

        // @Note: Reset back so we can branch in shader and turn of most of the heavy noise stuff.
        tunnel_meshren.sharedMaterial.SetFloat(tunnel_dist_hash, tunnel_min_dist);
    }

    public IEnumerator BreakTransitionFogFade() {
        
        float fade_in_dur  = 8.5f;
        float stay_faded_dur = 2.5f;
        float fade_out_dur = 3.5f;


        yield return null;
        yield return null;


        float time_accum = 0.0f;


        SetViewBlockerTransform(view_blocker_a.transform, clock_center.position, view_blocker_max_dist);
        SetViewBlockerTransform(view_blocker_b.transform, clock_center.position, view_blocker_max_dist);
        view_blocker_a.SetActive(true);
        view_blocker_b.SetActive(true);
        
        GameManager.instance.HapticImpulseLong(0.15f, 4.0f);


        bool buildup_playing = false;
        // Fade in
        while (time_accum < fade_in_dur){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, fade_in_dur) / fade_in_dur;
            float lerp_val = Mathy.ease_float(percentage, EasingFunction.InCubic);
            
            RenderSettings.fogDensity = Mathy.lerp(fog_normal_density, fog_max_density, lerp_val);

            float expo =  Mathy.lerp(0.6f, 0.0f, lerp_val);
            RenderSettings.skybox.SetFloat("_Exposure", expo);
            
            // start buildup 50% in
            if (!buildup_playing && percentage > 0.5f){
                buildup_playing = true;
                vfx_buildup.Play();
                //GameManager.instance.HapticImpulse(0.5f, 6.2f);
                GameManager.instance.HapticImpulseLong(0.5f, 6.2f);
            }

            float blocker_dist = Mathy.lerp(view_blocker_max_dist, view_blocker_min_dist, lerp_val);
            SetViewBlockerTransform(view_blocker_a.transform, clock_center.position, blocker_dist);
            SetViewBlockerTransform(view_blocker_b.transform, clock_center.position, blocker_dist);

            yield return null;
        }



        RenderSettings.fogDensity = fog_max_density;


        vfx_buildup.Stop();
        // TODO: here is also where we would want to swap the meshes out.

        yield return null;
        // Turn Off normal env
        normal_scene_go.SetActive(false);


        // wait a couple frames to spread out the work
        yield return null;
        yield return null;
        yield return null;
        
        //GameManager.instance.HapticImpulseLong(0.5f, 4.5f);

        
        bool burst_playing = false;

        // Blend Lighting
        time_accum = 0.0f;

        while (time_accum < stay_faded_dur){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, stay_faded_dur) / stay_faded_dur;
            float lerp_val = Flcrm.Mathy.ease_float(percentage, EasingFunction.Linear);
            
            probe_ref_volume.BlendLightingScenario(transformed_light_scenario, lerp_val); 
            
            if (!burst_playing && percentage > 0.8f){
                burst_playing = true;
                //vfx_burst.Play();
            }

            yield return null;
        }

        probe_ref_volume.BlendLightingScenario(transformed_light_scenario, 1.0f); 
        probe_ref_volume.lightingScenario = transformed_light_scenario;


        // Turn On Tranformed env
        yield return null;
        transformed_scene_go.SetActive(true);
        yield return null;
        yield return null;


        // Fade out Fog
        time_accum = 0.0f;
        while (time_accum < fade_in_dur){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, fade_out_dur) / fade_out_dur;
            float lerp_val = Flcrm.Mathy.ease_float(percentage, EasingFunction.OutCirc);
            
            RenderSettings.fogDensity = Mathy.lerp(fog_max_density, fog_normal_density, lerp_val);

            float expo =  Mathy.lerp(0.0f, 0.6f, lerp_val);
            RenderSettings.skybox.SetFloat("_Exposure", expo);

            float blocker_dist = Mathy.lerp(view_blocker_min_dist, view_blocker_max_dist, lerp_val);
            SetViewBlockerTransform(view_blocker_a.transform, clock_center.position, blocker_dist);
            SetViewBlockerTransform(view_blocker_b.transform, clock_center.position, blocker_dist);
        

            yield return null;
        }

        RenderSettings.fogDensity = fog_normal_density;

        SetViewBlockerTransform(view_blocker_a.transform, clock_center.position, view_blocker_max_dist);
        SetViewBlockerTransform(view_blocker_b.transform, clock_center.position, view_blocker_max_dist);
        view_blocker_a.SetActive(false);
        view_blocker_b.SetActive(false);
        
        //vfx_burst.Stop();

        yield return null;

        event_manager.event_break_transition_finished.Invoke();
    }


    public void OnBreakAnimFinished(){
        StartCoroutine(BreakSpiritEffect());
    }

    public IEnumerator BreakSpiritEffect(){

        yield return null;

        vfx_spirit_1.Play();
        vfx_spirit_1.SetFloat("normt", 0.0f);

        vfx_spirit_2.Play();
        vfx_spirit_2.SetFloat("normt", 0.0f);

        float time_accum = 0.0f;
        while (time_accum < spirit_duration){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, spirit_duration) / spirit_duration;

            float lerp_val = Flcrm.Mathy.ease_float(percentage, EasingFunction.InCubic);
            
            float t = lerp_val;

            vfx_spirit_1.SetFloat("normt", t);
            spirit_bezier_1.normalizedT = t;

            vfx_spirit_2.SetFloat("normt", t);
            spirit_bezier_2.normalizedT = t;


            yield return null;
        }

        spirit_bezier_1.normalizedT = 1.0f;
        spirit_bezier_2.normalizedT = 1.0f;
        vfx_spirit_1.Stop();
        vfx_spirit_2.Stop();
    }



    // ======== Repair Transition ========

    public void RepairStartTransition(){
        StartCoroutine(RepairTransition());
    }

    private IEnumerator RepairTransition(){

        float fade_in_dur  = 3.5f;
        float stay_faded_dur = 1.5f;
        float fade_out_dur = 2.5f;

        yield return null;

        GameManager.instance.HapticImpulse(0.2f, 5.2f);

        float time_accum = 0.0f;

        while (time_accum < fade_in_dur){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, fade_in_dur) / fade_in_dur;
            float lerp_val = Mathy.ease_float(percentage, EasingFunction.InCubic);
            
            RenderSettings.fogDensity = Mathy.lerp(fog_normal_density, fog_max_density, lerp_val);

            float expo =  Mathy.lerp(0.6f, 0.0f, lerp_val);
            RenderSettings.skybox.SetFloat(sky_exposure_hash, expo);
            
            float blocker_dist = Mathy.lerp(view_blocker_max_dist, view_blocker_min_dist, lerp_val);
            SetViewBlockerTransform(view_blocker_a.transform, clock_center.position, blocker_dist);
            SetViewBlockerTransform(view_blocker_b.transform, clock_center.position, blocker_dist);

            yield return null;
        }



        // ======================================


        yield return null;
        // Turn Off transformed env
        transformed_scene_go.SetActive(false);

        // wait a couple frames to spread out the work
        yield return null;
        yield return null;
        yield return null;
        
        // Turn On normal Env
        normal_scene_go.SetActive(true);

        yield return null;
        
        //bool burst_playing = false;

        // Blend Lighting
        time_accum = 0.0f;

        while (time_accum < stay_faded_dur){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, stay_faded_dur) / stay_faded_dur;
            float lerp_val = Flcrm.Mathy.ease_float(percentage, EasingFunction.Linear);
            
            probe_ref_volume.BlendLightingScenario(normal_light_scenario, lerp_val); 
            
            // if (!burst_playing && percentage > 0.8f){
            //     burst_playing = true;
            //     //vfx_burst.Play();
            // }

            yield return null;
        }

        probe_ref_volume.BlendLightingScenario(normal_light_scenario, 1.0f); 
        probe_ref_volume.lightingScenario = normal_light_scenario;


        yield return null;
        yield return null;
        yield return null;


        // ======================================




        // Fade out Fog
        time_accum = 0.0f;
        while (time_accum < fade_in_dur){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, fade_out_dur) / fade_out_dur;
            float lerp_val = Flcrm.Mathy.ease_float(percentage, EasingFunction.OutCirc);
            
            RenderSettings.fogDensity = Mathy.lerp(fog_max_density, fog_normal_density, lerp_val);

            float expo =  Mathy.lerp(0.0f, 0.6f, lerp_val);
            RenderSettings.skybox.SetFloat("_Exposure", expo);

            float blocker_dist = Mathy.lerp(view_blocker_min_dist, view_blocker_max_dist, lerp_val);
            SetViewBlockerTransform(view_blocker_a.transform, clock_center.position, blocker_dist);
            SetViewBlockerTransform(view_blocker_b.transform, clock_center.position, blocker_dist);
        

            yield return null;
        }

        RenderSettings.fogDensity = fog_normal_density;

        SetViewBlockerTransform(view_blocker_a.transform, clock_center.position, view_blocker_max_dist);
        SetViewBlockerTransform(view_blocker_b.transform, clock_center.position, view_blocker_max_dist);
        view_blocker_a.SetActive(false);
        view_blocker_b.SetActive(false);

        yield return null;
        yield return null;


        // @Note: this is invoked from clock because clock knows when the door is fully shut again.
        // GameManager.instance.event_manager.event_repair_transition_finished.Invoke();
    }
}
