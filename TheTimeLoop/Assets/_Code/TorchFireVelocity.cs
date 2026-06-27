using UnityEngine;

// Unused ATM ! Half broken when object is a child of another ?

[ExecuteAlways]
public class TorchFireVelocity : MonoBehaviour {

    public MeshRenderer meshren;
    public Rigidbody rigid;

    public float drag;
    public Vector3 curr_velocity;
    private Vector3 last_pos;

    public float drg;

    void FixedUpdate() {

        if (meshren == null){
            return;
        }



        Vector3 curr_pos = this.transform.position;
        //Vector3 vel = (last_pos - curr_pos) * Time.deltaTime * 100.0f;
        last_pos = curr_pos;

        Vector3 vel = rigid.linearVelocity;


        curr_velocity += vel;

        meshren.sharedMaterial.SetFloat("_vel_x", curr_velocity.x);
        meshren.sharedMaterial.SetFloat("_vel_y", curr_velocity.y);
        meshren.sharedMaterial.SetFloat("_vel_z", curr_velocity.z);
        
        float d = drag * Time.deltaTime;
        drg = d;

        curr_velocity *= d;

        
    }
}
