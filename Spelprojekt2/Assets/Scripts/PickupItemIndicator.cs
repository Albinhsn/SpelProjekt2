using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public struct PickupItemMesh
{
    public NativeArray<float3> m_vertices;
    public NativeArray<float3> m_normals;
    public NativeArray<float2> m_uvs;
    public NativeArray<int>    m_indices;
    public NativeArray<int>    m_counts;
}

[BurstCompile]
public struct CreatePickupItemMeshJob : IJob
{
    public float m_maxStepSize;
    public float m_maxAngleChange;
    public float m_meshRadius;

    public float3 m_start;
    public float3 m_end;

    public float3 m_forward;
    public float3 m_up;
    
    public PickupItemMesh m_mesh;

    public int m_maxStepCount;
    public int m_verticesPerStep;


    public void Execute()
    {
        int max_steps   = m_maxStepCount;
        float3 curr     = m_start;
        bool done       = false;

        float3 right    = math.cross(m_forward, m_up);

        float angle_inc = (2.0f * math.PI) / m_verticesPerStep;

        int vertex_count = 0;
        int index_count  = 0;

        for(int step_iter = 0; !done && step_iter < max_steps; step_iter++)
        {
            float3 dir               = m_end - m_start;
            float distance_remaining = math.length(curr - m_end);
            float step_size          = math.min(m_maxStepSize, distance_remaining);
            float3 next              = curr + step_size * dir;

            // ah: Compute pos, normal, uv and indices
            int vertex_prior_to_iter = vertex_count;
            for(int i = 0; i < m_verticesPerStep; i++)
            {
                float c, s;
                math.sincos(angle_inc * i, out s, out c);

                float3 normal = s * m_up + c * right;
                float3 vertex = curr + normal;
                float2 uv = new float2(
                        i / (float)m_verticesPerStep,
                        step_iter / (float)max_steps
                        );
                m_mesh.m_vertices[vertex_count] = vertex;
                m_mesh.m_normals[vertex_count]  = normal;
                m_mesh.m_uvs[vertex_count]      = uv;

                if(step_iter > 0)
                {

                    // ah: create two triangles with the next vertex of the same row
                    // and the symmetrical two on the previous row
                    int curr_vertex = vertex_count; 
                    int next_vertex = (i == m_verticesPerStep - 1) ? vertex_prior_to_iter : vertex_count + 1;

                    int prev_row_same_vertex = vertex_count - m_verticesPerStep;
                    int prev_row_next_vertex = (i == m_verticesPerStep - 1) ? vertex_prior_to_iter - m_verticesPerStep : vertex_count - m_verticesPerStep + 1;

                    // ah: First triangle
                    bool clockwise;
                    {
                        float3 v0 = vertex;
                        float3 v1 = m_mesh.m_vertices[prev_row_same_vertex];
                        float3 v2 = m_mesh.m_vertices[prev_row_next_vertex];

                        float3 n = math.cross(v1 - v0, v2 - v0);
                        clockwise = math.dot(n, normal) > 0;

                        if(clockwise)
                        {
                            m_mesh.m_indices[index_count + 0] = curr_vertex;
                            m_mesh.m_indices[index_count + 1] = prev_row_same_vertex;
                            m_mesh.m_indices[index_count + 2] = prev_row_next_vertex;
                        }
                        else
                        {
                            m_mesh.m_indices[index_count + 0] = curr_vertex;
                            m_mesh.m_indices[index_count + 1] = prev_row_next_vertex;
                            m_mesh.m_indices[index_count + 2] = prev_row_same_vertex;
                        }
                    }

                    // ah: Second triangle
                    {
                        if(clockwise)
                        {
                            m_mesh.m_indices[index_count + 0] = curr_vertex;
                            m_mesh.m_indices[index_count + 1] = next_vertex;
                            m_mesh.m_indices[index_count + 2] = prev_row_next_vertex;
                        }
                        else
                        {
                            m_mesh.m_indices[index_count + 0] = curr_vertex;
                            m_mesh.m_indices[index_count + 1] = prev_row_next_vertex;
                            m_mesh.m_indices[index_count + 1] = next_vertex;
                        }
                    }
                }
                vertex_count++;

            }
            // ah: Update coordinate system and pos
            
            done = distance_remaining < m_maxStepSize;
            curr = next;
        }

        m_mesh.m_counts[0] = vertex_count;
        m_mesh.m_counts[1] = index_count;
    }

}

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PickupItemIndicatorManager : MonoBehaviour
{

    private static PickupItemIndicatorManager I;

    
    [SerializeField]
    private Transform m_target;

    [SerializeField]
    private float m_maxStepSize;

    [SerializeField]
    private float m_maxAngleChange;

    [SerializeField]
    private float m_meshRadius;

    [SerializeField]
    [Range(16, 512)]
    private int m_maxStepCount;

    [SerializeField]
    [Range(3, 12)]
    private int m_verticesPerStep = 3;

    private PickupItemMesh[] m_meshes;

    [SerializeField]
    private Material m_indicatorMaterial;

    private Vector3[] m_targetRequests;
    private uint m_targetRequestHead;
    private uint m_targetRequestTail;
    private const uint MAX_TARGET_REQUESTS = 256;

    private MeshRenderer m_rend;
    private MeshFilter   m_filter;

    void Awake()
    {

        if(I != null && I != this)
        {
            Destroy(this.gameObject);
            return;
        }

        m_targetRequestHead = m_targetRequestTail = 0;
        m_targetRequests    = new Vector3[MAX_TARGET_REQUESTS];

        int vertex_count = m_maxStepCount * m_verticesPerStep;

        // TODO(ah): What is this number?
        int index_count  = vertex_count * 4;
        for(int i = 0; i < MAX_TARGET_REQUESTS; i++)
        {
            m_meshes[i] = new PickupItemMesh
            {
                m_vertices        = new NativeArray<float3>(vertex_count, Allocator.Persistent),
                m_normals         = new NativeArray<float3>(vertex_count, Allocator.Persistent),
                m_uvs             = new NativeArray<float2>(vertex_count, Allocator.Persistent),
                m_indices         = new NativeArray<int>(index_count, Allocator.Persistent),
                m_counts          = new NativeArray<int>(2, Allocator.Persistent),
            };
        }

        m_rend   = GetComponent<MeshRenderer>();
        m_filter = GetComponent<MeshFilter>();

    }

    void OnDisable()
    {
        for(int i = 0; i < MAX_TARGET_REQUESTS; i++)
        {
            if(m_meshes[i].m_vertices.IsCreated)
            {
                m_meshes[i].m_vertices.Dispose();
                m_meshes[i].m_normals.Dispose();
                m_meshes[i].m_uvs.Dispose();
                m_meshes[i].m_indices.Dispose();
                m_meshes[i].m_counts.Dispose();
            }
        }
    }

    void OnEnable()
    {
        Awake();
    }

    public static void Request(Vector3 target)
    {
        uint index = I.m_targetRequestHead++;
        if(I.m_targetRequestTail + MAX_TARGET_REQUESTS <= index)
        {
            Debug.LogError("Above limit for max target requests");
        }
        I.m_targetRequests[index % MAX_TARGET_REQUESTS] = target;
    }

    void LateUpdate()
    {

        uint tail = m_targetRequestTail;
        uint head = m_targetRequestHead;

        uint job_count = head - tail;
        job_count      = job_count > MAX_TARGET_REQUESTS ? MAX_TARGET_REQUESTS : job_count;

        if(job_count > 0)
        {
            // ah: Create mesh jobs
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>((int)job_count, Allocator.Temp);
            for(int i = 0; i < job_count; i++)
            {
                Vector3 target              = m_targetRequests[(i + tail) % MAX_TARGET_REQUESTS];
                CreatePickupItemMeshJob job = new CreatePickupItemMeshJob
                {
                    m_maxStepSize     = m_maxStepSize,
                    m_maxAngleChange  = m_maxAngleChange,
                    m_meshRadius      = m_meshRadius,
                    m_start           = this.transform.position,
                    m_end             = target,
                    m_mesh            = m_meshes[i],
                    m_maxStepCount    = m_maxStepCount,
                    m_verticesPerStep = m_verticesPerStep,
                    m_forward         = this.transform.forward,
                    m_up              = this.transform.up,
                };

                handles[i]           = job.Schedule();
            }

            // ah: complete jobs
            for(int i = 0; i < job_count; i++)
            {
                handles[i].Complete();
            }

            // ah: Combine meshes
            {
                int combined_vertex_count = 0;
                int combined_index_count  = 0;
                for(int i = 0; i < job_count; i++)
                {
                    PickupItemMesh item_mesh = m_meshes[i];
                    combined_vertex_count += item_mesh.m_counts[0];
                    combined_index_count  += item_mesh.m_counts[1];
                }

                NativeArray<float3> vertices = new NativeArray<float3>(combined_vertex_count, Allocator.Temp);
                NativeArray<float3> normals  = new NativeArray<float3>(combined_vertex_count, Allocator.Temp);
                NativeArray<float2> uvs      = new NativeArray<float2>(combined_vertex_count, Allocator.Temp);
                NativeArray<float3> indices  = new NativeArray<float3>(combined_index_count, Allocator.Temp);
                int vertex_offset = 0;
                int index_offset  = 0;
                for(int i = 0; i < job_count; i++)
                {
                    PickupItemMesh mesh = m_meshes[i];
                    NativeArray<float3>.Copy(vertices, vertex_offset, mesh.m_vertices, 0, mesh.m_counts[0]);
                    NativeArray<float3>.Copy(normals, vertex_offset, mesh.m_normals, 0, mesh.m_counts[0]);
                    NativeArray<float2>.Copy(uvs, vertex_offset, mesh.m_uvs, 0, mesh.m_counts[0]);

                    int index_count = mesh.m_counts[1];
                    for(int j = 0; j < index_count; j++)
                    {
                        indices[index_offset + j] = mesh.m_indices[j] = index_offset;
                    }

                    vertex_offset += mesh.m_counts[0];
                    index_offset  += index_count;

                }

                Mesh combined_mesh = new Mesh();

                combined_mesh.SetVertices(vertices);
                combined_mesh.SetUVs(0, uvs);
                combined_mesh.SetNormals(normals);
                combined_mesh.SetIndices(indices,MeshTopology.Triangles, 0, false, 0);
                combined_mesh.RecalculateTangents();

                m_filter.mesh = combined_mesh;
            }

            // Reset request count
            m_targetRequestTail = m_targetRequestHead;
        }

    }

}
