using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundClip {
    public SoundID identifier;
    public AudioClip clip;
}

[Serializable]
public class SoundClipCollection {
    public SoundID identifier;
    public List<AudioClip> clips;

    public AudioClip GetRandomClip() {
        int r = UnityEngine.Random.Range(0, clips.Count);
        return clips[r];
    }
}

// TODO: we could encode everthing into one integer if we use 0 as non existing state. The index will be the actual ArrayIndex+1 and we use negative values for collection/not_collection state.
public struct SoundIDIndex {
    public bool exists;
    public bool is_collection;
    public int index;
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

public class SoundStateInfo {
    public SoundState state;
    public float time_accum;
    public float delay_duration;
    public float fade_duration; // can be FadeIn or FadeOut
}

public class SoundManager : MonoBehaviour {
    
    public static SoundManager instance;
    [SerializeField] private SoundsDatabase database;
    
    [SerializeField] private GameObject SoundSrcPrefab;

    public AudioMixer master_mixer;

    private List<SoundSource> sound_pool;
    
    // Indexes Into SoundDatabase
    private SoundIDIndex[] sound_indexes = new SoundIDIndex[(int)SoundID.COUNT];
    
    // These must be kept in sync!
    private List<SoundSource> sounds;
    private List<SoundStateInfo> sounds_info;

    

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this);
            return;
        }
        
        // Initialize sound_indexes lookup table into database
        database.MergeDuplicates();
        if (database.sound_clips != null) {
            for (int index = 0; index < database.sound_clips.Count; index++) {
                int id = (int)database.sound_clips[index].identifier;
                sound_indexes[id].exists = true;
                sound_indexes[id].is_collection = false;
                sound_indexes[id].index = index;
            }
        }
        if (database.sound_clip_collections != null) {
            for (int index = 0; index < database.sound_clip_collections.Count; index++) {
                int id = (int)database.sound_clip_collections[index].identifier;
                sound_indexes[id].exists = true;
                sound_indexes[id].is_collection = true;
                sound_indexes[id].index = index;
            }
        }

        sound_pool = new List<SoundSource>();
        sounds = new List<SoundSource>();
        sounds_info = new List<SoundStateInfo>();
        
        SoundPoolGrow(10);
    }

    private void Update() {
        
        Debug.Assert(sounds.Count == sounds_info.Count);
        // First clear out all stopped sounds and return to pool
        if (sounds.Count > 0) {
            // Walking backwards so we can safely remove while iterating
            for (int i = sounds.Count - 1; i >= 0; i--) {
                if (sounds_info[i].state == SoundState.IsStopped) {
                    SoundSource src = sounds[i];
                    SoundPoolReturnElem(src);
                    sounds.RemoveAt(i);
                    sounds_info.RemoveAt(i);
                }
            }
        }
        
        // Now we update all active sounds.
        if (sounds.Count > 0) {
            
            for (int i = 0; i < sounds.Count; i++) {

                switch (sounds_info[i].state) {
                    case SoundState.Playing:
                        if (!sounds[i].sound.isPlaying) {
                            sounds[i].sound.Stop();
                            sounds_info[i].state = SoundState.IsStopped;
                        }
                        break;
                    case SoundState.Paused:
                        break;
                    case SoundState.FadingIn:
                        sounds_info[i].time_accum += Time.deltaTime;

                        float percent_in = Mathf.Clamp(sounds_info[i].time_accum, 0.0f, sounds_info[i].fade_duration) / sounds_info[i].fade_duration;
                        sounds[i].sound.volume = percent_in;
                        
                        if (sounds_info[i].time_accum >= sounds_info[i].fade_duration) {
                            sounds_info[i].state = SoundState.Playing;
                            sounds[i].sound.volume = 1.0f;
                        }
                        
                        break;
                    case SoundState.FadingOut:
                        sounds_info[i].time_accum += Time.deltaTime;

                        float percent_out = 1.0f - Mathf.Clamp(sounds_info[i].time_accum, 0.0f, sounds_info[i].fade_duration) / sounds_info[i].fade_duration;
                        sounds[i].sound.volume = percent_out;
                        
                        if (sounds_info[i].time_accum >= sounds_info[i].fade_duration) {
                            sounds_info[i].state = SoundState.IsStopped;
                            sounds[i].sound.Stop();
                            sounds[i].sound.volume = 0.0f;
                        }
                        break;
                    case SoundState.DelayedForPlay:
                        
                        sounds_info[i].time_accum += Time.deltaTime;

                        if (sounds_info[i].time_accum >= sounds_info[i].delay_duration) {
                            sounds_info[i].time_accum = 0.0f;
                            
                            if (sounds_info[i].fade_duration > 0.0f) {
                                sounds_info[i].state = SoundState.FadingIn;
                                sounds[i].sound.volume = 0.0f;
                            }
                            else {
                                sounds_info[i].state = SoundState.Playing;
                            }
                            sounds[i].sound.Play();
                        }
                        break;
                    case SoundState.DelayedForStop:
                        
                        sounds_info[i].time_accum += Time.deltaTime;

                        if (sounds_info[i].time_accum >= sounds_info[i].delay_duration) {
                            sounds_info[i].time_accum = 0.0f;
                            
                            if (sounds_info[i].fade_duration > 0.0f) {
                                sounds_info[i].state = SoundState.FadingOut;
                            } else {
                              sounds[i].sound.Stop();
                              sounds_info[i].state = SoundState.IsStopped;
                            }
                        }
                        break;
                }
            }
        }
    }

    private SoundSource PlaySoundAtInternal(PlayInitInfo init_info) {

        SoundIDIndex id_index = sound_indexes[(int)init_info.id];

        if (!id_index.exists) {
            return null;
        }
        
        SoundSource sound_source = SoundPoolRequestElem();
        
        if (id_index.is_collection) {
            sound_source.sound.clip = database.sound_clip_collections[id_index.index].GetRandomClip();
        } else {
            sound_source.sound.clip = database.sound_clips[id_index.index].clip;
        }
        
        sound_source.transform.position = init_info.postion;
        sound_source.sound.loop = init_info.is_looping;
        sound_source.sound.maxDistance = init_info.max_distance;
        sound_source.gameObject.SetActive(true);
        sound_source.sound.volume = 1.0f;

        SoundStateInfo state_info = new SoundStateInfo();
        state_info.time_accum     = 0.0f;
        state_info.fade_duration  = init_info.fade_in_duration;
        state_info.delay_duration = init_info.delay;

        SoundState state = SoundState.Playing;
        if (init_info.delay > 0.0f) {
            state = SoundState.DelayedForPlay;
        } else if (init_info.fade_in_duration > 0.0f) {
            state = SoundState.FadingIn;
            sound_source.sound.volume = 0.0f;
        }

        state_info.state = state;
        
        sounds.Add(sound_source);
        sounds_info.Add(state_info);

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
        
        // @Note: we could work this ID's and dont loop here but then we would have to make sure that ids are stable 
        // which would mean we would need to implement some kind of free list. Since there wont be that many sounds at a time it should be fine to just search.
        int arr_index = -1;
        for (int i = 0; i < sounds.Count; i++) {

            if (sound_source == sounds[i]) {
                arr_index = i;
                break;
            }
        }
        Debug.Assert(arr_index != -1);

        sounds_info[arr_index].time_accum = 0.0f;
        sounds_info[arr_index].fade_duration = fade_out_duration;
        sounds_info[arr_index].delay_duration = delay;

        SoundState state = SoundState.IsStopped;
        if (delay > 0.0) {
            state = SoundState.DelayedForStop;
        } else if (fade_out_duration > 0.0) {
            state = SoundState.FadingOut;
        }

        sounds_info[arr_index].state = state;

        if (state == SoundState.IsStopped) {
            sound_source.sound.Stop();
        }
    }

    public SoundSource SoundPoolRequestElem() {

        if (sound_pool.Count == 0) {
            SoundPoolGrow(2);    
        }

        int last_item = sound_pool.Count - 1;
        
        SoundSource last = sound_pool[last_item];
        sound_pool.RemoveAt(last_item);
        last.gameObject.SetActive(true);
        
        return last;
    }

    public void SoundPoolReturnElem(SoundSource sound_source) {
        sound_source.sound.clip = null;
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
}

