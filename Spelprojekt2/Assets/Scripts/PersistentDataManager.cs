using UnityEngine;

public sealed class PersistentDataManager
{

    private enum DataKind
    {
        Player = (1 << 0),
    }

    private static PersistentDataManager _instance;
    private static PersistentDataManager m_instance {
        get
        {
            if(_instance == null)
            {
                _instance = new();
            }
            return _instance;
        }

    }

    public PersistentDataManager()
    {
    }

    public static void DeserializeAll(SerializableObject[] objs)
    {
        byte[] game_state = Resources.Load<TextAsset>("game_state.bin").bin;
        if(game_state != null)
        {
        }

    }

    public static void SerializeAll(SerializableObject[] objs)
    {
        // Serialize current filter
    }





}
