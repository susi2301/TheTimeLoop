using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Flcrm;


public class UIState_Settings : MonoBehaviour {
    public MenuManager manager;

    public GameObject panel_go;
    public GameSettingsSO game_settings;

    public TextMeshProUGUI audio_vol_nbr_txt;
    public Slider audio_vol_slider;

    public TextMeshProUGUI head_height_nbr_txt;
    public Slider head_height_slider;
    
    public Button head_height_apply_btn;

    private float last_displayed_virtual_head_height;


    public void EnterState() {
        panel_go.SetActive(true);

        audio_vol_slider.value = Mathf.Clamp(game_settings.global_volume, 0.0f, 100.0f);
        OnAudioVolumeSliderChanged();

        //float head_height = Mathf.Clamp(game_settings.head_height, 0.0f, game_settings.head_height_max);
        //float head_slider_val = head_height / game_settings.head_height_max;
        
        // @Note: HACK: Setting this true because line below will invoke 'OnHeadHeightSliderChanged'
        // But for now when entering setting we just make sure to display the current head height
        HeightAdjustingState curr_state = manager.height_adjust_state;

        manager.height_adjust_state = HeightAdjustingState.Adjusting; 
        head_height_slider.value = 0.0f;
        //not_yet_applied_head_height = head_height;
        float current_virutal_height = manager.player.GetCurrentVirtualHeadHeight();
        last_displayed_virtual_head_height = current_virutal_height;

        UpdateTextUiElementWithFloat(head_height_nbr_txt, current_virutal_height, "F2");
        head_height_apply_btn.interactable = false;

        manager.height_adjust_state = curr_state; 
    }

    public void ExitState() {
        panel_go.SetActive(false);

        if (manager.height_adjust_state != HeightAdjustingState.IsApplied) {

            manager.ExitHeightAdjustingState();
        }
    }

    public void UpdateState(){
        
        // TODO cache value and only update ui when value is significantly different.

        float current_virutal_height = manager.player.GetCurrentVirtualHeadHeight();

        float height_offset = head_height_slider.value;

        float offseted_virtual_height = current_virutal_height + height_offset;

        // TODO: clamp the displayed value to the valid range of 0..4

        float diff = Mathf.Abs(last_displayed_virtual_head_height - offseted_virtual_height);
        if (diff > 0.005f) {
            UpdateTextUiElementWithFloat(head_height_nbr_txt, offseted_virtual_height, "F2");
            last_displayed_virtual_head_height = offseted_virtual_height;
        }
    }


    public void OnAudioVolumeSliderChanged() {
        float audio_vol = audio_vol_slider.value;
       
        SoundManager.instance.AdjustGlobalVolume(audio_vol);
        UpdateTextUiElementWithFloat(audio_vol_nbr_txt, game_settings.global_volume, "F0");
    }


    public void OnHeadHeightSliderChanged() {

        //float new_head_height = Mathf.Lerp(0.0f, game_settings.head_height_max, head_height_slider.value);
        //not_yet_applied_head_height = new_head_height;


        //UpdateTextUiElementWithFloat(head_height_nbr_txt, not_yet_applied_head_height, "F2");
        UpdateState();


        if (manager.height_adjust_state == HeightAdjustingState.IsApplied || manager.height_adjust_state == HeightAdjustingState.FadeOut) {
            manager.EnterHeightAdjustingState();
            head_height_apply_btn.interactable = true;
        }
    }

    public void OnButtonApplyHeadHeight() {
        
        manager.player.ApplyHeadHeightOffset(head_height_slider.value);
        
        // reset slider
        head_height_slider.value = 0.0f;


        manager.ReorientUI();
        manager.ExitHeightAdjustingState();
        head_height_apply_btn.interactable = false;
    }

    private void UpdateTextUiElementWithFloat(TextMeshProUGUI text_pro, float new_value, string fmt = "F1") {
        
        text_pro.text = new_value.ToString(fmt);
    }
}
