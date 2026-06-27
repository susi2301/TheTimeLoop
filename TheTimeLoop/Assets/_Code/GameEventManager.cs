using UnityEngine;

public class GameEventManager : MonoBehaviour {
   
   public EventBroadcaster event_restart_game;
   public EventBroadcaster event_menu_opened;
   public EventBroadcaster event_menu_closed;
   public EventBroadcaster event_quit_game;

   public EventBroadcaster event_clock_broken;
   public EventBroadcaster event_clock_fixed;
   
   public EventBroadcaster event_break_transition_start;
}
