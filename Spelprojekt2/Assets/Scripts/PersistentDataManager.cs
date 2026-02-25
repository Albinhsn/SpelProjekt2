using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using Interaction;
using Interaction.Dialogue;
using static Serialization;


public struct DeserializedPlayerResult
{
    public bool found;
    public FilterKind active_filter;
    public bool m_unlockedFilter;
    public bool m_unlockedFlipped;
}

public sealed class PersistentDataManager
{
    private struct GameState
    {
        public bool m_isValid;
        public ChunkPLAY m_play;
        public ChunkSTRY m_story;
        public ChunkLVLS m_levels;
    }

    private struct ForceInteractorTriggerData
    {
        public byte[] id;
        public bool m_hasBeenActivated;

        public ForceInteractorTriggerData(byte[] id, int activated)
        {
            this.id       = id;
            this.m_hasBeenActivated = activated == 1;
        }
    }

    private struct FlippedData
    {
        public byte[] id;
        public bool m_usesGravity;

        public FlippedData(byte[] id, int usesGravity)
        {
            this.id       = id;
            this.m_usesGravity = usesGravity == 1;
        }
    }

    private struct ObjectData
    {
        public byte[] id;
        public Vector3 position;
        public Quaternion rotation;

        public ObjectData(byte[] id, float[] position, float[] rotation)
        {
            this.id       = id;
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

    private static GameState m_gameState;
    private static string m_dataPath => Path.Combine(Application.persistentDataPath, "game.bin");

    public PersistentDataManager()
    {
    }

    private struct ChunkPLAY 
    {
        public bool m_exists;
        public FilterKind m_filter;
        public bool m_unlockedFilter;
        public bool m_unlockedFlipped;
        public Vector3 m_playerP;
        public Quaternion m_playerR;
        public Vector3 m_spawnP;
        public Quaternion m_spawnR;
        public int m_sceneIndex;

        public int m_chunkSize
        {
            get
            {
                int size = 0;
                // ah: header
                size += sizeof(int) * 3;

                // active filter, unlocked filter, unlocked flipped
                size  += sizeof(int) * 3;

                // play pos + spawn pos
                size += sizeof(float) * 3 * 2;

                // play rot + spawn rot
                size += sizeof(float) * 4 * 2;

                // spawnpoint build index
                size += sizeof(int);

                return size;
            }
        }

        const int version = 0;

        public int Serialize(byte[] buffer, int offset)
        {

            // ah: header
            {
                offset = SerializeScalar<int>(ref buffer, PLAY, offset);
                int size = m_chunkSize;
                offset = SerializeScalar<int>(ref buffer, size, offset);
                offset = SerializeScalar<int>(ref buffer, version, offset);
            }

            // ah: filter
            {
                offset = SerializeScalar<int>(ref buffer, (int)m_filter, offset);
                offset = SerializeScalar<int>(ref buffer, m_unlockedFilter ? 1 : 0, offset);
            }

            // ah: flipped
            {
                offset = SerializeScalar<int>(ref buffer, m_unlockedFlipped ? 1 : 0, offset);
            }

            // ah: player 
            {
                float[] p = new float[3]
                {
                    m_playerP.x,
                    m_playerP.y,
                    m_playerP.z,
                };
                offset = SerializeArray<float>(ref buffer, p, offset);

                float[] r = new float[4]
                {
                    m_playerR.x,
                    m_playerR.y,
                    m_playerR.z,
                    m_playerR.w,
                };
                offset = SerializeArray<float>(ref buffer, r, offset);
            }

            // ah: spawn
            {
                float[] p = new float[3]
                {
                    m_spawnP.x,
                    m_spawnP.y,
                    m_spawnP.z,
                };
                offset = SerializeArray<float>(ref buffer, p, offset);

                float[] r = new float[4]
                {
                    m_spawnR.x,
                    m_spawnR.y,
                    m_spawnR.z,
                    m_spawnR.w,
                };
                offset = SerializeArray<float>(ref buffer, r, offset);

                offset = SerializeScalar<int>(ref buffer, m_sceneIndex, offset);
            }

            return offset;
        }
    }

    private struct ChunkSTRY
    {
        public bool m_exists;
        public Dictionary<string, object> m_entries;

        public int m_chunkSize
        {
            get
            {
                return sizeof(int) * 3 + sizeof(int) + MAX_INK_VARIABLE_SIZE * 2 * m_entries.Count;
            }
        }

        private const int version = 0;

        public int Serialize(byte[] buffer, int offset)
        {

            // ah: header
            {
                offset = SerializeScalar<int>(ref buffer, STRY, offset);
                int size = m_chunkSize;
                offset = SerializeScalar<int>(ref buffer, size, offset);
                offset = SerializeScalar<int>(ref buffer, version, offset);
            }

            // ah: entries
            int entry_count = m_entries != null ? m_entries.Count : 0;
            offset = SerializeScalar<int>(ref buffer, entry_count, offset);
            if(m_entries != null)
            {
                foreach(KeyValuePair<string, object> kv in m_entries)
                {
                    // ah: key
                    {
                        string key   = kv.Key;

                        if(key.Length > MAX_INK_VARIABLE_SIZE)
                        {
                            Debug.LogError($"[PDM] Error serializing STRY, key is longer then max {key.Length} vs {MAX_INK_VARIABLE_SIZE}");
                        }
                        int size    = key.Length > MAX_INK_VARIABLE_SIZE ? MAX_INK_VARIABLE_SIZE : key.Length;
                        int padding = MAX_INK_VARIABLE_SIZE - size;

                        byte[] key_bytes = Encoding.UTF8.GetBytes(key);
                        offset = memcpy(ref buffer, key_bytes, offset);
                        offset += padding;
                    }

                    // ah: value
                    {
                        string value = (string)kv.Value;

                        if(value.Length > MAX_INK_VARIABLE_SIZE)
                        {
                            Debug.LogError($"[PDM] Error serializing STRY, key is longer then max {value.Length} vs {MAX_INK_VARIABLE_SIZE}");
                        }
                        int size    = value.Length > MAX_INK_VARIABLE_SIZE ? MAX_INK_VARIABLE_SIZE : value.Length;
                        int padding = MAX_INK_VARIABLE_SIZE - size;

                        byte[] key_bytes = Encoding.UTF8.GetBytes(value);
                        offset = memcpy(ref buffer, key_bytes, offset);
                        offset += padding;
                    }
                }
            }
            return offset;
        }
    }

    private struct ChunkLVL
    {
        public byte[] m_id;
        public List<ObjectData> m_sgos;
        public List<ObjectData> m_spawners;
        public List<FlippedData> m_flipped;
        public List<ForceInteractorTriggerData> m_triggers;

        private const int version = 0;

        public int m_chunkSize
        {
            get
            {
                // ah: header
                int size = sizeof(int) * 3;

                size += 16;

                // guid, pos, rotation
                int size_of_object_data = 16 + sizeof(float) * 3 + sizeof(float) * 4;

                // the fucking size of the arrays
                size += sizeof(int) * 4;
                size += size_of_object_data * (m_sgos != null ? m_sgos.Count : 0);
                size += size_of_object_data * (m_spawners != null ? m_spawners.Count : 0);

                int size_of_flipped_data = 16 + sizeof(int);
                size += size_of_flipped_data * (m_flipped != null ? m_flipped.Count : 0);

                int size_of_trigger_data = 16 + sizeof(int);
                size += size_of_trigger_data * (m_triggers != null ? m_triggers.Count : 0);

                return size;
            }
        }

        public int Serialize(byte[] buffer, int offset)
        {

            // ah: header
            {
                offset = SerializeScalar<int>(ref buffer, LVLS, offset);
                int size = m_chunkSize;
                offset = SerializeScalar<int>(ref buffer, size, offset);
                offset = SerializeScalar<int>(ref buffer, version, offset);
            }

            // ah: id
            {
                byte[] id = m_id;
                offset = SerializeArray<byte>(ref buffer, id, offset, 1);
            }

            // ah: sgos
            {
                int count = m_sgos == null ? 0 : m_sgos.Count;
                offset = SerializeScalar<int>(ref buffer, count, offset);
                if(count > 0)
                {
                    for(int i = 0; i < count; i++)
                    {
                        ObjectData obj = m_sgos[i];

                        byte[] id = obj.id;
                        offset = SerializeArray<byte>(ref buffer, id, offset, 1);

                        float[] p = new float[3]
                        {
                            obj.position.x,
                            obj.position.y,
                            obj.position.z,
                        };
                        offset = SerializeArray<float>(ref buffer, p, offset);

                        float[] r = new float[4]
                        {
                            obj.rotation.x,
                            obj.rotation.y,
                            obj.rotation.z,
                            obj.rotation.w,
                        };
                        offset = SerializeArray<float>(ref buffer, r, offset);
                    }
                }
            }

            // ah: spawner
            {
                int count = m_spawners == null ? 0 : m_spawners.Count;
                offset = SerializeScalar<int>(ref buffer, count, offset);
                if(count > 0)
                {
                    for(int i = 0; i < count; i++)
                    {
                        ObjectData obj = m_spawners[i];

                        byte[] id = obj.id;
                        offset = SerializeArray<byte>(ref buffer, id, offset, 1);

                        float[] p = new float[3]
                        {
                            obj.position.x,
                            obj.position.y,
                            obj.position.z,
                        };
                        offset = SerializeArray<float>(ref buffer, p, offset);

                        float[] r = new float[4]
                        {
                            obj.rotation.x,
                            obj.rotation.y,
                            obj.rotation.z,
                            obj.rotation.w,
                        };
                        offset = SerializeArray<float>(ref buffer, r, offset);
                    }
                }
            }

            // ah: flipped
            {
                int count = m_flipped == null ? 0 : m_flipped.Count;
                offset = SerializeScalar<int>(ref buffer, count, offset);
                if(count > 0)
                {
                    for(int i = 0; i < count; i++)
                    {
                        FlippedData obj = m_flipped[i];

                        byte[] id = obj.id;
                        offset = SerializeArray<byte>(ref buffer, id, offset, 1);
                        offset = SerializeScalar<int>(ref buffer, obj.m_usesGravity ? 1 : 0, offset);
                    }
                }

            }

            // ah: triggered
            {
                int count = m_triggers == null ? 0 : m_triggers.Count;
                offset = SerializeScalar<int>(ref buffer, count, offset);
                if(count > 0)
                {
                    for(int i = 0; i < count; i++)
                    {
                        ForceInteractorTriggerData obj = m_triggers[i];

                        byte[] id = obj.id;
                        offset = SerializeArray<byte>(ref buffer, id, offset, 1);
                        offset = SerializeScalar<int>(ref buffer, obj.m_hasBeenActivated ? 1 : 0, offset);
                    }
                }

            }

            return offset;
        }
    }

    private struct ChunkLVLS
    {
        public bool m_exists;
        public List<ChunkLVL> m_levels;

        public int m_chunkSize 
        {
            get
            {
                int size = 0;
                if(m_levels != null)
                {
                    for(int i = 0; i < m_levels.Count; i++)
                    {
                        size += m_levels[i].m_chunkSize;
                    }
                }
                return size;
            }
        }

        public int Serialize(byte[] buffer, int offset)
        {

            if(m_levels != null)
            {
                int prev = offset;
                for(int i = 0; i < m_levels.Count; i++)
                {
                    offset = m_levels[i].Serialize(buffer, offset);
                    prev = offset;
                }
            }


            return offset;
        }
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

    private const int LVLS = (((int)'l') << 24) | (((int)'v') << 16) | (((int)'l') << 8) | (((int)'s') << 0);
    private const int STRY = (((int)'s') << 24) | (((int)'t') << 16) | (((int)'r') << 8) | (((int)'y') << 0);
    private const int PLAY = (((int)'p') << 24) | (((int)'l') << 16) | (((int)'a') << 8) | (((int)'y') << 0);
    private const int MAX_INK_VARIABLE_SIZE = 64;

    private static void Serialize(GameState game_state)
    {
        int size = 0;
        // ah: calculate total size of the buffer required
        {
            // ah: play
            if(game_state.m_play.m_exists)
            {
                size += game_state.m_play.m_chunkSize;
            }

            // ah: stry
            if(game_state.m_story.m_exists)
            {
                size += game_state.m_story.m_chunkSize;
            }

            // ah: lvls
            if(game_state.m_levels.m_exists)
            {
                size += game_state.m_levels.m_chunkSize;
            }
        }

        byte[] buffer = new byte[size];
        int offset = 0;

        // ah: serialize game state
        {
            // ah: play
            if(game_state.m_play.m_exists)
            {
                offset = game_state.m_play.Serialize(buffer, offset);
            }

            // ah: stry
            if(game_state.m_story.m_exists)
            {
                int prev = offset;
                offset = game_state.m_story.Serialize(buffer, offset);
            }

            // ah: lvls
            if(game_state.m_levels.m_exists)
            {
                offset = game_state.m_levels.Serialize(buffer, offset);
            }
        }


        // ah: write the file to temporary
        var path = m_dataPath;
        File.WriteAllBytes(path + ".temp", buffer);


        // ah: move/replace the file over
        if(File.Exists(path))
        {
            File.Replace(m_dataPath + ".temp", m_dataPath, null);
        }
        else
        {
            File.Move(m_dataPath + ".temp", m_dataPath);
        }

    }

    private static GameState Deserialize()
    {
        GameState result = new();

        // ah: read the file
        var path = m_dataPath;
        if(File.Exists(path))
        {
            var buffer = File.ReadAllBytes(path);
            int offset = 0;

            result.m_isValid = true;

            for(; offset < buffer.Length;)
            {
                // ah: read header
                int magic = 0;
                offset = DeserializeScalar<int>(ref magic, buffer, offset);

                int size = 0;
                offset = DeserializeScalar<int>(ref size, buffer, offset);

                int version = 0;
                offset = DeserializeScalar<int>(ref version, buffer, offset);

                switch(magic)
                {
                    case LVLS:
                    {
                        ChunkLVL lvl = new();
                        byte[] level_id_bytes = new byte[16];
                        offset = DeserializeArray<byte>(ref level_id_bytes, buffer, offset, 1);
                        lvl.m_id = level_id_bytes;

                        // ah: SerializedGameObject
                        {
                            int count   = 0;
                            offset      = DeserializeScalar<int>(ref count, buffer, offset);

                            if(count > 0)
                            {
                                List<ObjectData> objs = new(count);
                                for(int i = 0; i < count; i++)
                                {
                                    byte[] id = new byte[16];
                                    offset = DeserializeArray<byte>(ref id, buffer, offset, 1);
                                    
                                    float[] position = new float[3];
                                    offset = DeserializeArray<float>(ref position, buffer, offset);


                                    float[] rotation = new float[4];
                                    offset = DeserializeArray<float>(ref rotation, buffer, offset);

                                    objs.Add(new(id, position, rotation));
                                }
                                lvl.m_sgos = objs;
                            }
                        }

                        // ah: Spawners
                        {
                            int count   = 0;
                            offset      = DeserializeScalar<int>(ref count, buffer, offset);

                            if(count > 0)
                            {
                                List<ObjectData> objs = new(count);
                                for(int i = 0; i < count; i++)
                                {
                                    byte[] id = new byte[16];
                                    offset = DeserializeArray<byte>(ref id, buffer, offset, 1);

                                    float[] position = new float[3];
                                    offset = DeserializeArray<float>(ref position, buffer, offset);

                                    float[] rotation = new float[4];
                                    offset = DeserializeArray<float>(ref rotation, buffer, offset);

                                    objs.Add(new(id, position, rotation));
                                }
                                lvl.m_spawners = objs;
                            }

                        }

                        // ah: Flipped
                        {
                            int count   = 0;
                            offset      = DeserializeScalar<int>(ref count, buffer, offset);

                            if(count > 0)
                            {
                                List<FlippedData> objs = new(count);
                                for(int i = 0; i < count; i++)
                                {
                                    byte[] id = new byte[16];
                                    offset = DeserializeArray<byte>(ref id, buffer, offset, 1);

                                    int direction = 0;
                                    offset = DeserializeScalar<int>(ref direction, buffer, offset);

                                    objs[i] = new(id, direction);
                                }
                                lvl.m_flipped = objs;
                            }
                        }

                        // ah: Triggers
                        {
                            int count   = 0;
                            offset      = DeserializeScalar<int>(ref count, buffer, offset);

                            if(count > 0)
                            {
                                List<ForceInteractorTriggerData> objs = new(count);
                                for(int i = 0; i < count; i++)
                                {
                                    byte[] id = new byte[16];
                                    offset = DeserializeArray<byte>(ref id, buffer, offset, 1);

                                    int has_been_activated = 0;
                                    offset = DeserializeScalar<int>(ref has_been_activated, buffer, offset);

                                    objs[i] = new(id, has_been_activated);
                                }
                                lvl.m_triggers = objs;
                            }

                        }

                        // ah: Add the lvl
                        result.m_levels.m_exists = true;
                        if(result.m_levels.m_levels == null)
                        {
                            result.m_levels.m_levels = new();
                        }
                        result.m_levels.m_levels.Add(lvl);
                        break;
                    }
                    case STRY:
                    {
                        // ah: GlobalInkVariableManager variables
                        int count   = 0;
                        offset      = DeserializeScalar<int>(ref count, buffer, offset);

                        Dictionary<string, object> variables = new();
                        if(count > 0)
                        {
                            byte[] variable_buf = new byte[MAX_INK_VARIABLE_SIZE];
                            for(int i = 0; i < count; i++)
                            {
                                // ah: key
                                offset         = DeserializeArray<byte>(ref variable_buf, buffer, offset, 1);
                                int string_len = Array.IndexOf(variable_buf, 0);
                                string_len     = string_len < 0 ? MAX_INK_VARIABLE_SIZE : string_len;
                                string key     = Encoding.UTF8.GetString(variable_buf, 0, string_len);

                                // ah: value
                                offset        = DeserializeArray<byte>(ref variable_buf, buffer, offset, 1);
                                string_len    = Array.IndexOf(variable_buf, 0);
                                string_len    = string_len < 0 ? MAX_INK_VARIABLE_SIZE : string_len;
                                string value  = Encoding.UTF8.GetString(variable_buf, 0, string_len);

                                variables[key] = value;
                            }
                        }

                        ChunkSTRY stry = new();
                        stry.m_exists  = true;
                        stry.m_entries = variables;

                        if(result.m_story.m_exists)
                        {
                            Debug.LogError("Duplicate STRY chunks in save file");
                        }
                        result.m_story = stry;
                        break;
                    }
                    case PLAY:
                    {

                        ChunkPLAY play = new();
                        play.m_exists  = true;

                        // ah: active filter
                        {
                            int filter    = 0;
                            offset        = DeserializeScalar<int>(ref filter, buffer, offset);
                            play.m_filter = (FilterKind)filter;
                        }

                        // ah: unlocked filter
                        {
                            int unlocked          = 0;
                            offset                = DeserializeScalar<int>(ref unlocked, buffer, offset);
                            play.m_unlockedFilter = unlocked == 1;
                        }

                        // ah: unlocked flipped
                        {
                            int unlocked           = 0;
                            offset                 = DeserializeScalar<int>(ref unlocked, buffer, offset);
                            play.m_unlockedFlipped = unlocked == 1;
                        }


                        // ah: player data
                        {
                            float[] position = new float[3];
                            offset = DeserializeArray<float>(ref position, buffer, offset);
                            play.m_playerP = new Vector3(position[0], position[1], position[2]);

                            float[] rotation = new float[4];
                            offset = DeserializeArray<float>(ref rotation, buffer, offset);
                            play.m_playerR = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
                        }

                        // ah: spawn data
                        {
                            float[] position = new float[3];
                            offset = DeserializeArray<float>(ref position, buffer, offset);
                            play.m_spawnP = new Vector3(position[0], position[1], position[2]);

                            float[] rotation = new float[4];
                            offset = DeserializeArray<float>(ref rotation, buffer, offset);
                            play.m_spawnR = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
                        }

                        // ah: spawnpoint build index
                        offset = DeserializeScalar<int>(ref play.m_sceneIndex, buffer, offset);

                        result.m_play = play;
                        break;
                    }
                }
            }
        }
        return result;
    }

    public static LevelData LevelToLoad(LevelsData levels)
    {
        // ah: init gamestate if it doesn't exist
        if(!m_gameState.m_isValid)
        {
            DeserializeAll();
        }

        LevelData result = levels.m_levels[0];
        if(m_gameState.m_isValid)
        {
            ChunkPLAY play = m_gameState.m_play;
            if(play.m_exists)
            {
                // ah: Find the target scene path
                string target_scene = SceneUtility.GetScenePathByBuildIndex(play.m_sceneIndex);


                // ah: See if the target scene path exists within levels 
                for(int i = 0; i < levels.m_levels.Length; i++)
                {
                    LevelData level = levels.m_levels[i];

                    // ah: check if main scene is correct even though
                    // that should never be the case
                    string scene_path = level.m_scenePath + level.m_sceneName + ".unity";

                    if(string.Equals(target_scene, scene_path, StringComparison.OrdinalIgnoreCase))
                    {
                        result = level;
                        break;
                    }


                    // ah: check versus subscenes
                    bool found = false;
                    for(int j = 0; j < level.m_scene.m_subscenes.Length; j++)
                    {
                        Subscene subscene = level.m_scene.m_subscenes[j];

                        string subscene_path = subscene.m_scenePath + subscene.m_scene + ".unity";
                        if(string.Equals(target_scene, subscene_path, StringComparison.OrdinalIgnoreCase))
                        {
                            result = level;
                            found = true;
                            break;
                        }
                    }

                    if(found)
                    {
                        break;
                    }
                }
            }
        }
        return result;
    }

    public static DeserializedPlayerResult DeserializePlayer(Player player)
    {
        // ah: init gamestate if it doesn't exist
        if(!m_gameState.m_isValid)
        {
            DeserializeAll();
        }

        DeserializedPlayerResult result = new();
        if(m_gameState.m_isValid)
        {
            ChunkPLAY play = m_gameState.m_play;
            if(play.m_exists)
            {
                result.found         = true;
                result.active_filter = play.m_filter;
                result.m_unlockedFilter = play.m_unlockedFilter;
                result.m_unlockedFlipped = play.m_unlockedFlipped;

                player.transform.position = play.m_playerP;
                player.transform.rotation = play.m_playerR;

                LevelCheckpointManager.SetNewSpawnPoint(play.m_spawnP, play.m_spawnR, play.m_sceneIndex);
            }
        }

        return result;
    }

    public static void DeserializeLoadedScenes()
    {
        // ah: init gamestate if it doesn't exist
        if(!m_gameState.m_isValid)
        {
            DeserializeAll();
        }

        ChunkSTRY stry = m_gameState.m_story;
        if(stry.m_exists)
        {
            GlobalInkVariableManager.SetVariables(m_gameState.m_story.m_entries);
        }

        ChunkLVLS lvls = m_gameState.m_levels;
        if(lvls.m_exists)
        {
            // ah: Create a dictionary that maps id to data
            Dictionary<Guid, ObjectData> sgos     = new();
            Dictionary<Guid, ObjectData> spawners = new();
            Dictionary<Guid, FlippedData> flipped = new();
            Dictionary<Guid, ForceInteractorTriggerData> triggers = new();

            // ah: map lvl chunks based on id
            for(int i = 0; i < lvls.m_levels.Count; i++)
            {
                ChunkLVL lvl = lvls.m_levels[i];
                for(int j = 0; lvl.m_sgos != null && j < lvl.m_sgos.Count; j++)
                {
                    ObjectData obj = lvl.m_sgos[j];
                    sgos[new Guid(obj.id)] = obj;
                }

                for(int j = 0; lvl.m_spawners != null && j < lvl.m_spawners.Count; j++)
                {
                    ObjectData obj = lvl.m_spawners[j];
                    spawners[new Guid(obj.id)] = obj;
                }

                for(int j = 0; lvl.m_flipped != null && j < lvl.m_flipped.Count; j++)
                {
                    FlippedData obj = lvl.m_flipped[j];
                    flipped[new Guid(obj.id)] = obj;
                }

                for(int j = 0; lvl.m_triggers != null && j < lvl.m_triggers.Count; j++)
                {
                    ForceInteractorTriggerData obj = lvl.m_triggers[j];
                    triggers[new Guid(obj.id)] = obj;
                }
            }

            // ah: Query for objects and check if serialized data exists
            {
                SerializableObject[] objs = UnityEngine.Object.FindObjectsByType<SerializableObject>(FindObjectsSortMode.None);
                for(int i = 0; i < objs.Length; i++)
                {
                    SerializableObject obj = objs[i];
                    var guid = new Guid(obj.m_id);

                    // ah: check if to serialize position and rotation
                    {
                        if(obj.m_serializePositionAndRotation)
                        {
                            if(sgos.ContainsKey(guid))
                            {
                                ObjectData data = sgos[guid];
                                obj.transform.position = data.position;
                                obj.transform.rotation = data.rotation;
                            }
                        }
                    }

                    // ah: check if spawner
                    {
                        Spawner spawner = obj.GetComponent<Spawner>();
                        if(spawner != null)
                        {
                            if(spawners.ContainsKey(guid))
                            {
                                ObjectData data = spawners[guid];
                                spawner.Spawn();
                                spawner.m_object.transform.position = data.position;
                                spawner.m_object.transform.rotation = data.rotation;
                            }
                        }

                    }

                    // ah: check if gravityflipped
                    {
                        GravityFlippedObject flip = obj.GetComponent<GravityFlippedObject>();
                        if(flip != null)
                        {
                            if(flipped.ContainsKey(guid))
                            {
                                FlippedData data = flipped[guid];
                                flip.SetGravity(data.m_usesGravity);
                            }
                        }
                    }

                    // ah: check if trigger
                    {
                        ForceInteractorTrigger trigger = obj.GetComponent<ForceInteractorTrigger>();
                        if(trigger != null)
                        {
                            if(triggers.ContainsKey(guid))
                            {
                                ForceInteractorTriggerData data = triggers[guid];
                                trigger.SetActive(!data.m_hasBeenActivated);
                            }
                        }
                    }

                }
            }
        }
    }
    public static Guid GuidFromStringHash(string str)
    {
        Guid result = new();
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            result = new Guid(hash);
        }
        return result;
    }

    public static void SerializeLoadedScenes(bool save = true)
    {

        // ah: Create a dictionary that maps scenes to a ChunkLVL
        Dictionary<Guid, ChunkLVL> scene_chunks = new();
        int scene_count = SceneManager.loadedSceneCount;
        for(int i = 0; i < scene_count; i++)
        {
            Scene scene      = SceneManager.GetSceneAt(i);
            var id           = GuidFromStringHash(scene.path);
            ChunkLVL lvl     = new();
            lvl.m_id         = id.ToByteArray();
            scene_chunks[id] = lvl;
        }

        // ah: map objects 
        {
            SerializableObject[] sgos = UnityEngine.Object.FindObjectsByType<SerializableObject>(FindObjectsSortMode.None);
            for(int i = 0; i < sgos.Length; i++)
            {
                SerializableObject obj = sgos[i];
                Guid scene_guid = GuidFromStringHash(obj.gameObject.scene.path);
                if(scene_chunks[scene_guid].m_sgos == null)
                {
                    ChunkLVL chunk = scene_chunks[scene_guid];
                    chunk.m_sgos     = new();
                    chunk.m_triggers = new();
                    chunk.m_spawners = new();
                    chunk.m_flipped  = new();
                    scene_chunks[scene_guid] = chunk;
                }

                // ah: sgos
                {
                    if(obj.m_serializePositionAndRotation)
                    {
                        float[] p = new float[3]
                        {
                            obj.transform.position.x,
                            obj.transform.position.y,
                            obj.transform.position.z,
                        };

                        float[] r = new float[4]
                        {
                            obj.transform.rotation.x,
                            obj.transform.rotation.y,
                            obj.transform.rotation.z,
                            obj.transform.rotation.w,
                        };
                        ObjectData data = new(obj.m_id, p, r);
                        scene_chunks[scene_guid].m_sgos.Add(data);
                    }
                }

                // ah:spawners
                {
                    Spawner spawner = obj.GetComponent<Spawner>();
                    if(spawner != null)
                    {
                        GameObject child = spawner.m_object;
                        if(scene_chunks[scene_guid].m_spawners == null)
                        {
                            ChunkLVL chunk = scene_chunks[scene_guid];
                            chunk.m_spawners   = new();
                            scene_chunks[scene_guid] = chunk;
                        }
                        float[] p = new float[3]
                        {
                            child.transform.position.x,
                            child.transform.position.y,
                            child.transform.position.z,
                        };

                        float[] r = new float[4]
                        {
                            child.transform.rotation.x,
                            child.transform.rotation.y,
                            child.transform.rotation.z,
                            child.transform.rotation.w,
                        };
                        scene_chunks[scene_guid].m_spawners.Add(new(obj.m_id, p, r));
                    }
                }

                // ah: gravity flipped
                {
                    GravityFlippedObject flip = obj.GetComponent<GravityFlippedObject>();
                    if(flip != null)
                    {
                        scene_chunks[scene_guid].m_flipped.Add(new(obj.m_id, flip.m_useGravity ? 1 : 0));
                    }
                }

                // ah: Force InteractorTrigger
                {
                    ForceInteractorTrigger trigger = obj.GetComponent<ForceInteractorTrigger>();
                    if(trigger != null)
                    {
                        scene_chunks[scene_guid].m_triggers.Add(new(obj.m_id, trigger.m_hasBeenActivated ? 1 : 0));
                    }

                }
            }
        }

        // ah: write new level state
        m_gameState.m_isValid = true;
        m_gameState.m_levels.m_levels = new();
        m_gameState.m_levels.m_exists = true;

        foreach(KeyValuePair<Guid, ChunkLVL> kv in scene_chunks)
        {
            m_gameState.m_levels.m_levels.Add(kv.Value);
        }

        if(save)
        {
            Serialize(m_gameState);
        }
    }

    public static void DeserializeAll()
    {
        m_gameState = Deserialize();
    }

    public static void SerializeAll()
    {
        // ah: serialize PLAY to GameState
        {
            m_gameState.m_play.m_exists = true;

            // ah: filter
            {
                m_gameState.m_play.m_filter         = FilterManager.m_activeFilter;
                m_gameState.m_play.m_unlockedFilter = FilterManager.m_filterUnlocked;
            }

            // ah: flipped
            m_gameState.m_play.m_unlockedFlipped = GravityFlippedManager.m_unlocked;

            // ah: player
            Player player = UnityEngine.Object.FindFirstObjectByType<Player>();
            if(player != null)
            {
                m_gameState.m_play.m_playerP = player.transform.position;
                m_gameState.m_play.m_playerP = player.transform.position;
            }
            else
            {
                Debug.LogError("Tried to serialize game without a player?");
            }

            // ah: spawn
            {
                m_gameState.m_play.m_spawnP     = LevelCheckpointManager.m_currentSpawnPointPosition;
                m_gameState.m_play.m_spawnR     = LevelCheckpointManager.m_currentSpawnPointRotation;
                m_gameState.m_play.m_sceneIndex = LevelCheckpointManager.m_sceneBuildIndex;
            }
        }

        // ah: serialize STRY to GameState
        {
            m_gameState.m_story.m_exists  = true;
            m_gameState.m_story.m_entries = new();
            foreach(KeyValuePair<string, object> kv in GlobalInkVariableManager.m_inkVariables)
            {
                m_gameState.m_story.m_entries[kv.Key] = kv.Value;
            }
            Debug.Log($"Serializing {m_gameState.m_story.m_entries.Count} ink entries");
        }

        SerializeLoadedScenes(false);
        Serialize(m_gameState);
    }

}
