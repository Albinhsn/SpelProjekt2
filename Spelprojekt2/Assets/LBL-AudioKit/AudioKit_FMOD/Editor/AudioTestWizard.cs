#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AudioKit.FMOD
{
    /// <summary>
    /// Skapar en helt genre-oberoende "AudioTest" scen + tomma test-assets.
    /// Efteråt fylls FMOD events/parametrar i test-assets via Inspector.
    /// </summary>
    public static class AudioTestWizard
    {
        private const string TestFolder = "Assets/AudioTest";
        private const string ScenePath  = "Assets/AudioTest/AudioTest.unity";

        [MenuItem("AudioKit/Test/Skapa AudioTest-scen")]
        public static void CreateAudioTestScene()
        {
            // 1) Se till att Resources/Audio assets finns
            AudioTools.CreateConfig();
            AudioTools.CreateParamLibrary();
            AudioTools.CreateEventRegistry();

            EnsureFolder(TestFolder);

            // 2) Skapa test assets (tomma; fylls i via Inspector)
            var oneShotCue = EnsureAsset<AudioCueSO>(Path.Combine(TestFolder, "AC_TestOneShot.asset").Replace("\\", "/"));
            var loopCue    = EnsureAsset<AudioCueSO>(Path.Combine(TestFolder, "AC_TestLoop.asset").Replace("\\", "/"));
            var testParam  = EnsureAsset<AudioParameterSO>(Path.Combine(TestFolder, "AP_TestParam.asset").Replace("\\", "/"));

            // 3) Ny scen
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Root för att hålla Hierarchy ren
            var root = new GameObject("AudioKitTest");

            // Flytta default-objekt under root (Camera/Light) för en renare Hierarchy
            var mainCam = Camera.main;
            if (mainCam != null) mainCam.transform.SetParent(root.transform, false);

#if UNITY_2023_1_OR_NEWER
            var anyLight = Object.FindFirstObjectByType<Light>(FindObjectsInactive.Exclude);
#else
            var anyLight = Object.FindObjectOfType<Light>();
#endif
            if (anyLight != null) anyLight.transform.SetParent(root.transform, false);

            // Audio system root
            var sysGo = new GameObject("_AudioSystem");
            sysGo.transform.SetParent(root.transform, false);
            sysGo.AddComponent<AudioSystem>();
            sysGo.AddComponent<AudioEventHub>();
            sysGo.AddComponent<SfxDirector>();
            sysGo.AddComponent<MusicDirector>();
            sysGo.AddComponent<AudioParameterDriver>();

            // Test objects parent
            var tests = new GameObject("_AudioTests");
            tests.transform.SetParent(root.transform, false);

            // One-shot trigger cube
            var oneShot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            oneShot.name = "OneShotTrigger";
            oneShot.transform.SetParent(tests.transform);
            oneShot.transform.position = new Vector3(0, 0.5f, 6);
            oneShot.GetComponent<Collider>().isTrigger = true;
            var t1 = oneShot.AddComponent<AudioOneShotTrigger>();
            SetPrivateField(t1, "cue", oneShotCue);

            // Loop emitter sphere
            var loop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            loop.name = "LoopEmitter";
            loop.transform.SetParent(tests.transform);
            loop.transform.position = new Vector3(4, 0.5f, 6);
            var em = loop.AddComponent<AudioEmitter>();
            SetPrivateField(em, "cue", loopCue);
            SetPrivateField(em, "playOnEnable", false); // starta via UI

            // Parameter zone cube
            var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zone.name = "ParamZone";
            zone.transform.SetParent(tests.transform);
            zone.transform.position = new Vector3(-4, 0.5f, 6);
            zone.transform.localScale = new Vector3(6, 2, 6);
            zone.GetComponent<Collider>().isTrigger = true;
            var pz = zone.AddComponent<AudioParameterZone>();

            // Placeholder-param via asset (AP_TestParam.fmodName fylls i)
            var pref = new AudioParamRef { parameter = testParam, key = "", name = "" };
            SetPrivateField(pz, "parameter", pref);
            SetPrivateField(pz, "sourceId", "test_zone");

            // 4) UI
            CreateUI(root.transform, oneShotCue, loopCue, testParam, em);

            // 5) Spara scen
            EnsureFolder(Path.GetDirectoryName(ScenePath).Replace("\\", "/"));
            EditorSceneManager.SaveScene(scene, ScenePath);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("AudioTest-scen skapad: " + ScenePath);
        }

        private static void CreateUI(Transform root, AudioCueSO oneShotCue, AudioCueSO loopCue, AudioParameterSO testParam, AudioEmitter loopEmitter)
        {
            // EventSystem
            if (FindEventSystem() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).transform.SetParent(root, false);

            // Canvas
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(root, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Panel
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(canvasGo.transform, false);

            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -20);
            rt.sizeDelta = new Vector2(420, 460);

            var vlg = panel.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 10;
            vlg.padding = new RectOffset(12, 12, 12, 12);

            // UI driver
            var uiGo = new GameObject("AudioTestUI");
            uiGo.transform.SetParent(canvasGo.transform, false);
            var ui = uiGo.AddComponent<AudioTestUI>();
            ui.oneShotCue = oneShotCue;
            ui.loopCue = loopCue;
            ui.loopEmitter = loopEmitter;
            ui.testParam = new AudioParamRef { parameter = testParam };
            ui.loopRegistryId = "TEST_LOOP"; // placeholder (valfritt)

            // Buttons
            AddButton(panel.transform, "Play OneShot (cue)", ui, AudioTestUIButton.PlayOneShot);
            AddButton(panel.transform, "Start Loop (emitter)", ui, AudioTestUIButton.StartLoop);
            AddButton(panel.transform, "Stop Loop (emitter)", ui, AudioTestUIButton.StopLoop);
            AddButton(panel.transform, "Start Music (if configured)", ui, AudioTestUIButton.StartMusic);
            AddButton(panel.transform, "Stop Music", ui, AudioTestUIButton.StopMusic);
            AddButton(panel.transform, "Param ON (test)", ui, AudioTestUIButton.ParamOn);
            AddButton(panel.transform, "Param OFF (test)", ui, AudioTestUIButton.ParamOff);
        }

        private enum AudioTestUIButton { PlayOneShot, StartLoop, StopLoop, StartMusic, StopMusic, ParamOn, ParamOff }

        private static void AddButton(Transform parent, string label, AudioTestUI ui, AudioTestUIButton kind)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 52);

            var btn = go.GetComponent<Button>();

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var txt = textGo.GetComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = GetBuiltinFont();
            txt.color = Color.black;

            var tr = textGo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;

            // Persistent listeners (sparas i scenen)
            switch (kind)
            {
                case AudioTestUIButton.PlayOneShot: UnityEventTools.AddPersistentListener(btn.onClick, ui.PlayOneShot); break;
                case AudioTestUIButton.StartLoop:   UnityEventTools.AddPersistentListener(btn.onClick, ui.StartLoop);   break;
                case AudioTestUIButton.StopLoop:    UnityEventTools.AddPersistentListener(btn.onClick, ui.StopLoop);    break;
                case AudioTestUIButton.StartMusic:  UnityEventTools.AddPersistentListener(btn.onClick, ui.StartMusic);  break;
                case AudioTestUIButton.StopMusic:   UnityEventTools.AddPersistentListener(btn.onClick, ui.StopMusic);   break;
                case AudioTestUIButton.ParamOn:     UnityEventTools.AddPersistentListener(btn.onClick, ui.SetParamOn);  break;
                case AudioTestUIButton.ParamOff:    UnityEventTools.AddPersistentListener(btn.onClick, ui.SetParamOff); break;
            }
        }

        // FIX: Saknades. Används för att kolla om det redan finns en EventSystem i scenen.
        private static EventSystem FindEventSystem()
        {
            if (EventSystem.current != null) return EventSystem.current;

#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Exclude);
#else
            return Object.FindObjectOfType<EventSystem>();
#endif
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            path = path.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            var current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static T EnsureAsset<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var t = obj.GetType();
            var f = t.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (f != null) f.SetValue(obj, value);
        }

        private static Font GetBuiltinFont()
        {
#if UNITY_6000_0_OR_NEWER
            // Unity 6: Arial.ttf är inte längre en giltig built-in font.
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        }
    }
}
#endif
