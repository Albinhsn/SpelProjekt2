using UnityEngine;

namespace AudioKit_FullTest_NoPlayer
{
    // Enkel helper så vi kan trigga AudioDucking från UnityEvent utan parametrar
    public sealed class AudioDuckingPulseInvoker : MonoBehaviour
    {
        [Header("Pulse")]
        [SerializeField] private string token = "test";
        [SerializeField, Range(0f, 1f)] private float duckAmount01 = 0.7f;
        [SerializeField] private float holdSeconds = 1.0f;

        public void Pulse()
        {
            var t = System.Type.GetType("AudioKit.FMOD.AudioDucking, Assembly-CSharp");
            // Fallback: försök hitta typen i alla assemblies
            if (t == null)
            {
                foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetType("AudioKit.FMOD.AudioDucking");
                    if (t != null) break;
                }
            }

            if (t == null) return;

            var prop = t.GetProperty("I", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var inst = prop != null ? prop.GetValue(null) : null;
            if (inst == null) return;

            var m = t.GetMethod("Pulse", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (m == null) return;

            m.Invoke(inst, new object[] { token, duckAmount01, holdSeconds });
        }
    }
}
