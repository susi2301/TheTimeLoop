using UnityEngine;

[ExecuteAlways]
public class TestEditorScript : MonoBehaviour
{


    public float angle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Quaternion target = Quaternion.identity;
        Quaternion curr_rot = this.transform.localRotation;

        angle = Quaternion.Angle(curr_rot, target);
    }
}
