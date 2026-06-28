//#define DEVELOPMENT
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if DEVELOPMENT

public class TextureArrayBuilder : EditorWindow
{
    [MenuItem("Tools/Texture Array Builder")]
    static void ShowWindow()
    {
        GetWindow<TextureArrayBuilder>("Texture Array Builder");
    }

    string outputPath = "Assets/Art/VFX/FireVolume.asset";
    string so_output  = "Assets/Art/VFX/TexArray.asset";
    void OnGUI()
    {
        GUILayout.Label("Build Texture2DArray from selected textures");

        outputPath = EditorGUILayout.TextField("Output path",outputPath);
        so_output  = EditorGUILayout.TextField("SO Path", so_output);

        if (GUILayout.Button("Build Texture Array"))
        {
            Build();
        }

        if (GUILayout.Button("Make SO From Selection")) {
            MakeSO();
        }
    }

    void Build()
    {
        Object[] selection = Selection.objects;

        //Texture2D[] textures = selection.OfType<Texture2D>().OrderBy(t => t.name).ToArray();
        TextureListArray[] tex_list_so_arr = selection.OfType<TextureListArray>().ToArray();


        if (tex_list_so_arr.Length == 0)
        {
            Debug.LogError("No texture list Scriptable Object selected.");
            return;
        }

        TextureListArray tex_list_so = tex_list_so_arr[0];

        if (tex_list_so.texture_list.Count == 0){
            Debug.LogError("No texture in list Scriptable Object selected.");
            return;
        }

        Texture2D first = tex_list_so.texture_list[0];

        int width = first.width;
        int height = first.height;
        TextureFormat format = first.format;

        // Validating Textures
        bool error_found = false;
        for (int i = 0; i < tex_list_so.texture_list.Count; i++) {

            if (tex_list_so.texture_list[i].width != width || tex_list_so.texture_list[i].height != height ||tex_list_so.texture_list[i].format != format) {
                Debug.LogError($"Texture {tex_list_so.texture_list[i].name} doesn't match dimensions/format.");
                error_found = true;
            }
        }

        if (error_found){
            return;
        }


        Texture2DArray array = new Texture2DArray(
            width,
            height,
            tex_list_so.texture_list.Count,
            format,
            true);

        array.wrapMode = TextureWrapMode.Clamp;
        array.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < tex_list_so.texture_list.Count; i++)
        {
            Graphics.CopyTexture(
                tex_list_so.texture_list[i],
                0,
                0,
                array,
                i,
                0);
        }

        AssetDatabase.CreateAsset(array, outputPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created Texture2DArray with {tex_list_so.texture_list.Count} slices.");
    }

    void MakeSO() {


        Object[] selection = Selection.objects;

        Texture2D[] textures = selection.OfType<Texture2D>().OrderBy(t => t.name).ToArray();
        
        if (textures.Length == 0){
            return;
        }
        
        ScriptableObject nonGenericInstance = ScriptableObject.CreateInstance(typeof(TextureListArray));
        TextureListArray tex_arr = (TextureListArray)nonGenericInstance;

        for (int i = 0; i < textures.Length; i++){

            tex_arr.texture_list.Add(textures[i]);
        }

        //TextureListArray[] tex_list_so_arr = selection.OfType<TextureListArray>().ToArray();



        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(so_output);

        AssetDatabase.CreateAsset(tex_arr, name);
        AssetDatabase.SaveAssets();
    }
}

#endif