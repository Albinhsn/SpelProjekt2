using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public struct PickupItemMesh
{
    public NativeArray<float3> m_vertices;
    public NativeArray<float3> m_normals;
    public NativeArray<int>     m_indices;
    public NativeArray<int>     m_counts;
    public int m_maxStepCount;
    public int m_verticesPerStep;
}

[BurstCompile]
public struct CreatePickupItemMeshJob : IJob
{
    public float m_maxStepSize;
    public float m_maxAngleChange;
    public float m_meshRadius;

    public float3 m_start;
    public float3 m_end;
    
    public PickupItemMesh m_mesh;

    public void Execute()
    {
        int max_steps   = m_mesh.m_maxStepCount;
        float3 curr     = m_start;
        bool done       = false;

        for(int i = 0; !m_done && i < max_steps; i++)
        {
            float3 dir               = m_end - m_start;
            float distance_remaining = math.length(curr - m_end);
            done                   = distance_remaining < m_maxStepSize;

            float step_size          = math.min(m_maxStepSize, distance_remaining);

            float3 next = curr + step_size * dir;

            // TODO(ah): Compute vertex positions, indices and normals

        }
    }

}

public class PickupItemIndicatorManager : MonoBehaviour
{
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
    private float m_maxStepCount;

    [SerializeField]
    [Range(3, 12)]
    private int m_verticesPerStep = 3;

    private PickupItemMesh m_mesh;

    [SerializeField]
    private Material m_indicatorMaterial;

    private List<Vector3> m_targetRequests;

    void Awake()
    {
        int vertex_count = m_maxStepCount * m_verticesPerStep;

        // TODO(ah): What is this number?
        int index_count  = vertex_count * 4;
        m_mesh = new PickupItemMesh
        {
            m_vertices = new NativeArray<float3>(Allocator.Persistent, vertex_count),
            m_normals  = new NativeArray<float3>(Allocator.Persistent, vertex_count),
            m_indices  = new NativeArray<int>(Allocator.Persistent, max_index_count),
            m_counts = new NativeArray<int>(Allocator.Persistent, 2),
            m_maxStepCount = m_maxStepCount,
            m_verticesPerStep = m_verticesPerStep,
        };

    }

    void OnDisable()
    {
        m_mesh.m_vertices?.Dispose();
        m_mesh.m_normals?.Dispose();
        m_mesh.m_indices?.Dispose();
        m_mesh.m_counts?.Dispose();
    }

    void OnEnable()
    {
        Awake();
    }

    public static void Request(Vector3 target)
    {

    }

    void LateUpdate()
    {
        // TODO(ah): Compute new mesh
        CreatePickupItemMeshJob job = new CreatePickupItemMeshJob
        {
            m_maxStepSize = m_maxStepSize,
            m_maxAngleChange = m_maxAngleChange,
            m_meshRadius = m_meshRadius,
            m_start = this.transform.position,
            m_end = m_target.position,
            m_mesh = m_mesh,
        };
    
        var handle = job.Schedule();
        handle.Complete();

        // TODO(ah): Create render params

        // TODO(ah): create a large mesh from all PickupItemMesh data

        // TODO(ah): Create a command buffer for rendering all meshes

        // TODO(ah): Render meshes with Graphics.RenderMeshIndirect
    }

}
