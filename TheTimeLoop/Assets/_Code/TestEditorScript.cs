using UnityEngine;

[ExecuteAlways]
public class TestEditorScript : MonoBehaviour
{
    public float cam_angle = 0.0f;
    public float ray_length = 1.0f;

    public float calculated_angle;

    private void OnDrawGizmos()
    {

        Vector3 origin = this.transform.position;

        Vector3 spawn_forward = this.transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin,spawn_forward * ray_length);

        Gizmos.color = Color.green;

        Vector3 cam_forward = Quaternion.AngleAxis(cam_angle, Vector3.up) * Vector3.forward;
        cam_forward.Normalize();
        
        Gizmos.DrawRay(origin,cam_forward * ray_length);

        float angle = Vector3.SignedAngle(spawn_forward, cam_forward, Vector3.up);
        float rad = angle * Mathf.Deg2Rad;

        calculated_angle = rad;
    }
}
