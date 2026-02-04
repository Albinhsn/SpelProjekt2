using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class PersistentDataManager
{

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
        var game_state = Resources.Load<TextAsset>("game_state.json");
        if(game_state != null)
        {
            JArray items = JArray.Parse(game_state.text);
        }

    }

    public static void DeserializeAll(GameObject[] objs)
    {

    }

    public static void SerializeAll(GameObject[] objs)
    {

    }





}
