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

    public float3 Reject(float3 a, float3 b)
    {
        return a - math.project(a,b);
    }


    public void Execute()
    {
        int max_steps   = m_maxStepCount;
        float3 curr     = m_start;
        bool done       = false;

        float3 forward  = m_forward;
        float3 up       = m_up;
        float3 right    = math.cross(forward, up);

        float angle_inc = (2.0f * math.PI) / m_verticesPerStep;

        int vertex_count = 0;
        int index_count  = 0;

        for(int step_iter = 0; !done && step_iter < max_steps; step_iter++)
        {
            float distance_remaining = math.length(curr - m_end);
            float step_size          = math.min(m_maxStepSize, distance_remaining);
            float3 next              = curr + step_size * forward;

            // ah: Compute pos, normal, uv and indices
            int vertex_prior_to_iter = vertex_count;
            for(int i = 0; i < m_verticesPerStep; i++)
            {
                float c, s;
                math.sincos(angle_inc * i, out s, out c);

                float3 normal = s * up + c * right;
                float3 vertex = curr + normal * m_meshRadius;
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
                        index_count += 3;
                    }

                    // ah: Second triangle
                    {
                        if(clockwise)
                        {
                            m_mesh.m_indices[index_count + 0] = curr_vertex;
                            m_mesh.m_indices[index_count + 1] = prev_row_next_vertex;
                            m_mesh.m_indices[index_count + 2] = next_vertex;
                        }
                        else
                        {
                            m_mesh.m_indices[index_count + 0] = curr_vertex;
                            m_mesh.m_indices[index_count + 1] = next_vertex;
                            m_mesh.m_indices[index_count + 2] = prev_row_next_vertex;
                        }
                        index_count += 3;
                    }

                }
                vertex_count++;

            }
            // ah: Update coordinate system and pos

            float3 target_dir = math.normalize(m_end - next);
            float angle_diff  = math.acos(math.dot(target_dir, forward));
            float angle_step  = math.min(1, m_maxAngleChange / angle_diff);

            quaternion q0 = quaternion.LookRotationSafe(forward, up);
            quaternion q1 = quaternion.LookRotationSafe(target_dir, Reject(up, target_dir));
            quaternion q = math.slerp(q0, q1, angle_step);
            float3x3 coord_sys = new float3x3(q);
            right             = coord_sys.c0;
            up                = coord_sys.c1;
            forward           = coord_sys.c2;
            
            done = distance_remaining < m_maxStepSize;
            curr = next;
        }

        m_mesh.m_counts[0] = vertex_count;
        m_mesh.m_counts[1] = index_count;
    }

}

public enum IndicatorKind
{
    None,
    Held,
    ClosestTarget,
    Target,
}

public class PickupItemIndicatorManager : MonoBehaviour
{

    private static PickupItemIndicatorManager I;

    [Header("Materials")]
    [SerializeField]
    private Material m_heldMaterial;

    [SerializeField]
    private Material m_indicatorMaterial;

    [SerializeField]
    private Material m_closestMaterial;

    [Header("Mesh Creation Settings")]
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
    
    private struct IndicatorRequest
    {
        public Vector3 src;
        public Vector3 dst;
        public Vector3 forward;
        public Vector3 up;
        public IndicatorKind kind;
        public IndicatorRequest(Vector3 src, Vector3 forward, Vector3 up, Vector3 dst, IndicatorKind kind)
        {
            this.src = src;
            this.dst = dst;
            this.forward = forward;
            this.up      = up;
            this.kind    = kind;
        }
    }

    private IndicatorRequest[] m_indicatorRequests;
    private uint m_indicatorRequestHead;
    private uint m_indicatorRequestTail;
    private const uint MAX_INDICATOR_REQUESTS = 256;

    void Awake()
    {

        if(I != null && I != this)
        {
            Destroy(this.gameObject);
            return;
        }

        I = this;

        m_indicatorRequestHead = m_indicatorRequestTail = 0;
        m_indicatorRequests    = new IndicatorRequest[MAX_INDICATOR_REQUESTS];

        int vertex_count = m_maxStepCount * m_verticesPerStep;

        // TODO(ah): What is this number?
        int index_count  = (m_maxStepCount - 1) * 6 * m_verticesPerStep;
        m_meshes = new PickupItemMesh[MAX_INDICATOR_REQUESTS];
        for(int i = 0; i < MAX_INDICATOR_REQUESTS; i++)
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

    }

