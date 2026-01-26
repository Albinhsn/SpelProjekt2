using System;
using srUtils.Unity.Events;
using UnityEngine;

namespace srUtils.Unity
{
    public class MusicVariantController : MonoBehaviour
    {
        [SerializeField] private GlobalEventAsset[] events;
        [SerializeField] private AudioClip[] audioClips;
        private AudioSource[] audioSources;
        [SerializeField] private bool playClip0OnLoad;

        private void Awake()
        {
            audioSources = new AudioSource[events.Length];
            for (int a = 0; a < events.Length; a++)
            {
                audioSources[a] = gameObject.AddComponent<AudioSource>();
                audioSources[a].clip = audioClips[a];
                audioSources[a].volume = 0;
                audioSources[a].loop = true;
                audioSources[a].Play();
            }

            if (playClip0OnLoad) audioSources[0].volume = 1;
        }

        private void MuteAll()
        {
            foreach (AudioSource obj in audioSources)
            {
                obj.volume = 0;
            }
        }

        private Action[] trackSwitchActions;
        private void OnEnable()
        {
            trackSwitchActions = new Action[events.Length];
            for (int a = 0; a < events.Length; a++)
            {
                int a1 = a;
                trackSwitchActions[a] = () =>
                {
                    MuteAll();
                    audioSources[a1].volume = 1;
                };
                events[a].Register(trackSwitchActions[a]);
            }
        }
        private void OnDisable()
        {
            for (int a = 0; a < events.Length; a++)
            {
                events[a].Unregister(trackSwitchActions[a]);
            }
        }
        
    }
}