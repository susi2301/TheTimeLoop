using UnityEngine;

public class Torch : MonoBehaviour
{

    public Light point_light;

    public float min_intensity;
    public float max_intensity;

    public float speed = 1.0f;
    
    private void Update() {
        
        float perlinA = Mathf.PerlinNoise1D(Time.time * speed);
        float perlinB = Mathf.PerlinNoise1D(Time.time * speed + Mathf.PI);
        float perlin = perlinA * 0.75f + perlinB * 0.25f;
        
        point_light.intensity = Mathf.Lerp(min_intensity, max_intensity, perlin);
    }
}
