using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using srUtils.Unity.Ports;
using UnityEngine;
using Object = UnityEngine.Object;

namespace srUtils.Unity
{
    [CreateAssetMenu(menuName = "Data/General/InstanceSet")]
    public class InstanceSet : ResettableScriptableObject, IEnumerable
    {
#if UNITY_EDITOR

        [SerializeField] private string description;
        [SerializeField] private List<Object> objectList = new List<Object>();
#endif
#if !UNITY_EDITOR
        private readonly HashSet<Object> objectList = new HashSet<Object>();
#endif
        [SerializeField] [CanBeNull] private IntPort countOutput;
        [CanBeNull] public event Action OnUpdated;
        public int count => objectList.Count;

        
        public void Add(Object obj)
        {
            objectList.Add(obj);
            OnUpdated?.Invoke();
            if(countOutput is not null) countOutput.value = count;
        }

        public void Remove(Object obj)
        {
            objectList.Remove(obj);
            OnUpdated?.Invoke();
            if(countOutput is not null) countOutput.value = count;
        }

        public IEnumerator<Object> GetEnumerator()
        {
            return objectList.GetEnumerator();
        }
        public IEnumerable<Object> GetEnumerable()
        {
            return objectList;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override void Reset()
        {
            objectList.Clear();
            OnUpdated = null;
        }
    }
}