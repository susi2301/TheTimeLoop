using UnityEngine;

[CreateAssetMenu(fileName = "GameSettingsSO", menuName = "Scriptable Objects/GameSettingsSO")]
public class GameSettingsSO : ScriptableObject
{
    public float global_volume;
    public float global_volume_default;

    public float head_height;
    public float head_height_default;
    public float head_height_max;

    public void ResetAllToDefaults(){
    	global_volume = global_volume_default;
    	head_height = head_height_default;
    }
}
