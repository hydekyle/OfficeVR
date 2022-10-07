using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioManager
{
    public AudioMixerGroup mixerSFX;
    public AudioSource audioMusic;
    Dictionary<int, AudioSource> audioSources = new Dictionary<int, AudioSource>();

    public void StopMusic()
    {
        audioMusic.Stop();
    }

    public void PlaySound(AudioClip soundClip, GameObject emitter = null, bool isLoop = false, float volume = 1f, float spatialBlend = 1f, float stereoPan = 0f, float pitch = 1f)
    {
        if (emitter == null)
        {
            PlaySound(soundClip, Camera.main.gameObject, isLoop, spatialBlend);
        }
        else if (emitter.TryGetComponent<AudioSource>(out AudioSource audioSource))
        {
            Play(audioSource, soundClip, emitter, isLoop, volume, spatialBlend, stereoPan, pitch);
        }
        else
        {
            var newAudioSource = emitter.AddComponent<AudioSource>();
            Play(newAudioSource, soundClip, emitter, isLoop, volume, spatialBlend, stereoPan, pitch);
        }

        void Play(AudioSource audioSource, AudioClip soundClip, GameObject emitter = null, bool isLoop = false, float volume = 1f, float spatialBlend = 1f, float stereoPan = 0f, float pitch = 1f)
        {
            audioSource.outputAudioMixerGroup = mixerSFX;
            audioSource.loop = isLoop;
            audioSource.spatialBlend = spatialBlend;
            audioSource.volume = volume;
            audioSource.panStereo = stereoPan;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(soundClip);
        }
    }

    /// <summary>
    /// Creates a GameObject for each emmiter so sounds still playing even if the GameObject is disabled
    /// only use this function if the gameobject can be disabled while playing sound since it produce garbage
    /// </summary>
    //TODO: 
    public void PlaySoundFromGameobjectDisabled(AudioClip soundClip, GameObject emitter)
    {
        var emitterID = emitter.GetHashCode();
        if (audioSources.ContainsKey(emitterID))
        {
            var audioSource = audioSources[emitterID];
            audioSource.transform.position = emitter.transform.position;
            audioSource.PlayOneShot(soundClip);
        }
        else
        {
            var go = new GameObject();
            go.transform.SetParent(GameManager.Instance.transform);
            var newAudioSource = go.AddComponent<AudioSource>();
            newAudioSource.outputAudioMixerGroup = mixerSFX;
            Debug.Log(mixerSFX);
            go.transform.position = emitter.transform.position;
            audioSources[emitterID] = newAudioSource;
            newAudioSource.PlayOneShot(soundClip);
        }
    }
}
