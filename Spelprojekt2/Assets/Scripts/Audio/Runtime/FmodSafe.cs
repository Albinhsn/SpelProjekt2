using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace SP2.Audio
{
    public static class FmodSafe
    {
        private static readonly Dictionary<string, bool> s_globalParamExists = new();
        private static readonly Dictionary<string, bool> s_eventExists = new();

        public static bool GlobalParameterExists(string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
                return false;

            if (s_globalParamExists.TryGetValue(paramName, out bool cached))
                return cached;

            var res = RuntimeManager.StudioSystem.getParameterDescriptionByName(paramName, out _);
            bool ok = (res == FMOD.RESULT.OK);

            s_globalParamExists[paramName] = ok;
            return ok;
        }

        public static bool EventExists(EventReference evt)
        {
            if (evt.IsNull)
                return false;

            string key = evt.Guid.ToString();
            if (s_eventExists.TryGetValue(key, out bool cached))
                return cached;

            var res = RuntimeManager.StudioSystem.getEventByID(evt.Guid, out FMOD.Studio.EventDescription desc);
            bool ok = (res == FMOD.RESULT.OK) && desc.isValid();

            s_eventExists[key] = ok;
            return ok;
        }

        public static void SetGlobalParameterSafe(string paramName, float value)
        {
            if (string.IsNullOrEmpty(paramName))
                return;

            if (!GlobalParameterExists(paramName))
                return;

            RuntimeManager.StudioSystem.setParameterByName(paramName, value);
        }

        public static void PlayOneShotSafe(EventReference evt, Vector3 pos)
        {
            if (evt.IsNull)
                return;

            if (!EventExists(evt))
                return;

            RuntimeManager.PlayOneShot(evt, pos);
        }

        public static void PlayOneShot2DSafe(EventReference evt)
        {
            if (evt.IsNull)
                return;

            if (!EventExists(evt))
                return;

            RuntimeManager.PlayOneShot(evt);
        }

        public static void ClearCaches()
        {
            s_globalParamExists.Clear();
            s_eventExists.Clear();
        }
    }
}
