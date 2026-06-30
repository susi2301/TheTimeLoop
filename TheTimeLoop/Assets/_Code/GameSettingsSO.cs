using UnityEngine;

[CreateAssetMenu(fileName = "GameSettingsSO", menuName = "Scriptable Objects/GameSettingsSO")]
public class GameSettingsSO : ScriptableObject
{
    public float global_volume;
    public float global_volume_default;

    public void ResetAllToDefaults(){
    	global_volume = global_volume_default;
    }
}
