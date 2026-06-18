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

    public void OnButtonRestart()
    {
        float angle = manager.player.CalculateCameraAngleBetweenForwardNowAndForwardSpawn();
        manager.vignette_animator.JustSetThisValueAndDontAskAnyQuestions(angle, "_CubemapRotationOffset");
        Debug.Log("Setting Angle Offset: " + angle);
        manager.game_event_manager.event_restart_game.Invoke();
        manager.CloseMenu();
    }
    
    public void OnButtonSettings() {
        manager.SwitchState(MenuState.Settings);
    }

    public void OnButtonCredits() {
        manager.SwitchState(MenuState.Credits);
    }

    public void OnButtonQuit() {
        manager.game_event_manager.event_quit_game.Invoke();
    }

}
