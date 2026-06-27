using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIState_Settings : MonoBehaviour {
    public MenuManager manager;

    public GameObject panel_go;
    public GameSettingsSO game_settings;

    public TextMeshProUGUI audio_vol_nbr_txt;
    public Slider audio_vol_slider;

    public TextMeshProUGUI head_height_nbr_txt;
    public Slider head_height_slider;
    
    private float not_yet_applied_head_height;

    public void EnterState() {
        panel_go.SetActive(true);

        audio_vol_slider.value = Mathf.Clamp(game_settings.global_volume, 0.0f, 100.0f);
        OnAudioVolumeSliderChanged();

        float head_height = Mathf.Clamp(game_settings.head_height, 0.0f, game_settings.head_height_max);
        float head_slider_val = head_height / game_settings.head_height_max;
        head_height_slider.value = head_slider_val;
        not_yet_applied_head_height = head_height;
        UpdateTextUiElement(head_height_nbr_txt, head_height, "F2");
    
    }

    public void LeaveState() {
        panel_go.SetActive(false);
    }


    public void OnAudioVolumeSliderChanged() {
        float audio_vol = audio_vol_slider.value;
       
        SoundManager.instance.AdjustGlobalVolume(audio_vol);
        UpdateTextUiElement(audio_vol_nbr_txt, game_settings.global_volume, "F0");
    }


    public void OnHeadHeightSliderChanged() {
        float new_head_height = Mathf.Lerp(0.0f, game_settings.head_height_max, head_height_slider.value);
        not_yet_applied_head_height = new_head_height;
        UpdateTextUiElement(head_height_nbr_txt, not_yet_applied_head_height, "F2");
    }

    public void OnButtonApplyHeadHeight() {
        manager.player.SetHeadHeight(not_yet_applied_head_height);
        manager.ReorientUI();
    }

    private void UpdateTextUiElement(TextMeshProUGUI text_pro, float new_value, string fmt = "F1") {
        
        text_pro.text = new_value.ToString(fmt);
    }

}
