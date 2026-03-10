
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class OutlineMeshPreparator : MonoBehaviour
{
    [SerializeField] private GameObject m_srcMeshObject;
    [SerializeField] private bool m_recalculate;
    [SerializeField] private Material m_material;
    private bool m_useSkinnedMeshRenderer;

    private void OnValidate()
    {
        if (m_recalculate)
        {
            m_recalculate = false;
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
            if (TryGetComponent(out MeshRenderer mr))
            {
                Destroy(mr);
                Destroy(GetComponent<MeshFilter>());
            }
            
            SkinnedMeshRenderer dst_renderer;
            if (!TryGetComponent(out dst_renderer)) dst_renderer = gameObject.AddComponent<SkinnedMeshRenderer>();
            dst_renderer.sharedMesh = smr.sharedMesh;
            dst_renderer.bones = smr.bones;
            dst_renderer.material = m_material;
            dst_renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
        else //Non-skinned mesh renderer
        {
            if (TryGetComponent(out SkinnedMeshRenderer lsmr))
            {
                Destroy(lsmr);
            }
            
            MeshFilter src_mesh = m_srcMeshObject.GetComponent<MeshFilter>();
            MeshFilter dst_mesh;
            if(!TryGetComponent(out dst_mesh)) dst_mesh=gameObject.AddComponent<MeshFilter>();
            dst_mesh.sharedMesh = src_mesh.sharedMesh;
            MeshRenderer dst_renderer;
            if(!TryGetComponent(out dst_renderer)) dst_renderer = gameObject.AddComponent<MeshRenderer>();
            dst_renderer.material = m_material;
            dst_renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        transform.position = m_srcMeshObject.transform.position;
    }
}