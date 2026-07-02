using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;


[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Scriptable Objects/SoundsDatabase")]
public class SoundsDatabase : ScriptableObject
{
    [Tooltip("Note:\n- No dublicates pls\n- Removing entries will break scripts if they are already used.\n- Reordering does not break scrips but will mess up already assigned identifiers within this database")]
    public List<String> identifiers;

    [Tooltip("Update identifiers list so they become accessible as a selection. This will recompile scripts. Avoid Changing the order just for the sake of it")]
    public bool DEV_update_identifiers = false;
    [Tooltip("Merge dublicates and put things of same identifier into a collection if not already.")]
    public bool DEV_merge_dublicates = false;
    public bool DEV_update_mixer_group_identifiers = false;

    public AudioMixer master_mixer = null;

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
                combined_collections[index].mixer_group = collection.mixer_group;
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
        // When we first construct a new Collection here we will use the current SoundClip's
        // volume and mixer group to construct it.
        foreach (SoundClip sclip in sound_clips) {
            
            int index = (int)sclip.identifier;

            if (sclip.clip == null) {
                continue;
            }

            if (combined_collections[index] == null) {
                combined_collections[index] = new SoundClipCollection();
                combined_collections[index].volume = sclip.volume;
                combined_collections[index].mixer_group = sclip.mixer_group;
            }
            
            
            if (combined_collections[index].clips == null) {
                combined_collections[index].clips = new List<AudioClip>();
                combined_collections[index].volume = sclip.volume;
                combined_collections[index].mixer_group = sclip.mixer_group;
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
            
            if (combined_collections[i].clips.Count == 1) {
                SoundClip s_clip = new SoundClip();
                s_clip.clip         = combined_collections[i].clips[0];
                s_clip.identifier   = identifier;
                s_clip.volume       = combined_collections[i].volume;
                s_clip.mixer_group  = combined_collections[i].mixer_group;
                sound_clips.Add(s_clip);
                continue;
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

        if (DEV_update_mixer_group_identifiers) {
            DEV_update_mixer_group_identifiers = false;
            #if UNITY_EDITOR
            
            if (master_mixer != null){
                AudioMixerGroup []all_groups = master_mixer.FindMatchingGroups(string.Empty);
                List<string> group_names = new List<string>();
                for (int i = 0; i < all_groups.Length; i ++){
                    group_names.Add(all_groups[i].name);

                }
                EnumGenerator.GenerateEnum("SoundMixerGroup", group_names, "Assets/_Code/Generated/SoundMixerGroup.cs");
            }

            #endif
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