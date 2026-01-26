using UnityEngine;

namespace srUtils.Unity
{
    public abstract class ResettableScriptableObject : ScriptableObject
    {
        public abstract void Reset();
    }
}