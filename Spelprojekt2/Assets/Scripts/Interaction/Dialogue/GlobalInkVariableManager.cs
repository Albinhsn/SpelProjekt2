using System.Collections.Generic;
using Ink.Runtime;
using JetBrains.Annotations;
using Object = UnityEngine.Object;
using UnityEngine;

namespace Interaction.Dialogue
{
    public class GlobalInkVariableManager
    {
        [CanBeNull] private static GlobalInkVariableManager m_single = null;

        private static GlobalInkVariableManager m_instance
        {
            get
            {
                m_single ??= new GlobalInkVariableManager();
                return m_single;
            }
        }

        private Dictionary<string, object> m_inkVariables_ = new Dictionary<string, object>();

        public static Dictionary<string, object> m_inkVariables => m_instance.m_inkVariables_;

        public static void SyncUp(Story story) //Read from story into global
        {
            foreach (string obj in story.variablesState)
            {
                Debug.Log($"Syncing {obj}");
                if (m_inkVariables.ContainsKey(obj)) m_inkVariables[obj] = story.variablesState[obj];
                else m_inkVariables.Add(obj, story.variablesState[obj]);
            }
        }
        
        public static void SyncDown(Story story) //Update local from global
        {
            List<string> vars = new List<string>(); //Collection was modified, may not enumerate workaround
            foreach (string obj in story.variablesState)
            {
                vars.Add(obj);
            }
            for (int a = 0; a < vars.Count; a++)
            {
                if (m_inkVariables.ContainsKey(vars[a])) story.variablesState[vars[a]] = m_inkVariables[vars[a]]; //Update only included variables
            }
        }

        public static void ClearAll()
        {
            m_inkVariables.Clear();
        }


        public static void ClearSelected(string[] selection)
        {
            for (int a = 0; a < selection.Length; a++)
            {
                m_inkVariables.Remove(selection[a]);
            }
        }

        public static void SetVariables(Dictionary<string, object> variables)
        {
            ClearAll();
            foreach(KeyValuePair<string, object> kv in variables)
            {
                m_instance.m_inkVariables_[kv.Key] = kv.Value;
            }
        }
    }
}
