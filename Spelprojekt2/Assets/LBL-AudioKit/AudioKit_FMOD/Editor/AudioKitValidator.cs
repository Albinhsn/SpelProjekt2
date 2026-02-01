#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// AudioKit anteckning
// Enkel validering av AudioKit-setup.
// Körs från menyn: AudioKit/Verktyg/Validera AudioKit

namespace AudioKit.FMOD
{
    public static class AudioKitValidator
    {
        [MenuItem("AudioKit/Verktyg/Validera AudioKit")]
        public static void ValidateAudioKit()
        {
            var problems = new List<string>(32);

            // Resources assets
            var cfg = AudioResources.Config;
            var prm = AudioResources.ParamLibrary;
            var reg = AudioResources.EventRegistry;

            if (cfg == null)
                problems.Add("Saknar Resources/Audio/AudioConfig.asset (AudioConfigSO)");
            if (prm == null)
                problems.Add("Saknar Resources/Audio/AudioParamLibrary.asset (AudioParamLibrarySO)");
            if (reg == null)
                problems.Add("Saknar Resources/Audio/EventRegistry.asset (AudioEventRegistrySO)");

            // Param library checks
            if (prm != null && prm.entries != null)
            {
                var seen = new HashSet<string>();
                for (int i = 0; i < prm.entries.Length; i++)
                {
                    var e = prm.entries[i];
                    if (string.IsNullOrWhiteSpace(e.key)) continue;
                    var k = e.key.Trim();
                    if (!seen.Add(k))
                        problems.Add($"AudioParamLibrary: duplicerad key '{k}' (index {i})");

                    if (string.IsNullOrWhiteSpace(e.fmodName))
                        problems.Add($"AudioParamLibrary: key '{k}' saknar fmodName (index {i})");
                }
            }

            // Event registry checks
            if (reg != null && reg.events != null)
            {
                var seen = new HashSet<string>();
                for (int i = 0; i < reg.events.Length; i++)
                {
                    var e = reg.events[i];
                    if (e == null) continue;
                    if (string.IsNullOrWhiteSpace(e.id)) continue;

                    var id = e.id.Trim();
                    if (!seen.Add(id))
                        problems.Add($"EventRegistry: duplicerad id '{id}' (index {i})");
                    if (e.evt.IsNull)
                        problems.Add($"EventRegistry: id '{id}' saknar EventReference (index {i})");
                }
            }

            // Config sanity
            if (cfg != null)
            {
                void CheckPath(string label, string v)
                {
                    if (string.IsNullOrWhiteSpace(v))
                    {
                        problems.Add($"AudioConfig: '{label}' är tom (kan leda till tyst ljud)");
                        return;
                    }

                    var t = v.Trim();
                    if (!t.StartsWith("vca:/") && !t.StartsWith("bus:/") && !t.StartsWith("event:/") && !t.StartsWith("snapshot:/"))
                        problems.Add($"AudioConfig: '{label}' ser inte ut som en FMOD-path: '{t}'");
                }

                CheckPath("VCA Master", cfg.vcaMaster);
                CheckPath("VCA Music", cfg.vcaMusic);
                CheckPath("VCA SFX", cfg.vcaSfx);
                CheckPath("VCA UI", cfg.vcaUi);

                if (!string.IsNullOrWhiteSpace(cfg.masterBus) && !cfg.masterBus.Trim().StartsWith("bus:/"))
                    problems.Add($"AudioConfig: MasterBus bör vara 'bus:/...' men är '{cfg.masterBus}'");

                // Snapshots (valfritt)
                if (!string.IsNullOrWhiteSpace(cfg.pauseSnapshotPath))
                    CheckPath("Pause Snapshot", cfg.pauseSnapshotPath);
                if (!string.IsNullOrWhiteSpace(cfg.damageSnapshotPath))
                    CheckPath("Damage Snapshot", cfg.damageSnapshotPath);
            }

            if (problems.Count == 0)
            {
                EditorUtility.DisplayDialog("AudioKit", "Inga problem hittades.\n\nSetup ser rimlig ut.", "OK");
            }
            else
            {
                var msg = string.Join("\n", problems);
                Debug.LogWarning("[AudioKitValidator] Problem hittade:\n" + msg);
                EditorUtility.DisplayDialog("AudioKit", "Problem hittades. Se Console för detaljer.", "OK");
            }
        }
    }
}
#endif
