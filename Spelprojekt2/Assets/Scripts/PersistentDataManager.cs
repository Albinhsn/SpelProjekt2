using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections.Generic;
using static Serialization;

/*
 v0 format for scene:
  Magic (SCEN) (32bit)
  Version (32bit)
  Count of objects (32bit)
  ID, Position, Rotation

 v0 format for player:
  Magic (PLAY) (32bit)
  Version (32bit)
  Filter active (32bit)
  Position, Rotation,
  Latest spawn position

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

    public PersistentDataManager()
    {
    }

    public static void RemoveSceneData(int scene_build_index)
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(scene_build_index);
        var path = Path.Combine(Application.persistentDataPath, $"{scene.name}.bin");
        if(File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static void RemoveAllSerializedData()
    {
        for(int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            Scene scene = SceneManager.GetSceneByBuildIndex(i);
            var path = Path.Combine(Application.persistentDataPath, $"{scene.name}.bin");
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        {
            var path = Path.Combine(Application.persistentDataPath, $"player.bin");
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }


    public static void SerializeLoadedLevels()
    {
        for(int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if(scene.isLoaded)
            {
                List<SerializableObject> objs = new();
                foreach(var root in scene.GetRootGameObjects())
                {
                    objs.AddRange(root.GetComponentsInChildren<SerializableObject>(true));
                }
                PersistentDataManager.SerializeScene(scene.name, objs.ToArray());
            }
        }
    }

    public static void SerializePlayer(Player player)
    {
        // Magic, Version
        int header_size = sizeof(int) * 2;

        // Filter active, Position, Rotation, spawn point position, spawn point rotation, scene build index
        int obj_size    = sizeof(float) * 14 + sizeof(int) * 2; 

        int total_size = header_size + obj_size;
        byte[] buffer  = new byte[total_size];

        // Magic
        buffer[0]  = (byte)'P';
        buffer[1]  = (byte)'L';
        buffer[2]  = (byte)'A';
        buffer[3]  = (byte)'Y';
        int offset = 4;

        // Version
        offset = SerializeScalar<int>(ref buffer, 0, offset);

        // Filter active
        offset = SerializeScalar<int>(ref buffer, (int)FilterManager.m_activeFilter, offset);

        // Position
        float[] pos = new float[3]
        {
            player.transform.position.x,
            player.transform.position.y,
            player.transform.position.z
        };
        offset = memcpy(ref buffer, SerializeArray(pos), offset);


        // Rotation 
        float[] rot = new float[4] 
        {
            player.transform.rotation.x,
            player.transform.rotation.y,
            player.transform.rotation.z,
            player.transform.rotation.w
        };
        offset = memcpy(ref buffer, SerializeArray(rot), offset);

        float[] spawn_point_position = new float[3]
        {
            LevelCheckpointManager.m_currentSpawnPointPosition.x,
            LevelCheckpointManager.m_currentSpawnPointPosition.y,
            LevelCheckpointManager.m_currentSpawnPointPosition.z,
        };
        offset = memcpy(ref buffer, SerializeArray(spawn_point_position), offset);

        float[] spawn_point_rotation = new float[4]
        {
            LevelCheckpointManager.m_currentSpawnPointRotation.x,
            LevelCheckpointManager.m_currentSpawnPointRotation.y,
            LevelCheckpointManager.m_currentSpawnPointRotation.z,
            LevelCheckpointManager.m_currentSpawnPointRotation.w,
        };
        offset = memcpy(ref buffer, SerializeArray(spawn_point_rotation), offset);

        offset = SerializeScalar<int>(ref buffer, LevelCheckpointManager.m_sceneBuildIndex, offset);

        var path = Path.Combine(Application.persistentDataPath, "player.bin");
        File.WriteAllBytes(path, buffer);

    }

    private struct DeserializedPlayer
    {
        public uint magic;
        public int version;
        public int filter;
        public Vector3 player_p;
        public Quaternion player_r;
        public Vector3 spawn_p;
        public Quaternion spawn_r;
        public int scene_index;
    }

    public static LevelData LevelToLoad(LevelsData levels)
    {
        var path = Path.Combine(Application.persistentDataPath, "player.bin");

        LevelData result = levels.m_levels[0];
        if(File.Exists(path))
        {
            byte[] buffer = File.ReadAllBytes(path);
            DeserializedPlayer player = DeserializePlayer(buffer);

            string scene_path = SceneUtility.GetScenePathByBuildIndex(player.scene_index);


            for(int i = 0; i < levels.m_levels.Length; i++)
            {
                LevelData level = levels.m_levels[i];
                if(scene_path.Equals(level.m_scenePath + level.m_sceneName + ".unity", StringComparison.OrdinalIgnoreCase))
                {
                    result = level;
                    break;
                }

                bool found = false;
                SceneData scene_data = level.m_scene;
                for(int j = 0; j < scene_data.m_subscenes.Length; j++)
                {
                    Subscene subscene = scene_data.m_subscenes[j];
                    if(scene_path.Equals(subscene.m_scenePath + subscene.m_scene + ".unity", StringComparison.OrdinalIgnoreCase))
                    {
                        result = level;
                        found = true;
                        break;
                    }

                }

                if(found) break;
            }
        }

        return result;
    }

    private static DeserializedPlayer
    DeserializePlayer(byte[] buffer)
    {
        DeserializedPlayer result = new();
        int offset = 0;

        offset = DeserializeScalar<uint>(ref result.magic, buffer, offset);
        offset = DeserializeScalar<int>(ref result.version, buffer, offset);
        offset = DeserializeScalar<int>(ref result.filter, buffer, offset);

        // Position
        float[] position = new float[3];
        offset = DeserializeArray<float>(ref position, buffer, offset);
        result.player_p = new Vector3(position[0], position[1], position[2]);

        // Rotation
        float[] rotation = new float[4];
        offset = DeserializeArray<float>(ref rotation, buffer, offset);
        result.player_r = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);


        float[] spawn_point_position = new float[3];
        offset = DeserializeArray<float>(ref spawn_point_position, buffer, offset);
        result.spawn_p = new Vector3(spawn_point_position[0], spawn_point_position[1], spawn_point_position[2]);

        float[] spawn_point_rotation = new float[4];
        offset = DeserializeArray<float>(ref spawn_point_rotation, buffer, offset);
        result.spawn_r = new Quaternion(spawn_point_rotation[0], spawn_point_rotation[1], spawn_point_rotation[2], spawn_point_rotation[3]);

        offset         = DeserializeScalar<int>(ref result.scene_index, buffer, offset);
        return result;
    }


    public static DeserializedPlayerResult
    DeserializePlayer(Player player)
    {
        var path = Path.Combine(Application.persistentDataPath, "player.bin");

        DeserializedPlayerResult result = new();
        result.active_filter = FilterKind.None;

        if(File.Exists(path))
        {
            result.found = true;
            byte[] buffer = File.ReadAllBytes(path);

            DeserializedPlayer data = DeserializePlayer(buffer);

            player.gameObject.transform.position = data.player_p;
            player.gameObject.transform.rotation = data.player_r;
            LevelCheckpointManager.SetNewSpawnPoint(data.spawn_p, data.spawn_r, data.scene_index);
        }
        else
        {
            Debug.Log("[PDM] Didn't find any player data to deserialize");
        }

        return result;
    }

    public static void DeserializeScene(String name, SerializableObject[] objs)
    {
        var path = Path.Combine(Application.persistentDataPath, $"{name}.bin");

        if(File.Exists(path))
        {
            byte[] buffer = File.ReadAllBytes(path);
            int offset = 0;

            // Magic
            {
                uint magic = 0;
                offset = DeserializeScalar<uint>(ref magic, buffer, offset);

                char b0 = (char)((magic >> 0 ) & 0xFF);
                char b1 = (char)((magic >> 8 ) & 0xFF);
                char b2 = (char)((magic >> 16) & 0xFF);
                char b3 = (char)((magic >> 24) & 0xFF);

            }


            int version = 0;
            offset = DeserializeScalar<int>(ref version, buffer, offset);

            int object_count = 0;
            offset = DeserializeScalar<int>(ref object_count, buffer, offset);


            Dictionary<string, ObjectData> obj_dict = new();
            for(int i = 0; i < object_count; i++)
            {
                byte[] guid_buf  = new byte[16];
                offset = DeserializeArray<byte>(ref guid_buf, buffer, offset, 1);

                float[] position = new float[3];
                offset = DeserializeArray<float>(ref position, buffer, offset);

                float[] rotation = new float[4];
                offset = DeserializeArray<float>(ref rotation, buffer, offset);

                obj_dict[new Guid(guid_buf).ToString()] = new ObjectData(position, rotation);
            }

            for(int i = 0; i < objs.Length; i++)
            {
                SerializableObject obj = objs[i];
                string key = obj.m_ID.ToString();
                if(obj_dict.ContainsKey(key))
                {
                    ObjectData data = obj_dict[key];

                    obj.gameObject.transform.position = data.position;
                    obj.gameObject.transform.rotation = data.rotation;
                }
            }

        }
        else
        {
            Debug.Log($"[PDM] Couldn't find persistent data for '{name}' in {path}");
        }
    }

    public static void SerializeScene(String name, SerializableObject[] objs)
    {

        // Magic, Version, Object count
        int header_size = sizeof(int) * 3;


        // 16-byte GUID, Position, Rotation
        int id_size     = sizeof(byte) * 16;
        int obj_size    = sizeof(float) * 7 + id_size; 

        int total_size = header_size + obj_size * objs.Length;
        byte[] buffer  = new byte[total_size];

        // Magic
        buffer[0]  = (byte)'S';
        buffer[1]  = (byte)'C';
        buffer[2]  = (byte)'E';
        buffer[3]  = (byte)'N';
        int offset = 4;

        // Version
        offset = SerializeScalar<int>(ref buffer, 0, offset);

        // Object count
        offset = SerializeScalar<int>(ref buffer, objs.Length, offset);

        foreach(SerializableObject obj in objs)
        {
            // ID
            offset = memcpy(ref buffer, obj.m_ID.ToByteArray(), offset);

            // Position
            float[] pos = new float[3]
            {
                obj.transform.position.x,
                obj.transform.position.y,
                obj.transform.position.z
            };
            offset = memcpy(ref buffer, SerializeArray(pos), offset);


            // Rotation 
            float[] rot = new float[4] 
            {
                obj.transform.rotation.x,
                obj.transform.rotation.y,
                obj.transform.rotation.z,
                obj.transform.rotation.w
            };
            offset = memcpy(ref buffer, SerializeArray(rot), offset);

        }
        

        var path = Path.Combine(Application.persistentDataPath, $"{name}.bin");
        Debug.Log($"[PDM] Serializing {name}, offset: {offset}, expected {total_size} to {path}");
        File.WriteAllBytes(path, buffer);
    }





}
