using System;
using JetBrains.Annotations;
using srUtils.Unity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Interaction
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private InstanceSet m_interactableSet;
        [SerializeField] private float m_range;
        [SerializeField] private float m_coneBaseRad;
        [SerializeField] private float m_coneFactor;
        
        [SerializeField] private InputActionReference m_interactAction;

        [CanBeNull] private Interactable m_selected = null;

        private void Update()
        {
            SearchFrustum();

            if (m_selected is not null && m_interactAction.action.WasPressedThisFrame())
            {
                m_selected.Interact(this);
            }
            
        }

        private void SearchFrustum()
        {
            Interactable? sel = null;
            float sel_distance = float.MaxValue;
            
            foreach (Interactable obj in m_interactableSet.GetEnumerable())
            {
                Vector3 obj_relative_position = obj.position - transform.position;
                float obj_linear_distance = Vector3.Dot(obj_relative_position, transform.forward);
                
                if(obj_linear_distance < 0 || obj_linear_distance > m_range) continue; //Out of range

                float obj_radial_distance = (obj_relative_position - transform.forward * obj_linear_distance).magnitude;
                if (obj_radial_distance > obj_linear_distance * m_coneFactor + m_coneBaseRad) continue; //Out of range

                if (obj_radial_distance < sel_distance)//May be subject to change, test if this, linear or combination feels better
                {
                    sel = obj;
                    sel_distance = obj_radial_distance;
                }
            }

            if (m_selected is not null && sel != m_selected)
            {
                m_selected.SetHighlighted(false);
            }
            m_selected = sel;
            m_selected?.SetHighlighted(true);
        }


        private void OnDrawGizmosSelected()
        {
            const int CONE_VERT_COUNT = 16;
            Gizmos.color = Color.yellow;
            Vector3[] cone_verts = new Vector3[CONE_VERT_COUNT * 4 + 8];
            
            for (int a = 0; a < CONE_VERT_COUNT * 2; a+=2)//Base
            {
                float v = (float)a / 2 / CONE_VERT_COUNT * 6.28318548f;//MathF.Tau apparently doesn't exist in this version
                cone_verts[a] = (new Vector3(
                    m_coneBaseRad * MathF.Cos(v),
                    m_coneBaseRad * MathF.Sin(v),
                    0
                    ));
                v = (float)(a / 2 + 1) / CONE_VERT_COUNT * 6.28318548f;
                cone_verts[a + 1] = (new Vector3(
                    m_coneBaseRad * MathF.Cos(v),
                    m_coneBaseRad * MathF.Sin(v),
                    0
                ));
            }

            float top_rad = m_coneBaseRad + m_coneFactor * m_range;
            for (int a = 0; a < CONE_VERT_COUNT * 2; a+=2)//Top
            {
                float v = (float)a / 2 / CONE_VERT_COUNT * 6.28318548f;
                cone_verts[a + CONE_VERT_COUNT * 2] = (new Vector3(
                    top_rad * MathF.Cos(v),
                    top_rad * MathF.Sin(v),
                    m_range
                ));
                v = (float)(a / 2 + 1) / CONE_VERT_COUNT * 6.28318548f;
                cone_verts[a + 1 + CONE_VERT_COUNT * 2] = (new Vector3(
                    top_rad * MathF.Cos(v),
                    top_rad * MathF.Sin(v),
                    m_range
                ));
            }
            
            
            cone_verts[CONE_VERT_COUNT * 4] = (new Vector3(m_coneBaseRad, 0, 0));
            cone_verts[CONE_VERT_COUNT * 4 + 2] = (new Vector3(0, m_coneBaseRad, 0));
            cone_verts[CONE_VERT_COUNT * 4 + 4] = (new Vector3(-m_coneBaseRad, 0, 0));
            cone_verts[CONE_VERT_COUNT * 4 + 6] = (new Vector3(0, -m_coneBaseRad, 0));
            cone_verts[CONE_VERT_COUNT * 4 + 1] = (new Vector3(top_rad, 0, m_range));
            cone_verts[CONE_VERT_COUNT * 4 + 3] = (new Vector3(0, top_rad, m_range));
            cone_verts[CONE_VERT_COUNT * 4 + 5] = (new Vector3(-top_rad, 0, m_range));
            cone_verts[CONE_VERT_COUNT * 4 + 7] = (new Vector3(0, -top_rad, m_range));

            //Transform
            for (int a = 0; a < CONE_VERT_COUNT*4+8; a++)
            {
                cone_verts[a] = transform.TransformPoint(cone_verts[a]);
            }
            
            Gizmos.DrawLineList(cone_verts);
        }
    }
}