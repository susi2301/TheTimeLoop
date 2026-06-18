using UnityEngine;
using UnityEngine.Events;

public class EventBroadcaster : MonoBehaviour {
    public UnityEvent game_event;
    public void Invoke() {
        game_event.Invoke();
    }
}
