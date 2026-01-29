using UnityEngine;

namespace SP2.Audio
{
    // Visar volymer + musikstate/intensity/danger (så långt systemet vet).
    public sealed class AudioDebugOverlay : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.F9;
        [SerializeField] private bool visible;

        private GUIStyle _style;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible) return;

            var sys = AudioSystem.Instance;
            if (sys == null || sys.Mixer == null) return;

            _style ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                richText = true
            };

            GUILayout.BeginArea(new Rect(12, 12, 520, 220), GUI.skin.box);
            GUILayout.Label("<b>SP2 Audio Debug</b>", _style);
            GUILayout.Space(4);

            GUILayout.Label($"Master: {sys.Mixer.Master01:0.00}  Music: {sys.Mixer.Music01:0.00}  SFX: {sys.Mixer.Sfx01:0.00}  UI: {sys.Mixer.Ui01:0.00}", _style);
            GUILayout.Label($"MusicState: {(int)sys.Config.startMusicState} (start)   Quantize: {sys.Config.quantizeOnBar}", _style);
            GUILayout.Label("Tips: Använd zoner/driver för global params (Combat/Indoor/DangerZone/GameOver).", _style);

            GUILayout.EndArea();
        }
    }
}
