using UnityEngine;

public class LoopingSoundPlayer : MonoBehaviour {
    
    public SoundID sound_id;
    public float max_distance = 5.0f;
    public bool play_on_start = false;
    public bool dynamic_position = false;
    
    // only valid if currently playing or paused;
    private SoundSource sound_source_ref = null;

    private void Start() {
        if (play_on_start) {
            PlaySound();
        }
    }

    public void PlaySound(float delay = 0.0f, float fade_in_duration = 0.0f) {
        
        if (sound_source_ref != null) {
            return; // already playing.
        }
        
        sound_source_ref = SoundManager.instance.PlaySoundLoopingAt(sound_id, this.transform.position, delay, fade_in_duration, max_distance);
    }

    public void StopSound(float delay = 0.0f, float fade_out_duration = 0.0f) {
        
        if (sound_source_ref == null) {
            return;
        }
        
        SoundManager.instance.StopAndReturnSoundSource(sound_source_ref, delay, fade_out_duration);
        sound_source_ref = null;
    }

    private void OnDrawGizmosSelected() {
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.transform.position, max_distance);
    }
}
