using UnityEngine;

public class UIState_MainMenu : MonoBehaviour {
    public MenuManager manager;

    public GameObject panel_go;

    public void EnterState() {
        panel_go.SetActive(true);
    }

    public void LeaveState() {
        panel_go.SetActive(false);
    }
    
    public void OnButtonPlay() {
        manager.CloseMenu();
    }

    public void OnButtonRestart() {
        manager.game_manager.RestartGame();
        manager.CloseMenu();
    }
    
    public void OnButtonSettings() {
        manager.SwitchState(MenuState.Settings);
    }

    public void OnButtonCredits() {
        manager.SwitchState(MenuState.Credits);
    }

    public void OnButtonQuit() {
        manager.game_manager.QuitGame();
    }

}
