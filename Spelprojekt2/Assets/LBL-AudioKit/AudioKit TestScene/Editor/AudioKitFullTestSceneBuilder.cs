#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AudioKit_FullTest_NoPlayer
{
    public static class AudioKitFullTestSceneBuilder
    {
        private const string RootFolder = "Assets/AudioKit_FullTest_NoPlayer";
        private const string GeneratedFolder = RootFolder + "/Generated";
        private const string GeneratedScenePath = GeneratedFolder + "/AudioKit_FullTest_NoPlayer.unity";

        private const string ResourcesAudioFolder = "Assets/Resources/Audio";

        // Test-keys/IDs (ni kan byta i inspector efteråt)
        private const string ID_TEST_SHOT = "TEST_SHOT";
        private const string ID_TEST_LOOP = "TEST_LOOP";
        private const string ID_TEST_SNAPSHOT = "TEST_SNAPSHOT";

        private const string PARAM_GLOBAL = "TEST_GLOBAL";
        private const string PARAM_LOCAL = "TEST_LOCAL";
        private const string PARAM_DISTANCE = "TEST_DISTANCE";
        private const string PARAM_PATH = "TEST_PATH";
        private const string PARAM_OCCLUSION = "TEST_OCCLUSION";

        [MenuItem("Tools/AudioKit/TestLab/Create FULL Test Scene (No Player)")]
        public static void CreateFullTestScene_NoPlayer()
        {
            // Kontroll: AudioKit måste finnas
            var audioTriggerT = FindType("AudioKit.FMOD.AudioTrigger");
            if (audioTriggerT == null)
            {
                EditorUtility.DisplayDialog(
                    "AudioKit saknas",
                    "Jag hittar inte AudioKit.FMOD.* i projektet. Importera LBL-AudioKit först, sen kör igen.",
                    "OK"
                );
                return;
            }

            Directory.CreateDirectory(GeneratedFolder);
            Directory.CreateDirectory(ResourcesAudioFolder);

            EnsureResourcesAssets();
            var cueShot = EnsureCueAsset("AC_TestShot");
            var cueEmitter = EnsureCueAsset("AC_TestEmitterLoop");

            // Skapa scen
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);

            // Root
            var root = new GameObject("_AudioKit_FullTest_NoPlayer");

            // Ground
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.position = new Vector3(0f, 0f, 35f);
            ground.transform.localScale = new Vector3(2.2f, 1f, 7.0f);

            // Spawn marker (du droppar in din player här)
            var spawn = new GameObject("PlayerSpawn_DROP_YOUR_PLAYER_PREFAB_HERE");
            spawn.transform.SetParent(root.transform);
            spawn.transform.position = new Vector3(0f, 1f, 2f);

            // Light
            var lightGo = new GameObject("Directional Light");
            lightGo.transform.SetParent(root.transform);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Camera (valfritt – ni kan radera och använda er player-camera)
            var camGo = new GameObject("TempCamera_RemoveIfYouUsePlayerCamera");
            camGo.transform.SetParent(root.transform);
            var cam = camGo.AddComponent<Camera>();
            camGo.transform.position = new Vector3(0f, 8f, -6f);
            camGo.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            // Ducking system
            var duckingGo = new GameObject("AudioDucking_System");
            duckingGo.transform.SetParent(root.transform);
            var duckingT = FindType("AudioKit.FMOD.AudioDucking");
            if (duckingT != null)
            {
                duckingGo.AddComponent(duckingT);
            }
            var duckInvoker = duckingGo.AddComponent<AudioDuckingPulseInvoker>();

            float z = 10f;

            // Station: OneShot via Cue
            CreateCubeStation(root.transform, "S01_OneShot_Cue", new Vector3(0f, 1f, z));
            {
                var go = GameObject.Find("S01_OneShot_Cue");
                go.AddComponent(FindType("AudioKit.FMOD.AudioOneShotTrigger"));
                SetSerializedObjectRef(go, "AudioOneShotTrigger", "cue", cueShot);
                SetSerializedString(go, "AudioOneShotTrigger", "activatorTag", "Player");
            }
            z += 7f;

            // Station: OneShot via EventRegistry ID (PlayEvent)
            CreateCubeStation(root.transform, "S02_OneShot_EventId", new Vector3(0f, 1f, z));
            {
                var go = GameObject.Find("S02_OneShot_EventId");
                var trig = go.AddComponent(audioTriggerT);
                ConfigureAudioTriggerActions(trig, new[] {
                    new ActionSpec {
                        typeIndex = 1, // PlayEvent
                        eventId = ID_TEST_SHOT
                    }
                }, runOnEnter: true, runOnExit: false);
            }
            z += 7f;

            // Station: Start Loop
            CreateCubeStation(root.transform, "S03_StartLoop", new Vector3(0f, 1f, z));
            {
                var go = GameObject.Find("S03_StartLoop");
                var trig = go.AddComponent(audioTriggerT);
                ConfigureAudioTriggerActions(trig, new[] {
                    new ActionSpec { typeIndex = 2, eventId = ID_TEST_LOOP }
                }, true, false);
            }
            z += 7f;

            // Station: Stop Loop
            CreateCubeStation(root.transform, "S04_StopLoop", new Vector3(0f, 1f, z));
            {
                var go = GameObject.Find("S04_StopLoop");
                var trig = go.AddComponent(audioTriggerT);
                ConfigureAudioTriggerActions(trig, new[] {
                    new ActionSpec { typeIndex = 3, eventId = ID_TEST_LOOP, fade = 0.6f }
                }, true, false);
            }
            z += 7f;

            // Station: Local param 0
            CreateCubeStation(root.transform, "S05_LocalParam_0", new Vector3(-4f, 1f, z));
            {
                var go = GameObject.Find("S05_LocalParam_0");
                var trig = go.AddComponent(audioTriggerT);
                ConfigureAudioTriggerActions(trig, new[] {
                    new ActionSpec { typeIndex = 5, eventId = ID_TEST_LOOP, paramName = PARAM_LOCAL, paramValue = 0f }
                }, true, false);
            }

            // Station: Local param 1
            CreateCubeStation(root.transform, "S06_LocalParam_1", new Vector3(4f, 1f, z));
            {
                var go = GameObject.Find("S06_LocalParam_1");
                var trig = go.AddComponent(audioTriggerT);
                ConfigureAudioTriggerActions(trig, new[] {
                    new ActionSpec { typeIndex = 5, eventId = ID_TEST_LOOP, paramName = PARAM_LOCAL, paramValue = 1f }
                }, true, false);
            }
            z += 9f;

            // Zone: Global parameter
            CreateZone(root.transform, "Z01_GlobalParamZone", new Vector3(0f, 1f, z), new Vector3(10f, 2f, 10f));
            {
                var go = GameObject.Find("Z01_GlobalParamZone");
                var zoneT = FindType("AudioKit.FMOD.AudioParameterZone");
                var zone = go.AddComponent(zoneT);
                // parameter.key = PARAM_GLOBAL
                SetAudioParamRefKey(zone, "parameter", PARAM_GLOBAL);
                SetSerializedFloat(zone, "valueInside", 1f);
                SetSerializedFloat(zone, "valueOutside", 0f);
                SetSerializedFloat(zone, "fadeSeconds", 0.25f);
                SetSerializedString(zone, "playerTag", "Player");
            }
            z += 13f;

            // Zone: Snapshot
            CreateZone(root.transform, "Z02_SnapshotZone", new Vector3(0f, 1f, z), new Vector3(10f, 2f, 10f));
            {
                var go = GameObject.Find("Z02_SnapshotZone");
                var snapZoneT = FindType("AudioKit.FMOD.SnapshotZone");
                var snapZone = go.AddComponent(snapZoneT);
                // snapshotIdOrPath = ID_TEST_SNAPSHOT
                SetSerializedString(snapZone, "snapshotIdOrPath", ID_TEST_SNAPSHOT);
                SetSerializedString(snapZone, "requiredTag", "Player");
                SetSerializedFloat(snapZone, "fadeOutSeconds", 0.25f);
            }
            z += 13f;

            // Station: Snapshot Pulse (UnityEvent -> SnapshotPulse.Pulse)
            CreateCubeStation(root.transform, "S07_SnapshotPulse", new Vector3(0f, 1f, z));
            {
                var go = GameObject.Find("S07_SnapshotPulse");
                var pulseT = FindType("AudioKit.FMOD.SnapshotPulse");
                var pulse = go.AddComponent(pulseT);

                var trig = go.AddComponent(audioTriggerT);
                ConfigureAudioTriggerActions(trig, Array.Empty<ActionSpec>(), true, false);

                AddUnityEventListener(trig, "onTriggered", pulse, "Pulse");
            }
            z += 7f;

            // Station: Duck Pulse (UnityEvent -> invoker.Pulse)
            CreateCubeStation(root.transform, "S08_DuckPulse", new Vector3(0f, 1f, z));
            {
                var go = GameObject.Find("S08_DuckPulse");
                var trig = go.AddComponent(audioTriggerT);
                ConfigureAudioTriggerActions(trig, Array.Empty<ActionSpec>(), true, false);
                AddUnityEventListener(trig, "onTriggered", duckInvoker, "Pulse");
            }
            z += 10f;

            // Driver: Approach Distance (player -> target)
            {
                var target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                target.name = "D01_Target_Distance";
                target.transform.SetParent(root.transform);
                target.transform.position = new Vector3(6f, 1f, z);
                target.transform.localScale = Vector3.one * 0.7f;

                var driverGo = new GameObject("D01_ApproachDistanceDriver");
                driverGo.transform.SetParent(root.transform);
                driverGo.transform.position = new Vector3(0f, 0f, z);
                var driverT = FindType("AudioKit.FMOD.ApproachDistanceParamDriver");
                var driver = driverGo.AddComponent(driverT);

                SetAudioParamRefKey(driver, "parameter", PARAM_DISTANCE);
                SetSerializedObjectRef(driver, "target", target.transform);
                SetSerializedString(driver, "playerTag", "Player");
                SetSerializedFloat(driver, "minDistance", 1.5f);
                SetSerializedFloat(driver, "maxDistance", 25f);
            }
            z += 12f;

            // Driver: Path Progress
            {
                var p0 = new GameObject("P0");
                var p1 = new GameObject("P1");
                var p2 = new GameObject("P2");
                p0.transform.SetParent(root.transform);
                p1.transform.SetParent(root.transform);
                p2.transform.SetParent(root.transform);
                p0.transform.position = new Vector3(-6f, 0.2f, z);
                p1.transform.position = new Vector3(0f, 0.2f, z + 8f);
                p2.transform.position = new Vector3(6f, 0.2f, z + 16f);

                var driverGo = new GameObject("D02_PathProgressDriver");
                driverGo.transform.SetParent(root.transform);
                driverGo.transform.position = new Vector3(0f, 0f, z);

                var driverT = FindType("AudioKit.FMOD.PathProgressParamDriver");
                var driver = driverGo.AddComponent(driverT);

                SetAudioParamRefKey(driver, "parameter", PARAM_PATH);
                SetSerializedString(driver, "playerTag", "Player");

                // points array
                var so = new SerializedObject(driver);
                var pointsProp = so.FindProperty("points");
                pointsProp.arraySize = 3;
                pointsProp.GetArrayElementAtIndex(0).objectReferenceValue = p0.transform;
                pointsProp.GetArrayElementAtIndex(1).objectReferenceValue = p1.transform;
                pointsProp.GetArrayElementAtIndex(2).objectReferenceValue = p2.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            z += 22f;

            // Spatial: Emitter + Occlusion + Wall
            {
                var emitterGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                emitterGo.name = "E01_EmitterSphere";
                emitterGo.transform.SetParent(root.transform);
                emitterGo.transform.position = new Vector3(0f, 1.2f, z);
                emitterGo.transform.localScale = Vector3.one * 0.9f;

                var emitterT = FindType("AudioKit.FMOD.AudioEmitter");
                var emitter = emitterGo.AddComponent(emitterT);
                SetSerializedObjectRef(emitter, "cue", cueEmitter);
                SetSerializedBool(emitter, "playOnEnable", true);

                var occT = FindType("AudioKit.FMOD.AudioOcclusionEmitter");
                var occ = emitterGo.AddComponent(occT);
                SetAudioParamRefKey(occ, "occlusionParam", PARAM_OCCLUSION);
                SetSerializedFloat(occ, "clearValue", 0f);
                SetSerializedFloat(occ, "occludedValue", 1f);

                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "E01_OccluderWall";
                wall.transform.SetParent(root.transform);
                wall.transform.position = new Vector3(0f, 1.2f, z - 6f);
                wall.transform.localScale = new Vector3(8f, 3f, 0.4f);
            }

            // Spara
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GeneratedScenePath);
            AssetDatabase.Refresh();

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(GeneratedScenePath);
            EditorGUIUtility.PingObject(sceneAsset);

            EditorUtility.DisplayDialog(
                "Klart",
                "Scenen är skapad:\n" + GeneratedScenePath + "\n\nÖppna den och droppa in er Player prefab vid PlayerSpawn.",
                "OK"
            );
        }

        // ---------- Helpers ----------

        private struct ActionSpec
        {
            public int typeIndex;
            public string eventId;
            public string vcaName;
            public float vcaValue;
            public string paramName;
            public float paramValue;
            public string snapshot;
            public bool snapshotEnable;
            public float fade;
        }

        private static void CreateCubeStation(Transform parent, string name, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(6f, 2f, 6f);

            var col = go.GetComponent<BoxCollider>();
            col.isTrigger = true;
        }

        private static void CreateZone(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var c = go.AddComponent<BoxCollider>();
            c.isTrigger = true;
            c.size = size;
        }

        private static void ConfigureAudioTriggerActions(Component audioTrigger, ActionSpec[] actions, bool runOnEnter, bool runOnExit)
        {
            var so = new SerializedObject(audioTrigger);
            so.FindProperty("runOnEnter").boolValue = runOnEnter;
            so.FindProperty("runOnExit").boolValue = runOnExit;
            so.FindProperty("activatorTag").stringValue = "Player";

            var actProp = so.FindProperty("actions");
            actProp.arraySize = actions.Length;

            for (int i = 0; i < actions.Length; i++)
            {
                var a = actions[i];
                var p = actProp.GetArrayElementAtIndex(i);
                p.FindPropertyRelative("type").enumValueIndex = a.typeIndex;
                p.FindPropertyRelative("eventId").stringValue = a.eventId ?? "";
                p.FindPropertyRelative("vcaName").stringValue = a.vcaName ?? "";
                p.FindPropertyRelative("vcaValue").floatValue = a.vcaValue;
                p.FindPropertyRelative("parameterName").stringValue = a.paramName ?? "";
                p.FindPropertyRelative("parameterValue").floatValue = a.paramValue;
                p.FindPropertyRelative("snapshot").stringValue = a.snapshot ?? "";
                p.FindPropertyRelative("snapshotEnable").boolValue = a.snapshotEnable;
                p.FindPropertyRelative("fade").floatValue = a.fade;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddUnityEventListener(Component host, string unityEventFieldName, Component target, string methodName)
        {
            var f = host.GetType().GetField(unityEventFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) return;
            var evt = f.GetValue(host) as UnityEvent;
            if (evt == null) return;

            var m = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (m == null) return;

            // Add persistent no-arg listener
            UnityAction call = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, m);
            UnityEventTools.AddPersistentListener(evt, call);
            EditorUtility.SetDirty(host);
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        private static UnityEngine.Object EnsureCueAsset(string cueName)
        {
            var cueType = FindType("AudioKit.FMOD.AudioCueSO");
            if (cueType == null) return null;

            var folder = GeneratedFolder + "/Cues";
            Directory.CreateDirectory(folder);

            var path = folder + "/" + cueName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath(path, cueType);
            if (existing != null) return existing;

            var inst = ScriptableObject.CreateInstance(cueType);
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            return inst;
        }

        private static void EnsureResourcesAssets()
        {
            // EventRegistry.asset
            EnsureIfMissing(ResourcesAudioFolder + "/EventRegistry.asset", "AudioKit.FMOD.AudioEventRegistrySO", so =>
            {
                // Lägg in placeholder IDs (EventReference lämnas tomt)
                var events = so.FindProperty("events");
                events.arraySize = 2;

                var e0 = events.GetArrayElementAtIndex(0);
                e0.FindPropertyRelative("id").stringValue = ID_TEST_SHOT;

                var e1 = events.GetArrayElementAtIndex(1);
                e1.FindPropertyRelative("id").stringValue = ID_TEST_LOOP;

                so.ApplyModifiedPropertiesWithoutUndo();
            });

            // ParamLibrary
            EnsureIfMissing(ResourcesAudioFolder + "/AudioParamLibrary.asset", "AudioKit.FMOD.AudioParamLibrarySO", so =>
            {
                var entries = so.FindProperty("entries");
                entries.arraySize = 4;

                SetParamEntry(entries, 0, PARAM_GLOBAL, PARAM_GLOBAL);
                SetParamEntry(entries, 1, PARAM_DISTANCE, PARAM_DISTANCE);
                SetParamEntry(entries, 2, PARAM_PATH, PARAM_PATH);
                SetParamEntry(entries, 3, PARAM_OCCLUSION, PARAM_OCCLUSION);

                so.ApplyModifiedPropertiesWithoutUndo();
            });

            // AudioConfig (valfri men skönt att ha)
            EnsureIfMissing(ResourcesAudioFolder + "/AudioConfig.asset", "AudioKit.FMOD.AudioConfigSO", so =>
            {
                so.ApplyModifiedPropertiesWithoutUndo();
            });
        }

        private static void EnsureIfMissing(string assetPath, string typeName, Action<SerializedObject> configure)
        {
            var t = FindType(typeName);
            if (t == null) return;

            var existing = AssetDatabase.LoadAssetAtPath(assetPath, t);
            if (existing != null) return;

            Directory.CreateDirectory(Path.GetDirectoryName(assetPath) ?? ResourcesAudioFolder);
            var inst = ScriptableObject.CreateInstance(t);
            AssetDatabase.CreateAsset(inst, assetPath);
            AssetDatabase.SaveAssets();

            var so = new SerializedObject(inst);
            configure?.Invoke(so);
            EditorUtility.SetDirty(inst);
            AssetDatabase.SaveAssets();
        }

        private static void SetParamEntry(SerializedProperty entries, int idx, string key, string fmodName)
        {
            var e = entries.GetArrayElementAtIndex(idx);
            e.FindPropertyRelative("key").stringValue = key;
            e.FindPropertyRelative("fmodName").stringValue = fmodName;
        }

        private static void SetAudioParamRefKey(Component component, string fieldName, string key)
        {
            var so = new SerializedObject(component);
            var param = so.FindProperty(fieldName);
            if (param != null)
            {
                var keyProp = param.FindPropertyRelative("key");
                if (keyProp != null) keyProp.stringValue = key;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedString(Component component, string fieldName, string value)
        {
            var so = new SerializedObject(component);
            var p = so.FindProperty(fieldName);
            if (p != null) p.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedString(GameObject host, string componentTypeNameContains, string fieldName, string value)
        {
            var c = host.GetComponents<Component>().FirstOrDefault(x => x != null && x.GetType().Name.Contains(componentTypeNameContains));
            if (c == null) return;
            SetSerializedString(c, fieldName, value);
        }

        private static void SetSerializedFloat(Component component, string fieldName, float value)
        {
            var so = new SerializedObject(component);
            var p = so.FindProperty(fieldName);
            if (p != null) p.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedBool(Component component, string fieldName, bool value)
        {
            var so = new SerializedObject(component);
            var p = so.FindProperty(fieldName);
            if (p != null) p.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedObjectRef(Component component, string fieldName, UnityEngine.Object obj)
        {
            var so = new SerializedObject(component);
            var p = so.FindProperty(fieldName);
            if (p != null) p.objectReferenceValue = obj;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedObjectRef(Component component, string fieldName, UnityEngine.Object obj, bool apply)
        {
            var so = new SerializedObject(component);
            var p = so.FindProperty(fieldName);
            if (p != null) p.objectReferenceValue = obj;
            if (apply) so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedObjectRef(Component component, string fieldName, Transform t)
        {
            SetSerializedObjectRef(component, fieldName, (UnityEngine.Object)t);
        }


        private static void SetSerializedObjectRef(GameObject host, string fieldName, UnityEngine.Object obj)
        {
            // Not used
        }

        private static void SetSerializedObjectRef(GameObject host, string componentTypeNameContains, string fieldName, UnityEngine.Object obj)
        {
            var c = host.GetComponents<Component>().FirstOrDefault(x => x != null && x.GetType().Name.Contains(componentTypeNameContains));
            if (c == null) return;
            SetSerializedObjectRef(c, fieldName, obj);
        }
    }
}
#endif
