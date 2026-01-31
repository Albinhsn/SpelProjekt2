using UnityEngine;

// AudioKit anteckning
// Sätter param baserat på avstånd
// Bra när man närmar sig mål
// Värde går mellan nära och långt


namespace AudioKit.FMOD
{
    [DisallowMultipleComponent]
    public sealed class SwitchApproachDistanceDriver : MonoBehaviour
    {
        [Header("Parameter")]
        [SerializeField] private string parameterName = "ApproachDistance";
        [Tooltip("Valfritt: använd en AudioParameterSO istället för att skriva namn.")] 
        [SerializeField] private AudioParameterSO parameter;
        [SerializeField] private bool setAsGlobalParameter = true;

        [Header("Event")]
        [SerializeField] private bool alsoSetOnEventInstance = true;
        [SerializeField] private string eventIdToAlsoSet = "LayerA";

        [Header("Punkter")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        [Header("Spelare")]
        [SerializeField] private Transform player;

        private void Update()
        {
            if (player == null) return;
            if (startPoint == null) return;
            if (endPoint == null) return;
            var pName = (parameter != null && parameter.IsValid) ? parameter.fmodName : parameterName;
            if (string.IsNullOrEmpty(pName)) return;

            float distAB = Vector3.Distance(startPoint.position, endPoint.position);
            if (distAB <= 0.001f) return;

            float distAtoP = Vector3.Distance(startPoint.position, player.position);
            float t = Mathf.Clamp01(distAtoP / distAB);

            if (setAsGlobalParameter)
            {
                if (AudioSystem.I != null) AudioSystem.I.SetGlobalParam(pName, t);
                else if (AudioEventHub.I != null) AudioEventHub.I.SetGlobalParam(pName, t);
            }

            if (alsoSetOnEventInstance && AudioEventHub.I != null)
                AudioEventHub.I.SetEventParam(eventIdToAlsoSet, pName, t);
        }
    }
}
