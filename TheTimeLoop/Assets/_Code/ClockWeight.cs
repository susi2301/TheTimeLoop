using UnityEngine;

public class ClockWeight : MonoBehaviour {
    
    public MenuMatSwitcher mat_switcher;
    public GrabableEffectiveChildOf child_of;

    [HideInInspector] public int ID;

    public void SpawnAt(Transform trans) {
        this.gameObject.SetActive(true);
        transform.position = trans.position;
        transform.rotation = trans.rotation;
    }

    public void Despawn() {
        this.gameObject.SetActive(false);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    public bool IsSpawned() {
        return this.gameObject.activeSelf;
    }


    public void OnMenuOpen() {
        if (child_of.IsGrabbed()) {
            mat_switcher.SwitchToMenuMat();
        }
    }

    public void OnMenuClose() {
        
        mat_switcher.SwitchToInGameMat();
    }
}
