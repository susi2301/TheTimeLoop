using UnityEngine;
using UnityEngine.Events;

public class EventBroadcaster : MonoBehaviour {
    public UnityEvent game_event;
    public void Invoke() {
        game_event.Invoke();
    }

    [SerializeField] private bool DEV_trigger_event = false;
    private void OnValidate() {
        if (DEV_trigger_event){
            Invoke();
            DEV_trigger_event = false;
        }
    }
}