    void OnDestroy()
    {
        for(int i = 0; i < MAX_INDICATOR_REQUESTS; i++)
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

    void OnDisable()
    {
        for(int i = 0; i < MAX_INDICATOR_REQUESTS; i++)
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

    public static void Request(Vector3 src, Vector3 forward, Vector3 up, Vector3 dst, IndicatorKind kind)
    {
        uint index = I.m_indicatorRequestHead++;
        if(I.m_indicatorRequestTail + MAX_INDICATOR_REQUESTS <= index)
        {
            Debug.LogError("Above limit for max target requests");
        }
        I.m_indicatorRequests[index % MAX_INDICATOR_REQUESTS] = new(src, forward, up, dst, kind);
    }

    void LateUpdate()
    {

        uint tail = m_indicatorRequestTail;
        uint head = m_indicatorRequestHead;

        uint job_count = head - tail;
        job_count      = job_count > MAX_INDICATOR_REQUESTS ? MAX_INDICATOR_REQUESTS : job_count;

        if(job_count > 0)
        {
            // ah: Create mesh jobs
            NativeArray<JobHandle> handles = new NativeArray<JobHandle>((int)job_count, Allocator.Temp);
            for(int i = 0; i < job_count; i++)
            {
                IndicatorRequest req = m_indicatorRequests[(i + tail) % MAX_INDICATOR_REQUESTS];
                CreatePickupItemMeshJob job = new CreatePickupItemMeshJob
                {
                    m_maxStepSize     = m_maxStepSize,
                    m_maxAngleChange  = m_maxAngleChange,
                    m_meshRadius      = m_meshRadius,
                    m_start           = req.src,
                    m_end             = req.dst,
                    m_mesh            = m_meshes[i],
                    m_maxStepCount    = m_maxStepCount,
                    m_verticesPerStep = m_verticesPerStep,
                    m_forward         = req.forward,
                    m_up              = req.up,
                };

                handles[i]           = job.Schedule();
            }

            // ah: complete jobs
            for(int i = 0; i < job_count; i++)
            {
                handles[i].Complete();
            }

            // ah: Create submeshes
            Mesh combined_mesh = new();
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
                int vertex_offset = 0;
                for(int i = 0; i < job_count; i++)
                {
                    PickupItemMesh mesh = m_meshes[i];
                    NativeArray<float3>.Copy(mesh.m_vertices, 0, vertices, vertex_offset,  mesh.m_counts[0]);
                    NativeArray<float3>.Copy(mesh.m_normals, 0, normals, vertex_offset, mesh.m_counts[0]);
                    NativeArray<float2>.Copy(mesh.m_uvs, 0, uvs, vertex_offset, mesh.m_counts[0]);

                    vertex_offset += mesh.m_counts[0];
                }

                combined_mesh.subMeshCount = (int)job_count;
                combined_mesh.SetVertices(vertices);
                combined_mesh.SetUVs(0, uvs);
                combined_mesh.SetNormals(normals);

                vertex_offset = 0;
                for(int i = 0; i < job_count; i++)
                {
                    PickupItemMesh mesh = m_meshes[i];
                    int index_count     = mesh.m_counts[1];
                    NativeArray<int> indices     = new NativeArray<int>(index_count, Allocator.Temp);
                    NativeArray<int>.Copy(mesh.m_indices, 0, indices, 0, index_count);

                    combined_mesh.SetIndices(indices, MeshTopology.Triangles, i, true, vertex_offset);
                    vertex_offset += mesh.m_counts[0];
                }
                combined_mesh.RecalculateTangents();
                combined_mesh.RecalculateBounds();
                combined_mesh.UploadMeshData(true);
                
            }

            // ah: Render
            {
                for(int i = 0; i < job_count; i++)
                {
                    IndicatorRequest req = m_indicatorRequests[(i + tail) % MAX_INDICATOR_REQUESTS];
                    Material mat;
                    switch(req.kind)
                    {
                        case IndicatorKind.Held: mat = m_heldMaterial; break;
                        case IndicatorKind.ClosestTarget: mat = m_closestMaterial; break;
                        case IndicatorKind.Target: mat = m_indicatorMaterial; break;
                        default:mat = null; break;
                    }
                    if(mat != null)
                    {
                        RenderParams rp = new(mat);
                        rp.worldBounds  = new (Vector3.zero, Vector3.one * 10000);

                        Graphics.RenderMeshPrimitives(rp, combined_mesh, i);
                    }
                }

            }

            // Reset request count
        }
        else
        {
        }
        m_indicatorRequestTail = m_indicatorRequestHead;

    }

}
