using UnityEngine;
using UnityEngine.Rendering;

public static class ManagerBootstraper
{

    private static bool ManagerDoesntExist<T>() where T : Object
    {
        return Object.FindFirstObjectByType<T>() == null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        var go = new GameObject("_Managers");
        Object.DontDestroyOnLoad(go);

        if(ManagerDoesntExist<FilterManager>())
        {
            FilterManager fm = go.AddComponent<FilterManager>();
            fm.m_filterChanged = new();
        }
    }
}
