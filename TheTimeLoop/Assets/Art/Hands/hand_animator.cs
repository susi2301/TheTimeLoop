using System;
using UnityEngine;

public class hand_animator : MonoBehaviour
{

    public Animator animator;

    public InputEvent grab_input;

    public bool is_down = false;

    private void Awake() {
        animator.SetLayerWeight(1, 0.0f);
        grab_input.Reset();
    }

    private void Update()
    {

        InputPollMode grab_poll_mode = is_down ? InputPollMode.OnRelease : InputPollMode.OnPressed;

        if (grab_input.Poll(grab_poll_mode)) {

            if (grab_poll_mode == InputPollMode.OnPressed) {
                
                animator.SetLayerWeight(1, 1.0f);
                animator.Play("right_grab");
                is_down = true;
            }
            else {
                Debug.Log("Released!");
                animator.SetLayerWeight(1, 0.0f);
                is_down = false;
            }
        }

    }
}
