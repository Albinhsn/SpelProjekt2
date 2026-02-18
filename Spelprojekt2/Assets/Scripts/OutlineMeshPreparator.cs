
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class OutlineMeshPreparator : MonoBehaviour
{
    [SerializeField] private GameObject m_srcMeshObject;
    [SerializeField] private float m_weldThreshold;

    private void OnValidate()
    {
        if (!m_srcMeshObject.TryGetComponent(out SkinnedMeshRenderer smr) &&
            !m_srcMeshObject.TryGetComponent(out MeshRenderer mr))
        {
            Debug.LogError("Outline mesh source object does not contain a MeshRenderer or SkinnedMeshRenderer", m_srcMeshObject);
        }
        
        CalculateMesh();
    }

    private void Awake()
    {
        CalculateMesh();
    }

    private void CalculateMesh()
    {
        SkinnedMeshRenderer src_renderer = m_srcMeshObject.GetComponent<SkinnedMeshRenderer>();

        Mesh src_mesh = src_renderer.sharedMesh;
        Mesh dst_mesh;

        List<VertexData> vertex_collection = new List<VertexData>();

        for (int a = 0; a < src_mesh.vertexCount; a++)//Collect vertex data
        {
            bool add_new = true;
            for (int b = 0; b < vertex_collection.Count; b++)
            {
                if (Vector3.Distance(vertex_collection[b].position, src_mesh.vertices[a]) <= m_weldThreshold)//Add to existing vertex entry
                {
                    vertex_collection[b].indices.Add(a);
                    vertex_collection[b].normals.Add(src_mesh.normals[b]);
                    add_new = false;
                    break;
                }
            }
            if (add_new)//Create new vertex entry
            {
                VertexData entry = new VertexData();
                entry.position = src_mesh.vertices[a];
                entry.indices.Add(a);
                entry.normals.Add(src_mesh.normals[a]);
                entry.weight = src_mesh.boneWeights[a];
                entry.weight_count = src_mesh.weight[a];
                vertex_collection.Add(entry);
            }
        }

        dst_mesh = new Mesh();
        Vector3[] normals = new Vector3[vertex_collection.Count];
        Vector3[] positions = new Vector3[vertex_collection.Count];
        BoneWeight[] weights = new BoneWeight[vertex_collection.Count];
        byte[] weight_counts = new Byte[vertex_collection.Count];

        for (int a = 0; a < vertex_collection.Count; a++)
        {
            normals[a] = vertex_collection[a].GetSharedNormal();
            positions[a] = vertex_collection[a].position;
            weights[a] = vertex_collection[a].weight;
            weight_counts[a] = 4;
        }

        dst_mesh.SetVertices(positions);
        dst_mesh.SetNormals(normals);
        dst_mesh.SetBoneWeights(new NativeArray<byte>(weight_counts, Allocator.None), new NativeArray<BoneWeight1>(weights, Allocator.None)); //TODO: figure out how to use BoneWeight1

    }

    private class VertexData
    {
        public Vector3 position;
        public List<Vector3> normals = new List<Vector3>();
        public List<int> indices = new List<int>();
        public BoneWeight weight;
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