using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Torch : MonoBehaviour
{

    public Light point_light;

    public float min_intensity;
    public float max_intensity;

    public float speed = 1.0f;


    public GrabableEffectiveChildOf child_of;
    public XRGrabInteractable xr_interactable;

    private Vector3 spawn_pos;
    private Quaternion spawn_rot;
    

    private void Awake(){
        spawn_pos = this.transform.position;
        spawn_rot = this.transform.rotation;
    }

    public void HardReset(){

        if (child_of.IsGrabbed()) {
            // @Note: This forces the interactable to be dropped.
            // otherwise it would stay in the hand after a restart.
            xr_interactable.enabled = false;
            xr_interactable.enabled = true;
        }

        this.transform.position = spawn_pos;
        this.transform.rotation = spawn_rot;

        child_of.HardReset();
    }

    private void Update() {
        
        float perlinA = Mathf.PerlinNoise1D(Time.time * speed);
        float perlinB = Mathf.PerlinNoise1D(Time.time * speed + Mathf.PI);
        float perlin = perlinA * 0.75f + perlinB * 0.25f;
        
        point_light.intensity = Mathf.Lerp(min_intensity, max_intensity, perlin);
    }
}
