using System;
using UnityEngine;

namespace srUtils.Unity
{
    public class DisableRendererOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Renderer>().enabled = false;
        }
    }
}