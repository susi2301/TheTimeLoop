using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Scriptable Objects/SoundsDatabase")]
public class SoundsDatabase : ScriptableObject
{
    [Tooltip("Note:\n- No dublicates pls\n- Removing entries will break scripts if they are already used.\n- Reordering does not break scrips but will mess up already assigned identifiers within this database")]
    public List<String> identifiers;

    [Tooltip("Update identifiers list so they become accessible as a selection. This will recompile scripts. Avoid Changing the order just for the sake of it")]
    public bool DEV_update_identifiers = false;
    [Tooltip("Merge dublicates and put things of same identifier into a collection if not already.")]
    public bool DEV_merge_dublicates = false;
    
    [Space]
    public List<SoundClip> sound_clips;
    public List<SoundClipCollection> sound_clip_collections;
    
    public void MergeDuplicates() {
        
        SoundClipCollection[] combined_collections = new SoundClipCollection[(int)SoundID.COUNT];

        foreach (SoundClipCollection collection in sound_clip_collections) {
            int index = (int)collection.identifier;

            if (combined_collections[index] == null) {
                combined_collections[index] = new SoundClipCollection();
                combined_collections[index].volume = collection.volume;
            }
            
            foreach (AudioClip clip in collection.clips) {
                
                if (combined_collections[index].clips == null) {
                    combined_collections[index].clips = new List<AudioClip>();
                }

                if (clip != null) {
                    combined_collections[index].clips.Add(clip);
                }
            }
        }
        
        // check if we need to merge sound clips into a collection
        // we will use the 'SoundClips' volume as a voluem for the collection
        // so we can use it later when splitting again.
        foreach (SoundClip sclip in sound_clips) {
            
            int index = (int)sclip.identifier;

            if (sclip.clip == null) {
                continue;
            }

            if (combined_collections[index] == null) {
                combined_collections[index] = new SoundClipCollection();
                combined_collections[index].volume = sclip.volume;
            }
            
            
            if (combined_collections[index].clips == null) {
                combined_collections[index].clips = new List<AudioClip>();
                combined_collections[index].volume = sclip.volume;
            }

            if (sclip.clip != null) {
                combined_collections[index].clips.Add(sclip.clip);
            }
        }
        
        sound_clips.Clear();
        sound_clip_collections.Clear();

        for (int i = 0; i < (int)SoundID.COUNT; i++) {

            SoundID identifier = (SoundID)i;
            
            if (combined_collections[i] == null || combined_collections[i].clips == null) {
                continue;
            }
            
            if (combined_collections[i].clips.Count == 0) {
                continue;
            }
            
            if (combined_collections[i].clips.Count == 1)
            {
                SoundClip s_clip = new SoundClip();
                s_clip.clip = combined_collections[i].clips[0];
                s_clip.identifier = identifier;
                s_clip.volume = combined_collections[i].volume;
                sound_clips.Add(s_clip);
                continue;
            }
             
            SoundClipCollection new_collection = new SoundClipCollection();
            new_collection.identifier = identifier;

            new_collection.clips = new List<AudioClip>();
                
            foreach (AudioClip aclip in combined_collections[i].clips) {
                new_collection.clips.Add(aclip);
            }
            
            combined_collections[i].identifier = identifier;
            sound_clip_collections.Add(combined_collections[i]);
        }

        GC.Collect();
    }

    private void OnValidate() {
        if (DEV_update_identifiers) {
            DEV_update_identifiers = false;
            #if UNITY_EDITOR
            EnumGenerator.GenerateEnum("SoundID", identifiers, "Assets/_Code/Generated/SoundID.cs");
            #endif
        }

        if (DEV_merge_dublicates) {
            DEV_merge_dublicates = false;
            MergeDuplicates();
        }
    }
}


public static class EnumGenerator
{
    public static void GenerateEnum(string enumName, List<String> field_names, string outputPath) {

        #if UNITY_EDITOR
            var sb = new StringBuilder();

            sb.AppendLine("// CODE Generated. Do not edit!");
            sb.AppendLine();
            sb.AppendLine($"public enum {enumName}");
            sb.AppendLine("{");
            sb.AppendLine("    None = 0,");


            for (int i = 0; i < field_names.Count; i++) {
                
                sb.AppendLine($"    {field_names[i]},");
            }
            
            sb.AppendLine("    COUNT,");
            sb.AppendLine("}");

            File.WriteAllText(outputPath, sb.ToString());
            
            AssetDatabase.Refresh();
        #endif
    }
}