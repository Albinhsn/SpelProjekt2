using System.Collections.Generic;
using UnityEngine;

// AudioKit anteckning
// Enkel runtime-overlay för felsökning.
// Visar volymer, ducking, aktiva loopar och aktiva globala parametrar.

namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    [AddComponentMenu("AudioKit/Debug/Audio Debug Overlay")]
    public sealed class AudioDebugOverlay : MonoBehaviour
    {
        [Header("Toggle")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private bool startVisible = false;

        [Header("View")]
        [SerializeField] private bool showVolumes = true;
        [SerializeField] private bool showLoops = true;
        [SerializeField] private bool showGlobalParams = true;
        [SerializeField] private int maxLoopsToList = 20;

        [Header("Layout")]
        [SerializeField] private Vector2 pos = new Vector2(12f, 12f);
        [SerializeField] private float width = 420f;

        private bool visible;

        private readonly List<string> loopIds = new List<string>(64);
        private readonly List<string> paramNames = new List<string>(64);

        private GUIStyle style;

        private void Awake()
        {
            visible = startVisible;
        }

        private void Update()
        {
            if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
                visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible) return;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    fontSize = 12
                };
            }

            var x = pos.x;
            var y = pos.y;

            var lines = 0;
            string TextLine(string t)
            {
                lines++;
                return t + "\n";
            }

            var sb = new System.Text.StringBuilder(512);
            sb.Append(TextLine("<b>AudioKit Debug</b>"));

            if (showVolumes)
            {
                var sys = AudioSystem.I;
                if (sys != null)
                {
                    sb.Append(TextLine($"Master: base {sys.GetBaseVolume("Master"):0.00}  duck {sys.GetDuckMultiplier("Master"):0.00}  eff {sys.GetEffectiveVolume("Master"):0.00}"));
                    sb.Append(TextLine($"Music : base {sys.GetBaseVolume("Music"):0.00}  duck {sys.GetDuckMultiplier("Music"):0.00}  eff {sys.GetEffectiveVolume("Music"):0.00}"));
                    sb.Append(TextLine($"SFX   : base {sys.GetBaseVolume("Sfx"):0.00}    duck {sys.GetDuckMultiplier("Sfx"):0.00}    eff {sys.GetEffectiveVolume("Sfx"):0.00}"));
                    sb.Append(TextLine($"UI    : base {sys.GetBaseVolume("UI"):0.00}     duck {sys.GetDuckMultiplier("UI"):0.00}     eff {sys.GetEffectiveVolume("UI"):0.00}"));
                }
                else
                {
                    sb.Append(TextLine("(AudioSystem saknas)"));
                }

                sb.Append("\n");
                lines++;
            }

            if (showLoops)
            {
                var hub = AudioEventHub.I;
                if (hub != null)
                {
                    hub.GetActiveLoopIds(loopIds);
                    sb.Append(TextLine($"Loops: {loopIds.Count}"));

                    var n = Mathf.Min(loopIds.Count, Mathf.Max(0, maxLoopsToList));
                    for (int i = 0; i < n; i++)
                        sb.Append(TextLine("  - " + loopIds[i]));

                    if (loopIds.Count > n)
                        sb.Append(TextLine($"  ... +{loopIds.Count - n}"));
                }
                else
                {
                    sb.Append(TextLine("(AudioEventHub saknas)"));
                }

                sb.Append("\n");
                lines++;
            }

            if (showGlobalParams)
            {
                var drv = AudioParameterDriver.I;
                if (drv != null)
                {
                    drv.GetActiveParams(paramNames);
                    sb.Append(TextLine($"Global Params: {paramNames.Count}"));

                    for (int i = 0; i < paramNames.Count; i++)
                    {
                        var pName = paramNames[i];
                        if (drv.TryGetCurrentValue(pName, out var v))
                            sb.Append(TextLine($"  {pName}: {v:0.00}"));
                        else
                            sb.Append(TextLine($"  {pName}: ?"));
                    }
                }
                else
                {
                    sb.Append(TextLine("(AudioParameterDriver saknas)"));
                }
            }

            var text = sb.ToString();
            var height = Mathf.Max(60f, 18f * lines);

            GUI.Box(new Rect(x - 6f, y - 6f, width + 12f, height + 12f), GUIContent.none);
            GUI.Label(new Rect(x, y, width, height), text, style);
        }
    }
}
