using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Flcrm;

[Serializable]
public class SoundClip {
    public SoundID identifier;
    public AudioClip clip;
    public SoundMixerGroup mixer_group = SoundMixerGroup.Master;
    [Range(0.0f, 1.0f)] public float volume = 1.0f;
}

[Serializable]
public class SoundClipCollection {
    public SoundID identifier;
    public List<AudioClip> clips;
    public SoundMixerGroup mixer_group = SoundMixerGroup.Master;
    [Range(0.0f, 1.0f)] public float volume = 1.0f;

    private int next_clip = 0;

    public AudioClip GetRandomClip() {
        if (clips.Count == 0){
            return null;
        }
        int r = UnityEngine.Random.Range(0, clips.Count);
        return clips[r];
    }

    public AudioClip GetNextClip() {
        if (clips.Count == 0){
            return null;
        }

        if (next_clip >= clips.Count) {
            next_clip = 0;
        }

        AudioClip clip = clips[next_clip];
        next_clip = (next_clip + 1) % clips.Count;
        return clips[next_clip];
    }
}

// @Performance: we could encode everthing into one integer if we use 0 as non existing state. 
// The index will be the actual ArrayIndex+1 and we use negative values for collection/not_collection state.
public struct SoundIDIndex {
    public bool exists;
    public bool is_collection;
    public int  array_index;
}


public struct PlayInitInfo {
    public SoundID id;
    public Vector3 postion;
    public float delay;
    public float fade_in_duration;
    public float max_distance;
    public bool is_looping;
}

public enum SoundState {
    Playing,
    Paused,
    FadingIn,
    FadingOut,
    DelayedForPlay,
    DelayedForStop,
    IsStopped,
}

// @Performnace: 'sound_base_volume' messes up our cachline boundry.
public struct SoundStateInfo {
    public SoundState state;
    public float time_accum;
    public float delay_duration;
    public float fade_duration; // can be FadeIn or FadeOut
    public float sound_base_volume;
}

public class SoundManager : MonoBehaviour {
    
    public static SoundManager instance;
    
    [SerializeField] private SoundsDatabase database;
    
    [SerializeField] private GameObject SoundSrcPrefab;

    public GameSettingsSO game_settings;
    public AudioMixer master_mixer;

    private List<SoundSource> sound_pool;
    
    // Lookup info into databse
    private SoundIDIndex[] sound_indexes; // = new SoundIDIndex[(int)SoundID.COUNT];
    
    // These must be kept in sync!
    private List<SoundSource>    active_sounds;
    private List<SoundStateInfo> active_sounds_info;
    
    private AudioMixerGroup[] mixer_groups; // Has length of SoundMixerGroup.COUNT  // = new AudioMixerGroup[(int)SoundMixerGroup.COUNT];

    private void Awake() {
        InitInstance();
    }

