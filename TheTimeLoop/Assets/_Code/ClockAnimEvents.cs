using UnityEngine;
using UnityEngine.Events;




public class ClockAnimEvents : MonoBehaviour {

    public UnityEvent event_break_anim_finished;
    
    public void TestAnimEvent(int no_need) {
        event_break_anim_finished.Invoke();
    }
}
