using UnityEngine;
using System.Collections;
using Flcrm;

public class TranformVfxHandler : MonoBehaviour
{
    
    public MeshRenderer tunnel_meshren;

    public float tunnel_min_dist = -5.0f;
    public float tunnel_max_dist = 30.0f;
    public float dist_duration = 5.0f;
    public EasingFunction dist_easing_function;

    private void Awake(){
        Debug.Assert(tunnel_meshren != null);
        HardReset();
    }

    public void HardReset(){
        tunnel_meshren.sharedMaterial.SetFloat("_transform_dist", tunnel_min_dist);
    }

    public void StartTransformVfx(){
        StartCoroutine(BreakTransitionFade());
    }

    public IEnumerator BreakTransitionFade(){
        yield return null;
        
        yield return new WaitForSeconds(1.50f);

        float time_accum = 0.0f;


        while (time_accum < dist_duration){
            time_accum += Time.deltaTime;

            float percentage = Mathf.Clamp(time_accum, 0.0f, dist_duration) / dist_duration;
            
            float lerp_val = Flcrm.Mathy.ease_float(percentage, dist_easing_function);
            float transform_dist = Mathf.Lerp(tunnel_min_dist, tunnel_max_dist, lerp_val);

            tunnel_meshren.sharedMaterial.SetFloat("_transform_dist", transform_dist);

            yield return null;
        }
    }
}
