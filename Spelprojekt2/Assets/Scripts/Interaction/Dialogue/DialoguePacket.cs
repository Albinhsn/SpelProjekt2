using Ink.Runtime;

namespace Interaction.Dialogue
{
    public class DialoguePacket
    {
        private Story m_story;
        private float m_typeDelay;
        private char m_typingIndicator;
        //TODO: audio

        public Story story => m_story;
        public float typeDelay => m_typeDelay;
        public char typingIndicator => m_typingIndicator;

        public DialoguePacket(Story story, float type_delay, char typing_indicator)
        {
            m_story = story;
            m_typeDelay = type_delay;
            m_typingIndicator = typing_indicator;
        }
    }
}