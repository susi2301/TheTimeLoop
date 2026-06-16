using UnityEngine;

// @Note (Fulcrum): You cant make a Transform by yourself as in 'Trasform my_transform = new Tranform()'
// it must be a pointer to a gameobject transform component... UNITY ?? WTF!
public struct Trans {
    public Quaternion orientation;
    public Vector3 position;
    
    // @Note: Constructor so we can do this -> "Trans my_tranform = new Trans(gameobject.tranform)"
    // doing that does not heap allocate! apparently 'new' does not mean 'heap allocate' for c# structs as it would be in c++.
    public Trans(Transform transform) {
        orientation = transform.rotation;
        position = transform.position;
    }
}
