using System;
using srUtils.Unity;
using TMPro;
using UnityEngine;

namespace srUtils.Unity.Ports
{
    public class IntDisplay : MonoBehaviour
    {
        [SerializeField] private IntPort port;
        [SerializeField] private TMP_Text display;
        [SerializeField] private string prefix = "";
        [SerializeField] private string postfix = "";

        private void Awake()
        {
            port.OnUpdated += SetDisplay;
            SetDisplay(port.value);
        }

        private void OnDestroy()
        {
            port.OnUpdated -= SetDisplay;
        }

        private void SetDisplay(int value)
        {
            display.text = prefix + value.ToString() + postfix;
        }
    }
}