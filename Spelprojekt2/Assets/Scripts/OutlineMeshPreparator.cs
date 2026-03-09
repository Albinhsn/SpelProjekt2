
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class OutlineMeshPreparator : MonoBehaviour
{
    [SerializeField] private GameObject m_srcMeshObject;
    [SerializeField] private float m_weldThreshold = float.Epsilon;
    [SerializeField] private bool m_recalculate;
    private bool m_useSkinnedMeshRenderer;

    private void OnValidate()
    {
        if (m_srcMeshObject.IsUnityNull())
        {
            Debug.LogError("Outline mesh source object has not been assigned", this);
            return;
        }
        if (!m_srcMeshObject.TryGetComponent(out SkinnedMeshRenderer smr) &&
            !m_srcMeshObject.TryGetComponent(out MeshRenderer mr))
        {
            Debug.LogError("Outline mesh source object does not contain a MeshRenderer or SkinnedMeshRenderer", m_srcMeshObject);
            return;
        }
        
        CalculateMesh();
    }

    private void Awake()
    {
        CalculateMesh();
    }

    private void CalculateMesh()
    {
        m_useSkinnedMeshRenderer = m_srcMeshObject.TryGetComponent(out SkinnedMeshRenderer smr);

        if (m_useSkinnedMeshRenderer)
        {
            SkinnedMeshRenderer src_renderer = smr;

            Mesh src_mesh = src_renderer.sharedMesh;
            Mesh dst_mesh;

            List<VertexData> vertex_collection = new List<VertexData>();
            BoneWeight1[] bone_weights = src_mesh.GetAllBoneWeights().ToArray();
            int next_weight_index = 0;

            for (int a = 0; a < src_mesh.vertexCount; a++) //Collect vertex data
            {
                bool add_new = true;
                for (int b = 0; b < vertex_collection.Count; b++)
                {
                    if (Vector3.Distance(vertex_collection[b].position, src_mesh.vertices[a]) <=
                        m_weldThreshold) //Add to existing vertex entry
                    {
                        vertex_collection[b].indices.Add(a);
                        vertex_collection[b].normals.Add(src_mesh.normals[a]);
                        add_new = false;
                        break;
                    }
                }

                if (add_new) //Create new vertex entry
                {
                    VertexData entry = new VertexData();
                    entry.position = src_mesh.vertices[a];
                    entry.indices.Add(a);
                    entry.normals.Add(src_mesh.normals[a]);

                    entry.weight_count = src_mesh.GetBonesPerVertex()[a];
                    entry.weights = bone_weights.Skip(next_weight_index).Take(entry.weight_count).ToArray();
                    next_weight_index += entry.weight_count;

                    vertex_collection.Add(entry);
                }
            }
            
            int[] tris = new int[src_mesh.triangles.Length];
            for (int a = 0; a < tris.Length; a++)//Tris
            {
                for (int b = 0; b < vertex_collection.Count; b++)//Lookup in new vertex list
                {
                    if (vertex_collection[b].ContainsIndex(src_mesh.triangles[a]))
                    {
                        tris[a] = b;
                        break;
                    }
                }
            }

            dst_mesh = new Mesh();
            Vector3[] normals = new Vector3[vertex_collection.Count];
            Vector3[] positions = new Vector3[vertex_collection.Count];
            List<BoneWeight1> weights = new List<BoneWeight1>();
            byte[] weight_counts = new Byte[vertex_collection.Count];

            for (int a = 0; a < vertex_collection.Count; a++)
            {
                normals[a] = vertex_collection[a].GetSharedNormal();
                positions[a] = vertex_collection[a].position;
                weights.AddRange(vertex_collection[a].weights);
                weight_counts[a] = vertex_collection[a].weight_count;
            }

            dst_mesh.SetVertices(positions);
            dst_mesh.SetNormals(normals);
            dst_mesh.triangles = tris;
            dst_mesh.SetBoneWeights(
                new NativeArray<byte>(weight_counts, Allocator.None),
                new NativeArray<BoneWeight1>(weights.ToArray(), Allocator.None));
            dst_mesh.name = $"(opr) {src_mesh.name}";
            
            GetComponent<SkinnedMeshRenderer>().sharedMesh = dst_mesh;
        }
        else //Non-skinned mesh renderer
        {
            
            MeshFilter src_renderer = m_srcMeshObject.GetComponent<MeshFilter>();

            Mesh src_mesh = src_renderer.sharedMesh;
            Mesh dst_mesh;

            List<VertexData> vertex_collection = new List<VertexData>();

            for (int a = 0; a < src_mesh.vertexCount; a++) //Collect vertex data
            {
                bool add_new = true;
                for (int b = 0; b < vertex_collection.Count; b++)
                {
                    if (Vector3.Distance(vertex_collection[b].position, src_mesh.vertices[a]) <=
                        m_weldThreshold) //Add to existing vertex entry
                    {
                        vertex_collection[b].indices.Add(a);
                        vertex_collection[b].normals.Add(src_mesh.normals[a]);
                        add_new = false;
                        break;
                    }
                }

                if (add_new) //Create new vertex entry
                {
                    VertexData entry = new VertexData();
                    entry.position = src_mesh.vertices[a];
                    entry.indices.Add(a);
                    entry.normals.Add(src_mesh.normals[a]);

                    vertex_collection.Add(entry);
                }
            }

            int[] tris = new int[src_mesh.triangles.Length];
            for (int a = 0; a < tris.Length; a++)//Tris
            {
                for (int b = 0; b < vertex_collection.Count; b++)//Lookup in new vertex list
                {
                    if (vertex_collection[b].ContainsIndex(src_mesh.triangles[a]))
                    {
                        tris[a] = b;
                        break;
                    }
                }
            }
            

            dst_mesh = new Mesh();
            Vector3[] normals = new Vector3[vertex_collection.Count];
            Vector3[] positions = new Vector3[vertex_collection.Count];

            for (int a = 0; a < vertex_collection.Count; a++)
            {
                normals[a] = vertex_collection[a].GetSharedNormal();
                positions[a] = vertex_collection[a].position;
            }

            dst_mesh.SetVertices(positions);
            dst_mesh.SetNormals(normals);
            dst_mesh.triangles = tris;
            dst_mesh.name = $"(opr) {src_mesh.name}";

            GetComponent<MeshFilter>().sharedMesh = dst_mesh;
        }

        transform.position = m_srcMeshObject.transform.position;
    }

    private class VertexData
    {
        public Vector3 position;
        public List<Vector3> normals = new List<Vector3>();
        public List<int> indices = new List<int>();
        public BoneWeight1[] weights;
        public byte weight_count;

        public bool ContainsIndex(int index) => indices.Contains(index);

        public Vector3 GetSharedNormal()
        {
            Vector3 output_normal = Vector3.zero; //Not normalized, required for consistent outline thickness

            for (int a = 0; a < normals.Count; a++)
            {
                output_normal += normals[a];
            }

            return output_normal;
        }
    }
}