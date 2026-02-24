using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections.Generic;
using static Serialization;

// TODO: 
// - Update the presentation that you basically have subchunks of lvls
// - 

/*
   Call DeserializeAll when we have transitioned the first time after pressing play
   Call DeserializeLoadedScenes when we have transtioned from say tutorial -> world1

   LVLS: 
    MAGIC
    Storlek
    Version
    SGO Count
    Object data
    Spawner Count
    Spawner data
   STRY:
    MAGIC
    Storlek
    Version
    KV Count
    Key, Value data
   PLAY:
    MAGIC
    Storlek
    Version
    Aktivt filter
    Unlocked filter
    Spelarens position
    Spelarens rotation
    Spawn position
    Spawn rotation
    Spawnpoint build index

 */

public struct DeserializedPlayerResult
{
    public bool found;
    public FilterKind active_filter;
}

public sealed class PersistentDataManager
{

    private struct ObjectData
    {
        public Vector3 position;
        public Quaternion rotation;

        public ObjectData(float[] position, float[] rotation)
        {
            this.position = new Vector3(position[0], position[1], position[2]);
            this.rotation = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
        }
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

    private static string m_dataPath => Path.Combine(Application.persistentDataPath, "game.bin");

    public PersistentDataManager()
    {
    }

    private struct ChunkPLAY 
    {
        public bool m_exists;
        public uint magic;
        public int version;
        public int filter;
        public bool unlocked_filter;
        public Vector3 player_p;
        public Quaternion player_r;
        public Vector3 spawn_p;
        public Quaternion spawn_r;
        public int scene_index;
    }

    private struct STRYDictEntry
    {
        public string m_key;
        public string m_value;
    }

    private struct ChunkSTRY
    {
        public bool m_exists;
        public STRYDictEntry[] m_entries;
    }

    private struct ChunkLVL
    {
        public string m_id;
        public ObjectData[] m_sgos;
        public ObjectData[] m_spawners;
    }

    private struct ChunkLVLS
    {
        public bool m_exists;
        public ChunkLVL[] m_levels;
    }


    private struct GameState
    {
        public bool m_existed;
        public ChunkPLAY m_player;
        public ChunkSTRY m_story;
        public ChunkLVLS m_levels;
    }

    public static void DeleteSave()
    {
        var path = m_dataPath;
        if(File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static int MAGIC(char a, char b, char c, char d)
    {
        return (int)((((int)a) << 24) | 
                     (((int)b) << 16)  |
                     (((int)c) << 8)   |
                     (int)d
                    );

    }

    private static GameState Deserialize()
    {
        GameState result = new();

        int LVLS = MAGIC('l', 'v', 'l', 's');
        int STRY = MAGIC('s', 't', 'r', 'y');
        int PLAY = MAGIC('p', 'l', 'a', 'y');


        // Read the file
        var path = m_dataPath;
        if(File.Exists(path))
        {
            var buffer = File.ReadAllBytes(path);
            int offset = 0;

            for(; offset < buffer.Length;)
            {
                // Read MAGIC
                int magic = 0;
                offset = DeserializeScalar<int>(ref magic, buffer, offset);

                // Read storlek
                int size = 0;
                offset = DeserializeScalar<int>(ref size, buffer, offset);

                switch(magic)
                {
                    case LVLS:
                    {

                        break;
                    }
                    case STRY:
                    {
                        break;
                    }
                    case PLAY:
                    {
                        break;
                    }
                }
                 

            }

        }
        return result;
    }

    public static LevelData LevelToLoad(LevelsData levels)
    {
    }

    public static void DeserializeLoadedScenes()
    {
    }

    public static void SerializeLoadedScenes()
    {
    }

    private static void SerializeDialogueState()
    {
    }

    // NOTE(ah): this resets it to the serialized version, if it exists
    private static void DeserializeDialogueState()
    {

    }

    public static void DeserializeAll()
    {

    }

    public static void SerializeAll()
    {

    }

}
