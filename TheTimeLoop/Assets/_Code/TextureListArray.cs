using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TextureListArray", menuName = "Scriptable Objects/TextureListArray")]
public class TextureListArray : ScriptableObject
{
    public List<Texture2D> texture_list = new List<Texture2D>();
}
