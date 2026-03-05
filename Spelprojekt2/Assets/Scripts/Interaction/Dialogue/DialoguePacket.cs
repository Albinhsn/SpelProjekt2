using Ink.Runtime;

namespace Interaction.Dialogue
{
    public class DialoguePacket
    {
        private Story m_story;
        private float m_typeDelay;
        //TODO: audio

        public Story story => m_story;
        public float typeDelay => m_typeDelay;

        public DialoguePacket(Story story, float type_delay)
        {
            m_story = story;
            m_typeDelay = type_delay;
        }
    }
}