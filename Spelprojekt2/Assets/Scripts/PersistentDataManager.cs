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
    private struct GameState
    {
        public bool m_isValid;
        public ChunkPLAY m_play;
        public ChunkSTRY m_story;
        public ChunkLVLS m_levels;
    }

    private static GameState m_gameState;
    private static string m_dataPath => Path.Combine(Application.persistentDataPath, "game.bin");

    private struct InkAssetRegistryData
    {
        public byte[] id;
        public int m_activeAssetIndex;

        public InkAssetRegistryData(byte[] id, int active_asset_index)
        {
            this.id = id;
            this.m_activeAssetIndex = active_asset_index;
        }

        public int Serialize(ref byte[] buffer, int offset)
        {
            offset = SerializeArray<byte>(ref buffer, id, offset, 4);
            offset = SerializeScalar<int>(ref buffer, m_activeAssetIndex, offset, 4);
            return offset;
        }
        
        public static InkAssetRegistryData Deserialize(byte[] buffer, ref int offset)
        {
            byte[] id = new byte[16];
            offset = DeserializeArray<byte>(ref id, buffer, offset, 4);

            int active_asset_index = 0;
            offset = DeserializeScalar<int>(ref active_asset_index, buffer, offset, 4);

            return new InkAssetRegistryData(id, active_asset_index);
        }

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
        public int Serialize(ref byte[] buffer, int offset)
        {
            offset = SerializeArray<byte>(ref buffer, id, offset, 1);
            offset = SerializeScalar<int>(ref buffer, m_hasBeenActivated ? 1 : 0, offset);
            return offset;
        }

        public static ForceInteractorTriggerData Deserialize(byte[] buffer, ref int offset)
        {
            byte[] id = new byte[16];
            offset = DeserializeArray<byte>(ref id, buffer, offset, 1);

            int has_been_activated = 0;
            offset = DeserializeScalar<int>(ref has_been_activated, buffer, offset);

            return new(id, has_been_activated);
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

        public int Serialize(ref byte[] buffer, int offset)
        {
            offset = SerializeArray<byte>(ref buffer, id, offset, 1);
            offset = SerializeScalar<int>(ref buffer, m_usesGravity ? 1 : 0, offset);
            return offset;
        }
        public static FlippedData Deserialize(byte[] buffer, ref int offset)
        {
            FlippedData data = new();
            data.id = new byte[16];
            offset = DeserializeArray<byte>(ref data.id, buffer, offset, 1);
            int uses_gravity = 0;
            offset = DeserializeScalar<int>(ref uses_gravity, buffer, offset);
            data.m_usesGravity = uses_gravity == 1 ? true : false;
            return data;
        }
        
    }

    private struct ObjectData
    {
        public byte[] id;
        public Vector3 position;
        public Quaternion rotation;

        public ObjectData(byte[] id, Vector3 p, Quaternion r)
        {
            this.id       = id;
            this.position = p;
            this.rotation = r;
        }

        public int Serialize(ref byte[] buffer, int offset)
        {
            offset = SerializeArray<byte>(ref buffer, this.id, offset, 1);
            offset = SerializeVector3(ref buffer, this.position, offset);
            offset = SerializeQuaternion(ref buffer, this.rotation, offset);
            return offset;
        }

        public static ObjectData Deserialize(byte[] buffer, ref int offset)
        {
            ObjectData data = new();
            data.id = new byte[16];
            offset = DeserializeArray<byte>(ref data.id, buffer, offset, 1);
            offset = DeserializeVector3(ref data.position, buffer, offset);
            offset = DeserializeQuaternion(ref data.rotation, buffer, offset);
            return data;
        }
    }

    private struct ChunkPLAY 
    {
        const int version = 0;

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

        public int Serialize(byte[] buffer, int offset)
        {

            // ah: header
            offset = SerializeScalar<int>(ref buffer, PLAY, offset);
            offset = SerializeScalar<int>(ref buffer, m_chunkSize, offset);
            offset = SerializeScalar<int>(ref buffer, version, offset);

            // ah: filter
            offset = SerializeScalar<int>(ref buffer, (int)m_filter, offset);
            offset = SerializeScalar<int>(ref buffer, m_unlockedFilter ? 1 : 0, offset);

            // ah: flipped
            offset = SerializeScalar<int>(ref buffer, m_unlockedFlipped ? 1 : 0, offset);

            // ah: player 
            offset = SerializeVector3(ref buffer, m_playerP, offset);
            offset = SerializeQuaternion(ref buffer, m_playerR, offset);

            // ah: spawn
            offset = SerializeVector3(ref buffer, m_spawnP, offset);
            offset = SerializeQuaternion(ref buffer, m_spawnR, offset);
            offset = SerializeScalar<int>(ref buffer, m_sceneIndex, offset);

            return offset;
        }
    }

    private struct ChunkSTRY
    {
        private const int version = 0;
        public bool m_exists;
        public Dictionary<string, object> m_entries;

        public int m_chunkSize
        {
            get
            {
                return sizeof(int) * 3 + sizeof(int) + MAX_INK_VARIABLE_SIZE * 2 * m_entries.Count;
            }
        }


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
                    offset = SerializeString(kv.Key, buffer, offset, MAX_INK_VARIABLE_SIZE);

                    // ah: value
                    string value;
                    if(kv.Value is bool)
                    {
                        value = (bool)kv.Value ? "true" : "false";
                    }
                    else if(kv.Value is string)
                    {
                        value = (string)kv.Value;
                    }
                    else
                    {
                        Debug.LogError($"Unable to serialize ink variable of this type {kv.Value.GetType().ToString()}");
                        value = "";
                    }
                    offset = SerializeString(value, buffer, offset, MAX_INK_VARIABLE_SIZE);
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
        public List<InkAssetRegistryData> m_iaregs;

        private const int version = 0;

        public int m_chunkSize
        {
            get
            {
                // ah: header
                int size = sizeof(int) * 3;

                // ah: lvl id
                size += 16;

                // ah: size of ObjectData guid, pos, rotation
                int size_of_object_data = 16 + sizeof(float) * 3 + sizeof(float) * 4;

                // ah: the fucking size of the arrays (spent some time debuggin this :) )
                size += sizeof(int) * 4;

                // ah: sgo and spawners
                size += size_of_object_data * (m_sgos != null ? m_sgos.Count : 0);
                size += size_of_object_data * (m_spawners != null ? m_spawners.Count : 0);

                // ah: gravity flipped objects
                int size_of_flipped_data = 16 + sizeof(int);
                size += size_of_flipped_data * (m_flipped != null ? m_flipped.Count : 0);

                // ah: force interaction trigger data
                int size_of_trigger_data = 16 + sizeof(int);
                size += size_of_trigger_data * (m_triggers != null ? m_triggers.Count : 0);
                
                // sh: ink asset registry data (am I doing this right?)
                int size_of_iar_data = 16 + sizeof(int);
                size += size_of_iar_data += size_of_iar_data * (m_iaregs?.Count ?? 0);

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
            byte[] id = m_id;
            offset = SerializeArray<byte>(ref buffer, id, offset, 1);

            // ah: sgos
            int count = m_sgos == null ? 0 : m_sgos.Count;
            offset = SerializeScalar<int>(ref buffer, count, offset);
            for(int i = 0; i < count; i++)
            {
                offset = m_sgos[i].Serialize(ref buffer, offset);
            }

            // ah: spawner
            count = m_spawners == null ? 0 : m_spawners.Count;
            offset = SerializeScalar<int>(ref buffer, count, offset);
            for(int i = 0; i < count; i++)
            {
                offset = m_spawners[i].Serialize(ref buffer, offset);
            }

            // ah: flipped
            count = m_flipped == null ? 0 : m_flipped.Count;
            offset = SerializeScalar<int>(ref buffer, count, offset);
            for(int i = 0; i < count; i++)
            {
                offset = m_flipped[i].Serialize(ref buffer, offset);
            }

            // ah: triggered
            count = m_triggers == null ? 0 : m_triggers.Count;
            offset = SerializeScalar<int>(ref buffer, count, offset);
            for(int i = 0; i < count; i++)
            {
                offset = m_triggers[i].Serialize(ref buffer, offset);
            }
            
            // sh: inkAssetRegistries
            count = m_iaregs?.Count ?? 0;
            offset = SerializeScalar<int>(ref buffer, count, offset);
            for (int i = 0; i < count; i++)
            {
                offset = m_iaregs[i].Serialize(ref buffer, offset);
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
                for(int i = 0; m_levels != null && i < m_levels.Count; i++)
                {
                    size += m_levels[i].m_chunkSize;
                }
                return size;
            }
        }

        public int Serialize(byte[] buffer, int offset)
        {
            for(int i = 0; m_levels != null && i < m_levels.Count; i++)
            {
                offset = m_levels[i].Serialize(buffer, offset);
            }
            return offset;
        }
    }



    public static void DeleteSave()
    {
        if(File.Exists(m_dataPath))
        {
            File.Delete(m_dataPath);
        }
        m_gameState.m_isValid = false;
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
            size += game_state.m_play.m_exists ? game_state.m_play.m_chunkSize : 0;
            size += game_state.m_story.m_exists ? game_state.m_story.m_chunkSize : 0;
            size += game_state.m_levels.m_exists ? game_state.m_levels.m_chunkSize : 0;
        }

        byte[] buffer = new byte[size];
        int offset = 0;

        // ah: serialize game state
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
                        lvl.m_id = new byte[16];
                        offset = DeserializeArray<byte>(ref lvl.m_id, buffer, offset, 1);

                        // ah: SerializedGameObject
                        int count   = 0;
                        offset      = DeserializeScalar<int>(ref count, buffer, offset);
                        lvl.m_sgos = new(count);
                        for(int i = 0; i < count; i++)
                        {
                            lvl.m_sgos.Add(ObjectData.Deserialize(buffer, ref offset));
                        }

                        // ah: Spawners
                        offset      = DeserializeScalar<int>(ref count, buffer, offset);
                        lvl.m_spawners = new(count);
                        for(int i = 0; i < count; i++)
                        {
                            lvl.m_spawners.Add(ObjectData.Deserialize(buffer, ref offset));
                        }

                        // ah: Flipped
                        offset      = DeserializeScalar<int>(ref count, buffer, offset);
                        lvl.m_flipped = new(count);
                        for(int i = 0; i < count; i++)
                        {
                            lvl.m_flipped.Add(FlippedData.Deserialize(buffer, ref offset));
                        }

                        // ah: Triggers
                        offset      = DeserializeScalar<int>(ref count, buffer, offset);
                        lvl.m_triggers = new(count);
                        for(int i = 0; i < count; i++)
                        {
                            lvl.m_triggers.Add(ForceInteractorTriggerData.Deserialize(buffer, ref offset));
                        }
                        
                        // sh: InkAssetRegistries
                        offset = DeserializeScalar<int>(ref count, buffer, offset);
                        lvl.m_iaregs = new List<InkAssetRegistryData>(count);
                        for (int i = 0; i < count; i++)
                        {
                            lvl.m_iaregs.Add(InkAssetRegistryData.Deserialize(buffer, ref offset));
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
                        string key = "", value = "";
                        for(int i = 0; i < count; i++)
                        {
                            offset         = DeserializeString(ref key, buffer, offset, MAX_INK_VARIABLE_SIZE);
                            offset         = DeserializeString(ref value, buffer, offset, MAX_INK_VARIABLE_SIZE);
                            variables[key] = value;
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
                        int filter    = 0;
                        offset        = DeserializeScalar<int>(ref filter, buffer, offset);
                        play.m_filter = (FilterKind)filter;

                        // ah: unlocked filter
                        int unlocked          = 0;
                        offset                = DeserializeScalar<int>(ref unlocked, buffer, offset);
                        play.m_unlockedFilter = unlocked == 1;

                        // ah: unlocked flipped
                        offset                 = DeserializeScalar<int>(ref unlocked, buffer, offset);
                        play.m_unlockedFlipped = unlocked == 1;


                        // ah: player data
                        offset = DeserializeVector3(ref play.m_playerP, buffer, offset);
                        offset = DeserializeQuaternion(ref play.m_playerR, buffer, offset);

                        // ah: spawn data
                        offset = DeserializeVector3(ref play.m_spawnP, buffer, offset);
                        offset = DeserializeQuaternion(ref play.m_spawnR, buffer, offset);

                        // ah: spawnpoint build index
                        offset = DeserializeScalar<int>(ref play.m_sceneIndex, buffer, offset);

                        result.m_play = play;
                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Tried to deserialize but file at {path} didn't exist");
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
                Debug.Log($"Found target scene {target_scene}");


                // ah: See if the target scene path exists within levels 
                bool found = false;
                for(int i = 0; i < levels.m_levels.Length; i++)
                {
                    LevelData level = levels.m_levels[i];

                    // ah: check if main scene is correct even though
                    // that should never be the case
                    string scene_path = level.m_scenePath + level.m_sceneName + ".unity";


                    if(string.Equals(target_scene, scene_path, StringComparison.OrdinalIgnoreCase))
                    {
                        result = level;
                        found = true;
                        break;
                    }


                    // ah: check versus subscenes
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
                if(!found)
                {
                    Debug.LogError("Couldn't find the target scene");
                }
            }
            else
            {
                Debug.LogError("Tried to deserialize with no valid PLAY");
            }
        }
        else
        {
            Debug.LogError("Tried to deserialize with no valid gamestate");
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
            Dictionary<Guid, InkAssetRegistryData> iaregs = new();

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
                
                for(int j = 0; lvl.m_iaregs != null && j < lvl.m_iaregs.Count; j++)
                {
                    InkAssetRegistryData obj = lvl.m_iaregs[j];
                    iaregs[new Guid(obj.id)] = obj;
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
                    if(obj.m_serializePositionAndRotation)
                    {
                        if(sgos.ContainsKey(guid))
                        {
                            ObjectData data = sgos[guid];
                            obj.transform.position = data.position;
                            obj.transform.rotation = data.rotation;
                        }
                    }

                    // ah: check if spawner
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

                    // ah: check if gravityflipped
                    GravityFlippedObject flip = obj.GetComponent<GravityFlippedObject>();
                    if(flip != null)
                    {
                        if(flipped.ContainsKey(guid))
                        {
                            flip.SetGravity(flipped[guid].m_usesGravity);
                        }
                    }

                    // ah: check if trigger
                    ForceInteractorTrigger trigger = obj.GetComponent<ForceInteractorTrigger>();
                    if(trigger != null)
                    {
                        if(triggers.ContainsKey(guid))
                        {
                            ForceInteractorTriggerData data = triggers[guid];
                            trigger.SetActive(!data.m_hasBeenActivated);
                        }
                    }

                    InkAssetRegistry iareg = obj.GetComponent<InkAssetRegistry>();
                    if (iareg != null)
                    {
                        if (iaregs.ContainsKey(guid))
                        {
                            InkAssetRegistryData data = iaregs[guid];
                            iareg.SetActiveAsset(data.m_activeAssetIndex);
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
        for(int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            var id           = GuidFromStringHash(SceneManager.GetSceneAt(i).path);
            ChunkLVL lvl     = new();
            lvl.m_id         = id.ToByteArray();
            scene_chunks[id] = lvl;
        }

        // ah: map objects 
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
                chunk.m_iaregs = new List<InkAssetRegistryData>();
                scene_chunks[scene_guid] = chunk;
            }

            // ah: sgos
            if(obj.m_serializePositionAndRotation)
            {
                ObjectData data = new(obj.m_id, obj.transform.position, obj.transform.rotation);
                scene_chunks[scene_guid].m_sgos.Add(data);
            }

            // ah:spawners
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
                if(child != null)
                {
                    scene_chunks[scene_guid].m_spawners.Add(new(obj.m_id, child.transform.position, child.transform.rotation));
                }
            }

            // ah: gravity flipped
            GravityFlippedObject flip = obj.GetComponent<GravityFlippedObject>();
            if(flip != null)
            {
                scene_chunks[scene_guid].m_flipped.Add(new(obj.m_id, flip.m_useGravity ? 1 : 0));
            }

            // ah: Force InteractorTrigger
            ForceInteractorTrigger trigger = obj.GetComponent<ForceInteractorTrigger>();
            if(trigger != null)
            {
                scene_chunks[scene_guid].m_triggers.Add(new(obj.m_id, trigger.m_hasBeenActivated ? 1 : 0));
            }
            
            // sh: InkAssetRegistry
            InkAssetRegistry iareg = obj.GetComponent<InkAssetRegistry>();
            if (iareg != null)
            {
                scene_chunks[scene_guid].m_iaregs.Add(new InkAssetRegistryData(obj.m_id, iareg.activeAssetIndex));
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
        m_gameState.m_play.m_exists = true;

        // ah: filter
        m_gameState.m_play.m_filter         = FilterManager.m_activeFilter;
        m_gameState.m_play.m_unlockedFilter = FilterManager.m_filterUnlocked;

        // ah: flipped
        m_gameState.m_play.m_unlockedFlipped = GravityFlippedManager.m_unlocked;

        // ah: player
        Player player = UnityEngine.Object.FindFirstObjectByType<Player>();
        PlayerTransformHandler handler = UnityEngine.Object.FindFirstObjectByType<PlayerTransformHandler>();
        if(player != null && handler != null)
        {
            m_gameState.m_play.m_playerP = player.transform.position;
            m_gameState.m_play.m_playerR = handler.transform.rotation;
        }
        else
        {
            Debug.LogError("Tried to serialize game without a player?");
        }

        // ah: spawn
        m_gameState.m_play.m_spawnP     = LevelCheckpointManager.m_currentSpawnPointPosition;
        m_gameState.m_play.m_spawnR     = LevelCheckpointManager.m_currentSpawnPointRotation;
        m_gameState.m_play.m_sceneIndex = LevelCheckpointManager.m_sceneBuildIndex;


        // ah: serialize STRY to GameState
        m_gameState.m_story.m_exists  = true;
        m_gameState.m_story.m_entries = new();
        foreach(KeyValuePair<string, object> kv in GlobalInkVariableManager.m_inkVariables)
        {
            m_gameState.m_story.m_entries[kv.Key] = kv.Value;
        }

        SerializeLoadedScenes(false);
        Serialize(m_gameState);
    }

}
