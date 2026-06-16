using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIState_Settings : MonoBehaviour {
    public MenuManager manager;

    public GameObject panel_go;
    

    public TextMeshProUGUI audio_vol_nbr_txt;
    public Slider audio_vol_slider;

    public float debug_audio_value;
    
    public void EnterState() {
        panel_go.SetActive(true);

        audio_vol_slider.value = debug_audio_value / 100.0f; // assuming audio goes from 0..100 we remap it to slider value 0..1;
        UpdateTextUiElement(audio_vol_nbr_txt, debug_audio_value);
    }

    public void LeaveState() {
        panel_go.SetActive(false);
    }


    public void OnAudioVolumeSliderChanged() {
        float audio_vol = Mathf.Lerp(0.0f, 100.0f, audio_vol_slider.value);
        debug_audio_value = audio_vol;
        UpdateTextUiElement(audio_vol_nbr_txt, audio_vol);
    }

    public void UpdateTextUiElement(TextMeshProUGUI text_pro, float new_value) {
        
        audio_vol_nbr_txt.text = new_value.ToString("F1");
    }
}