    private void InitInstance(){
        
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this);
            return;
        }

        sound_indexes = new SoundIDIndex[(int)SoundID.COUNT];

        // Initialize sound_indexes lookup table into database
        database.MergeDuplicates();

        if (database.sound_clips != null) {
            
            for (int index = 0; index < database.sound_clips.Count; index++) {
                
                int id = (int)database.sound_clips[index].identifier;
                sound_indexes[id].exists = true;
                sound_indexes[id].is_collection = false;
                sound_indexes[id].array_index = index;

            }
        }

        if (database.sound_clip_collections != null) {
            for (int index = 0; index < database.sound_clip_collections.Count; index++) {
                int id = (int)database.sound_clip_collections[index].identifier;
                sound_indexes[id].exists = true;
                sound_indexes[id].is_collection = true;
                sound_indexes[id].array_index = index;
            }
        }

        sound_pool          = new List<SoundSource>();
        active_sounds       = new List<SoundSource>();
        active_sounds_info  = new List<SoundStateInfo>();        
        
        mixer_groups = new AudioMixerGroup[(int)SoundMixerGroup.COUNT];

        AudioMixerGroup[] all_groups = master_mixer.FindMatchingGroups(string.Empty);
        if (all_groups != null && all_groups.Length > 0){

            for (int i = 0; i < (int)SoundMixerGroup.COUNT; i++){

                SoundMixerGroup group_enum = (SoundMixerGroup)i;

                string group_enum_str = group_enum.ToString();
                for (int g = 0; g < all_groups.Length; g++){
                    if (group_enum_str == all_groups[g].name){
                        mixer_groups[i] = all_groups[g];
                        break;
                    }
                }
            }    
        }
        
        SoundPoolGrow(16);
    }


    public void Init(){
        AdjustGlobalVolume(game_settings.global_volume);
    }

    private void Update() {
        
        Debug.Assert(active_sounds.Count == active_sounds_info.Count);
        // First clear out all stopped sounds and return to pool
        if (active_sounds.Count > 0) {
            // Walking backwards so we can safely remove while iterating
            for (int i = active_sounds.Count - 1; i >= 0; i--) {
                if (active_sounds_info[i].state == SoundState.IsStopped) {
                    SoundSource src = active_sounds[i];
                    SoundPoolReturnElem(src);
                    active_sounds.RemoveAt(i);
                    active_sounds_info.RemoveAt(i);
                }
            }
        }
        
        // Now we update all active sounds.
        if (active_sounds.Count > 0) {
            
            for (int i = 0; i < active_sounds.Count; i++) {

                SoundStateInfo s_info = active_sounds_info[i];

                if (active_sounds[i].follow_target != null){
                    active_sounds[i].transform.position = active_sounds[i].follow_target.position;
                }

                switch (s_info.state) {
                    case SoundState.Playing: {
                        if (!active_sounds[i].sound.isPlaying) {
                            active_sounds[i].sound.Stop();
                            s_info.state = SoundState.IsStopped;
                        }
                        break;
                    }
                    case SoundState.Paused: break;
                    case SoundState.FadingIn: {
                        s_info.time_accum += Time.deltaTime;

                        float percent_in = Mathf.Clamp(s_info.time_accum, 0.0f, s_info.fade_duration) / s_info.fade_duration;
                        

                        active_sounds[i].sound.volume = s_info.sound_base_volume * percent_in;
                        
                        if (s_info.time_accum >= s_info.fade_duration) {
                            s_info.state = SoundState.Playing;
                            active_sounds[i].sound.volume = s_info.sound_base_volume;
                        }
                        
                        break;
                    }
                    case SoundState.FadingOut:{

                        s_info.time_accum += Time.deltaTime;

                        float percent_out = 1.0f - Mathf.Clamp(s_info.time_accum, 0.0f, s_info.fade_duration) / s_info.fade_duration;
                        active_sounds[i].sound.volume = s_info.sound_base_volume * percent_out;
                        
                        if (s_info.time_accum >= s_info.fade_duration) {
                            s_info.state = SoundState.IsStopped;
                            active_sounds[i].sound.Stop();
                            active_sounds[i].sound.volume = 0.0f;
                        }
                        break;
                    }
                    case SoundState.DelayedForPlay: {                        
                        s_info.time_accum += Time.deltaTime;

                        if (s_info.time_accum >= s_info.delay_duration) {
                            s_info.time_accum = 0.0f;
                            
                            if (s_info.fade_duration > 0.0f) {
                                s_info.state = SoundState.FadingIn;
                                active_sounds[i].sound.volume = 0.0f;
                            }
                            else {
                                s_info.state = SoundState.Playing;
                            }
                            active_sounds[i].sound.Play();
                        }
                        break;
                    }
                    case SoundState.DelayedForStop: {                        
                        s_info.time_accum += Time.deltaTime;

                        if (s_info.time_accum >= s_info.delay_duration) {
                            s_info.time_accum = 0.0f;
                            
                            if (s_info.fade_duration > 0.0f) {
                                s_info.state = SoundState.FadingOut;
                            } else {
                              active_sounds[i].sound.Stop();
                              s_info.state = SoundState.IsStopped;
                            }
                        }
                        break;
                    }
                }

                active_sounds_info[i] = s_info;
            }
        }
    }


    private SoundSource PlaySoundAtInternal(PlayInitInfo init_info) {

        SoundIDIndex id_index = sound_indexes[(int)init_info.id];

        if (!id_index.exists) {
            return null;
        }
        
        SoundSource sound_source = SoundPoolRequestElem();
        
        SoundStateInfo state_info = new SoundStateInfo();
        state_info.time_accum     = 0.0f;
        state_info.fade_duration  = init_info.fade_in_duration;
        state_info.delay_duration = init_info.delay;
        state_info.sound_base_volume = 1.0f;

        SoundMixerGroup mix_group_id = SoundMixerGroup.Master;
        if (id_index.is_collection) {
            sound_source.sound.clip      = database.sound_clip_collections[id_index.array_index].GetRandomClip();
            state_info.sound_base_volume = database.sound_clip_collections[id_index.array_index].volume;
            mix_group_id = database.sound_clip_collections[id_index.array_index].mixer_group;

        } else {
            sound_source.sound.clip = database.sound_clips[id_index.array_index].clip;
            state_info.sound_base_volume = database.sound_clips[id_index.array_index].volume;
            mix_group_id = database.sound_clips[id_index.array_index].mixer_group;
        }
        
        sound_source.follow_target = null;
        sound_source.transform.position = init_info.postion;
        sound_source.sound.loop = init_info.is_looping;
        sound_source.sound.maxDistance = init_info.max_distance;
        sound_source.gameObject.SetActive(true);
        sound_source.sound.volume = state_info.sound_base_volume;
        sound_source.sound.outputAudioMixerGroup = mixer_groups[(int)mix_group_id];

        SoundState state = SoundState.Playing;
        if (init_info.delay > 0.0f) {
            state = SoundState.DelayedForPlay;
        } else if (init_info.fade_in_duration > 0.0f) {
            state = SoundState.FadingIn;
            sound_source.sound.volume = 0.0f;
        }

        state_info.state = state;
        
        active_sounds.Add(sound_source);
        active_sounds_info.Add(state_info);

        if (state == SoundState.Playing) {
            sound_source.sound.Play();
        }

        return sound_source;
    }
    
    public void PlaySoundAt(SoundID id, Vector3 position, float delay = 0.0f, float fade_in_duration = 0.0f, float max_distance = 5.0f) {
        PlayInitInfo init_info = new PlayInitInfo();
        init_info.id = id;
        init_info.postion = position;
        init_info.delay = delay;
        init_info.fade_in_duration = fade_in_duration;
        init_info.max_distance = max_distance;
        init_info.is_looping = false;
        SoundSource src = PlaySoundAtInternal(init_info);
    }
    
    // Can return null if sound doesn't exist!
    public SoundSource PlaySoundLoopingAt(SoundID id, Vector3 position, float delay = 0.0f, float fade_in_duration = 0.0f, float max_distance = 5.0f) {

        PlayInitInfo init_info = new PlayInitInfo();
        init_info.id = id;
        init_info.postion = position;
        init_info.delay = delay;
        init_info.fade_in_duration = fade_in_duration;
        init_info.max_distance = max_distance;
        init_info.is_looping = true;
        SoundSource src = PlaySoundAtInternal(init_info);
        
        return src;
    }
    
    // Callers Sound source is no longer valid after calling this.
    public void StopAndReturnSoundSource(SoundSource sound_source,  float delay = 0.0f, float fade_out_duration = 0.0f) {
        
        if (sound_source == null) {
            return;
        }
        
        // Find which sound source is being returned.
        // @Note: we could work with ID's and avoid linear search here but then we would have to make sure that ids are stable 
        // which would mean we would need to implement some kind of free list or hash map.
        // Since there wont be that many sounds playing at a time it should be fine to just search.
        int arr_index = -1;
        for (int i = 0; i < active_sounds.Count; i++) {

            if (sound_source == active_sounds[i]) {
                arr_index = i;
                break;
            }
        }
        Debug.Assert(arr_index != -1); // if we dont find it, its a bug and panic

        SoundStateInfo s_info = active_sounds_info[arr_index];
        s_info.time_accum     = 0.0f;
        s_info.delay_duration = delay;
        s_info.fade_duration  = fade_out_duration;

        // Check if we stop immidiatly or delay or fade out.
        SoundState state = SoundState.IsStopped;
        if (delay > 0.0f) {
            state = SoundState.DelayedForStop;
        } else if (fade_out_duration > 0.0f) {
            state = SoundState.FadingOut;
        }

        s_info.state = state;

        if (state == SoundState.IsStopped) {
            sound_source.sound.Stop();
            sound_source.follow_target = null;
        }

        active_sounds_info[arr_index] = s_info;
    }

    public SoundSource SoundPoolRequestElem() {

        if (sound_pool.Count == 0) {
            SoundPoolGrow(2);    
        }

        // @Note: Apparantly 'List' in C# doesn't have a pop function... that what we're doing here..
        int last_elem = sound_pool.Count - 1;
        
        SoundSource sound_source = sound_pool[last_elem];
        sound_pool.RemoveAt(last_elem);
        sound_source.gameObject.SetActive(true);
        
        return sound_source;
    }

    public void SoundPoolReturnElem(SoundSource sound_source) {
        sound_source.sound.clip = null;
        sound_source.sound.outputAudioMixerGroup = null;
        sound_source.follow_target = null;
        sound_source.gameObject.SetActive(false);
        sound_pool.Add(sound_source);
    }
    
    
    public void SoundPoolGrow(int amount) {

        for (int i = 0; i < amount; i++) {
            GameObject go = Instantiate(SoundSrcPrefab, this.transform);
            SoundSource sound_src = go.GetComponent<SoundSource>();
            sound_pool.Add(sound_src);
            go.SetActive(false);
        }
    }

    public void AdjustGlobalVolume(float new_volume_0_to_100){

        // Remap volume from range 0..100%  to range of -80 .. +20 decibel attenuation.
        // where 50% equals 0 db attenuation.
        float max_atten_db = 20.0f;
        float min_atten_db = -80.0f;

        // First clamp and normalize input volume to 0..1 range;
        float vol = Mathf.Clamp(new_volume_0_to_100, 0.0f, 100.0f);
        float vol_01 = vol / 100.0f;


        float vol_atten_db = 0.0f;
        if (vol_01 > 0.5f) {
            vol_atten_db = Mathy.remap(0.5f, 1.0f, 0.0f, max_atten_db, vol_01);
        } else {
            vol_atten_db = Mathy.remap(0.0f, 0.5f, min_atten_db, 0.0f, vol_01);
        }

        master_mixer.SetFloat("MasterVolume", vol_atten_db);
        game_settings.global_volume = vol;
    }
}

