using UnityEngine;

public class UIState_Controls : MonoBehaviour {
    
    public MenuManager manager;
    public GameObject panel_go;

    public void EnterState() {
        panel_go.SetActive(true);
    }

    public void ExitState() {
        panel_go.SetActive(false);
    }
}
