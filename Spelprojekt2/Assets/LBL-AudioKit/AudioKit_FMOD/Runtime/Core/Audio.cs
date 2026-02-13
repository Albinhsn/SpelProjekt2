using UnityEngine;
using FMODUnity;

namespace AudioKit.FMOD
{
    // Team-API: en enkel "front door" in i AudioKit.
    // Alla i projektet kan använda Audio.* utan att behöva veta om SfxDirector / AudioEventHub / AudioSystem.
    public static class Audio
    {
        // SFX via AudioCueSO (rekommenderat för SFX)
        public static void Sfx(AudioCueSO cue, Vector3 pos)
        {
            if (cue == null) return;
            if (SfxDirector.I == null) return;

            SfxDirector.I.PlayCue(cue, pos);
        }

        // OneShot via EventRegistry ID (t.ex. "UI_CLICK", "SFX_DOOR_OPEN")
        public static void OneShot(string eventId, Vector3 pos)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            if (AudioEventHub.I == null) return;

            AudioEventHub.I.PlayOneShot(eventId, pos);
        }

        // OneShot direkt via EventReference (om du redan har en EventReference)
        public static void OneShot(EventReference evt, Vector3 pos, bool is2D = false)
        {
            if (evt.IsNull) return;
            if (AudioEventHub.I == null) return;

            AudioEventHub.I.PlayOneShot(evt, pos, is2D);
        }

        // Loop start/stop via EventRegistry ID (t.ex. "AMB_ROOM", "MUS_MAIN")
        public static void LoopStart(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            if (AudioEventHub.I == null) return;

            AudioEventHub.I.StartLoop(eventId);
        }

        public static void LoopStop(string eventId, bool fadeOut = true)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            if (AudioEventHub.I == null) return;

            AudioEventHub.I.StopLoop(eventId, fadeOut);
        }

        // Global parameter (key via AudioParamLibrary eller direkt FMOD-namn)
        public static void Param(string keyOrName, float value)
        {
            if (string.IsNullOrEmpty(keyOrName)) return;
            if (AudioSystem.I == null) return;

            AudioSystem.I.SetGlobalParam(keyOrName, value);
        }

        // Snapshot via id/path (t.ex. "Pause", "event:/Snapshots/Pause")
        public static void Snapshot(string idOrPath, bool enable, float fade = 0.1f)
        {
            if (string.IsNullOrEmpty(idOrPath)) return;
            if (AudioSystem.I == null) return;

            AudioSystem.I.SetSnapshot(idOrPath, enable, fade);
        }

        // Snabb-helpers för volym (0..1)
        public static void Master(float v)
        {
            if (AudioSystem.I == null) return;
            AudioSystem.I.SetMasterVolume(v);
        }

        public static void Music(float v)
        {
            if (AudioSystem.I == null) return;
            AudioSystem.I.SetMusicVolume(v);
        }

        public static void SfxVol(float v)
        {
            if (AudioSystem.I == null) return;
            AudioSystem.I.SetSfxVolume(v);
        }

        public static void Ui(float v)
        {
            if (AudioSystem.I == null) return;
            AudioSystem.I.SetUiVolume(v);
        }
    }
}
