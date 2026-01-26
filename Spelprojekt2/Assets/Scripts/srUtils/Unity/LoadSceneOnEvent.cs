using System;
using srUtils.Unity.Events;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace srUtils.Unity
{
    public class LoadSceneOnEvent : MonoBehaviour
    {
        [SerializeField] private GlobalEventAsset trigger;
        [SerializeField] private int sceneBuildIndex;

        private void Awake()
        {
            trigger.Register(Load);
        }

        private void OnDestroy()
        {
            trigger.Unregister(Load);
        }

        private void Load() => SceneManager.LoadSceneAsync(sceneBuildIndex);
    }
}