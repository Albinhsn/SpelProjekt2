using System.Collections.Generic;
using Ink.Runtime;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

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

        private Dictionary<string, object> m_inkVariables = new Dictionary<string, object>();

        public static void SyncUp(Story story) //Read from story into global
        {
            foreach (string obj in story.variablesState)
            {
                if (m_instance.m_inkVariables.ContainsKey(obj)) m_instance.m_inkVariables[obj] = story.variablesState[obj];
                else m_instance.m_inkVariables.Add(obj, story.variablesState[obj]);
            }
        }
        
        public static void SyncDown(Story story) //Update local from global
        {
            foreach (string obj in story.variablesState)
            {
                story.variablesState[obj] = m_instance.m_inkVariables[obj]; //Update only included variables
            }
        }
    }
}